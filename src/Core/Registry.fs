namespace MCPReference.Core

module Registry =
    open System
    open MCPReference.Core.Protocol
    open System.Text.Json

    type Tool =
        { Name: string
          Description: string
          Invoke: obj -> obj }

    let mutable private tools : Map<string, Tool> = Map.empty

    let register (t: Tool) =
        tools <- tools.Add(t.Name, t)

    let listTools () =
        tools |> Map.toList |> List.map (fun (_, t) -> t.Name) |> Array.ofList

    type ToolDescriptor = { Name: string; Description: string }

    let listDescriptors () : ToolDescriptor [] =
        tools |> Map.toList |> List.map (fun (_, t) -> { Name = t.Name; Description = t.Description }) |> Array.ofList

    let tryInvoke (name: string) (arg: obj) : option<obj> =
        match tools.TryFind name with
        | Some t -> Some (t.Invoke arg)
        | None -> None

    let registerDefaults () =
        // Register Echo tool (accepts Message or JSON { Text: string })
        let echoTool =
            { Name = "Echo"
              Description = "Echo tool - echoes messages"
              Invoke = fun o ->
                match o with
                | :? Message as m -> box (Echo.echo m)
                | :? JsonElement as je ->
                    if je.ValueKind = JsonValueKind.String then
                        let txt = je.GetString()
                        box (({ Id = 0; Text = txt } : Message) |> Echo.echo)
                    else if je.ValueKind = JsonValueKind.Object then
                        let mutable v = Unchecked.defaultof<JsonElement>
                        let ok = je.TryGetProperty("Text", &v)
                        if ok && v.ValueKind = JsonValueKind.String then
                            let txt = v.GetString()
                            box (({ Id = 0; Text = txt } : Message) |> Echo.echo)
                        else box ({ Id = 0; Text = "" } : Message)
                    else box ({ Id = 0; Text = "" } : Message)
                | _ -> box ({ Id = 0; Text = "" } : Message) }
        register echoTool

        // Register Add tool (accepts JSON { A:int, B:int })
        let addTool =
            { Name = "Add"
              Description = "Add two integers (A + B)"
              Invoke = fun o ->
                match o with
                | :? JsonElement as je when je.ValueKind = JsonValueKind.Object ->
                    let mutable aProp = Unchecked.defaultof<JsonElement>
                    let mutable bProp = Unchecked.defaultof<JsonElement>
                    let a = if je.TryGetProperty("A", &aProp) then aProp.GetInt32() else 0
                    let b = if je.TryGetProperty("B", &bProp) then bProp.GetInt32() else 0
                    box (a + b)
                | _ -> box 0 }
        register addTool
