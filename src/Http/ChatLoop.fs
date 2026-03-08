namespace MCPReference.Http

module ChatLoop =
    open System
    open System.Net.Http
    open System.Net.Http.Headers
    open System.Text
    open System.Text.Json
    open System.Text.Json.Nodes
    open System.Threading.Tasks
    open MCPReference.Core

    type ChatRequest = { Prompt: string }
    type ChatResponse = { Reply: string }

    let private http = new HttpClient()

    let private getEnv (name: string) (fallback: string) =
        match Environment.GetEnvironmentVariable(name) with
        | null
        | "" -> fallback
        | v -> v

    let private buildToolsNode () : JsonArray =
        let tools = JsonArray()

        for t in Registry.listLlmDescriptors() do
            let parameters = JsonNode.Parse(t.ParametersJson)
            let toolNode =
                JsonSerializer.SerializeToNode(
                    {| ``type`` = "function"
                       ``function`` =
                        {| name = t.Name
                           description = t.Description
                           parameters = parameters |} |}
                )

            tools.Add(toolNode)

        tools

    let private invokeTool (toolName: string) (argumentsJson: option<JsonElement>) : string =
        let argObj =
            match argumentsJson with
            | Some v -> box v
            | None -> box ""

        match Registry.tryInvoke toolName argObj with
        | Some res ->
            match res with
            | :? string as s -> s
            | :? int as i -> string i
            | :? MCPReference.Core.Protocol.Message as m -> m.Text
            | _ -> JsonSerializer.Serialize(res)
        | None -> sprintf "Tool '%s' not found." toolName

    let private postChatCompletion (messages: JsonArray) : Task<Result<string, string>> =
        task {
            let apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            if String.IsNullOrWhiteSpace(apiKey) then
                return Error "Missing OPENAI_API_KEY"
            else
                let endpoint = getEnv "OPENAI_BASE_URL" "https://api.openai.com/v1/chat/completions"
                let model = getEnv "OPENAI_MODEL" "gpt-4.1-mini"
                let toolsNode = buildToolsNode ()

                let payloadNode : JsonNode =
                    JsonSerializer.SerializeToNode(
                        {| model = model
                           messages = messages
                           tools = toolsNode
                           tool_choice = "auto" |}
                    )

                use req = new HttpRequestMessage(HttpMethod.Post, endpoint)
                req.Headers.Authorization <- AuthenticationHeaderValue("Bearer", apiKey)
                req.Content <- new StringContent(payloadNode.ToJsonString(), Encoding.UTF8, "application/json")

                use! resp = http.SendAsync(req)
                let! body = resp.Content.ReadAsStringAsync()

                if not resp.IsSuccessStatusCode then
                    return Error (sprintf "LLM request failed: %d %s" (int resp.StatusCode) body)
                else
                    return Ok body
        }

    let private parseToolArguments (raw: string) : option<JsonElement> =
        if String.IsNullOrWhiteSpace(raw) then
            None
        else
            try
                use doc = JsonDocument.Parse(raw)
                Some (doc.RootElement.Clone())
            with _ ->
                None

    let run (prompt: string) : Task<string> =
        task {
            let messages = JsonArray()

            messages.Add(JsonSerializer.SerializeToNode({| role = "system"; content = "You are a concise assistant. Use tools when useful. Tool names and parameter schemas are authoritative." |}))
            messages.Add(JsonSerializer.SerializeToNode({| role = "user"; content = prompt |}))

            let mutable answer = ""
            let mutable doneLoop = false
            let mutable turns = 0

            while not doneLoop && turns < 6 do
                turns <- turns + 1
                let! llm = postChatCompletion messages

                match llm with
                | Error e ->
                    answer <- e
                    doneLoop <- true
                | Ok raw ->
                    try
                        use doc = JsonDocument.Parse(raw)
                        let root = doc.RootElement
                        let msg = root.GetProperty("choices").[0].GetProperty("message")

                        let mutable contentProp = Unchecked.defaultof<JsonElement>
                        let content =
                            if msg.TryGetProperty("content", &contentProp) && contentProp.ValueKind = JsonValueKind.String then
                                contentProp.GetString()
                            else
                                ""

                        let mutable toolCallsProp = Unchecked.defaultof<JsonElement>
                        let hasToolCalls =
                            msg.TryGetProperty("tool_calls", &toolCallsProp)
                            && toolCallsProp.ValueKind = JsonValueKind.Array
                            && toolCallsProp.GetArrayLength() > 0

                        if hasToolCalls then
                            let toolCallsNode = JsonNode.Parse(toolCallsProp.GetRawText())
                            messages.Add(JsonSerializer.SerializeToNode({| role = "assistant"; content = content; tool_calls = toolCallsNode |}))

                            for tc in toolCallsProp.EnumerateArray() do
                                let mutable idProp = Unchecked.defaultof<JsonElement>
                                let id =
                                    if tc.TryGetProperty("id", &idProp) && idProp.ValueKind = JsonValueKind.String then
                                        idProp.GetString()
                                    else
                                        ""

                                let mutable fnProp = Unchecked.defaultof<JsonElement>
                                if tc.TryGetProperty("function", &fnProp) then
                                    let mutable fnNameProp = Unchecked.defaultof<JsonElement>
                                    let mutable fnArgsProp = Unchecked.defaultof<JsonElement>

                                    let fnName =
                                        if fnProp.TryGetProperty("name", &fnNameProp) && fnNameProp.ValueKind = JsonValueKind.String then
                                            fnNameProp.GetString()
                                        else
                                            ""

                                    let fnArgsRaw =
                                        if fnProp.TryGetProperty("arguments", &fnArgsProp) && fnArgsProp.ValueKind = JsonValueKind.String then
                                            fnArgsProp.GetString()
                                        else
                                            ""

                                    let toolResult =
                                        match parseToolArguments fnArgsRaw with
                                        | Some toolArgs -> invokeTool fnName (Some toolArgs)
                                        | None -> sprintf "Tool '%s' arguments could not be parsed." fnName

                                    messages.Add(JsonSerializer.SerializeToNode({| role = "tool"; tool_call_id = id; content = toolResult |}))
                        else
                            answer <-
                                if String.IsNullOrWhiteSpace(content) then
                                    "No assistant reply was produced."
                                else
                                    content
                            doneLoop <- true
                    with ex ->
                        answer <- sprintf "Failed to parse LLM response: %s" ex.Message
                        doneLoop <- true

            return
                if String.IsNullOrWhiteSpace(answer) then
                    "No response."
                else
                    answer
        }
