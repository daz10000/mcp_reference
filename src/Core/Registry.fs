namespace MCPReference.Core

module Registry =
    open System
    open System.Text.Json
    open MCPReference.Core.Protocol

    type Tool =
        { Name: string
          Description: string
          ParametersJson: string
          Invoke: obj -> obj }

    type ToolDescriptor =
        { Name: string
          Description: string }

    type LlmToolDescriptor =
        { Name: string
          Description: string
          ParametersJson: string }

    let mutable private tools : Map<string, Tool> = Map.empty

    let private tryGetPropertyInsensitive (je: JsonElement) (name: string) : option<JsonElement> =
        if je.ValueKind <> JsonValueKind.Object then
            None
        else
            je.EnumerateObject()
            |> Seq.tryFind (fun p -> String.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase))
            |> Option.map (fun p -> p.Value)

    let register (t: Tool) =
        tools <- tools.Add(t.Name, t)

    let listTools () =
        tools |> Map.toList |> List.map (fun (_, t) -> t.Name) |> Array.ofList

    let listDescriptors () : ToolDescriptor [] =
        tools
        |> Map.toList
        |> List.map (fun (_, t) -> { Name = t.Name; Description = t.Description })
        |> Array.ofList

    let listLlmDescriptors () : LlmToolDescriptor [] =
        tools
        |> Map.toList
        |> List.map (fun (_, t) ->
            { Name = t.Name
              Description = t.Description
              ParametersJson = t.ParametersJson })
        |> Array.ofList

    let tryInvoke (name: string) (arg: obj) : option<obj> =
        match tools.TryFind name with
        | Some t -> Some (t.Invoke arg)
        | None -> None

    let registerDefaults () =
        let echoTool =
            { Name = "Echo"
              Description = "Echo tool - echoes messages"
              ParametersJson = "{ \"type\": \"object\", \"properties\": { \"text\": { \"type\": \"string\", \"description\": \"Text to echo\" } }, \"required\": [\"text\"] }"
              Invoke =
                fun o ->
                    match o with
                    | :? Message as m -> box (Echo.echo m)
                    | :? JsonElement as je ->
                        if je.ValueKind = JsonValueKind.String then
                            box (({ Id = 0; Text = je.GetString() } : Message) |> Echo.echo)
                        else if je.ValueKind = JsonValueKind.Object then
                            match tryGetPropertyInsensitive je "Text" with
                            | Some v when v.ValueKind = JsonValueKind.String ->
                                box (({ Id = 0; Text = v.GetString() } : Message) |> Echo.echo)
                            | _ -> box ({ Id = 0; Text = "" } : Message)
                        else
                            box ({ Id = 0; Text = "" } : Message)
                    | _ -> box ({ Id = 0; Text = "" } : Message) }

        register echoTool

        let addTool =
            { Name = "Add"
              Description = "Add two integers (A + B)"
              ParametersJson = "{ \"type\": \"object\", \"properties\": { \"a\": { \"type\": \"integer\", \"description\": \"First number\" }, \"b\": { \"type\": \"integer\", \"description\": \"Second number\" } }, \"required\": [\"a\", \"b\"] }"
              Invoke =
                fun o ->
                    match o with
                    | :? JsonElement as je when je.ValueKind = JsonValueKind.Object ->
                        let parseIntFromProperty propName =
                            match tryGetPropertyInsensitive je propName with
                            | Some p when p.ValueKind = JsonValueKind.Number -> p.GetInt32()
                            | Some p when p.ValueKind = JsonValueKind.String ->
                                match Int32.TryParse(p.GetString()) with
                                | true, value -> value
                                | _ -> 0
                            | _ -> 0

                        let a = parseIntFromProperty "A"
                        let b = parseIntFromProperty "B"
                        box (a + b)
                    | _ -> box 0 }

        register addTool
