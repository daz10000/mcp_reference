namespace MCPReference.Core

open System.ComponentModel
open ModelContextProtocol.Server
open MCPReference.Core.TypedTools
open System

[<McpServerToolType>]
type McpTools =
    [<McpServerTool; Description("Echoes a string back to the caller.")>]
    static member Echo(text: string) : string =
        echo { Text = text }

    [<McpServerTool; Description("Adds two integers and returns the sum.")>]
    static member Add(a: int, b: int) : int =
        add { A = a; B = b }

    [<McpServerTool; Description("Adds two integers and returns the sum. The second integer is optional and defaults to 42 if not provided.")>]
    static member AddOptional1(a: int, ?b: int) : int =
        let bFinal = defaultArg b 42
        add { A = a; B = bFinal }

    [<McpServerTool; Description("Adds two integers and returns the sum. The second integer is optional and defaults to 42 if not provided.")>]
    static member AddOptional2(a: int, b: int option) : int =
        let bFinal = defaultArg b 42
        add { A = a; B = bFinal }

    [<McpServerTool; Description("Adds two integers and returns the sum. The second integer is optional and defaults to 42 if not provided.")>]
    static member AddOptional3(a: int, b: Nullable<int>) : int =
        let bFinal = if b.HasValue then b.Value else 42
        add { A = a; B = bFinal }
    [<McpServerTool; Description("Adds two integers and returns the sum. The second integer is optional and defaults to 42 if not provided.")>]
    static member AddOptional4(a: int, [<Description("The second integer to add. If not provided, defaults to 42."); DefaultValue(42:int)>] b: Nullable<int>) : int =
        let bFinal = if b.HasValue then b.Value else 84 // note we lied here and use 84 but the suggested default value is 42 (to test who is implementing the default value logic)
        add { A = a; B = bFinal }

    [<McpServerTool; Description("Lists all available MCP resources with their URIs and descriptions.")>]
    static member ListResources() : string =
        "Resources:\n- info://server (text/plain): Server Information\n- data://sample (application/json): Sample Data"

    [<McpServerTool; Description("Retrieves the content of a specific MCP resource by its URI.")>]
    static member GetResource([<Description("The URI of the resource to retrieve (e.g. 'info://server' or 'data://sample').")>] uri: string) : string =
        match uri with
        | "info://server" ->
            let r = McpResources.ServerInfo()
            r.Text
        | "data://sample" ->
            let r = McpResources.SampleData()
            r.Text
        | _ -> sprintf "Resource not found: %s" uri
