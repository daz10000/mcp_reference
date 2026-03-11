namespace MCPReference.Core

open System.ComponentModel
open ModelContextProtocol.Server
open MCPReference.Core.TypedTools

[<McpServerToolType>]
type McpTools =
    [<McpServerTool; Description("Echoes a string back to the caller.")>]
    static member Echo(text: string) : string =
        echo { Text = text }

    [<McpServerTool; Description("Adds two integers and returns the sum.")>]
    static member Add(a: int, b: int) : int =
        add { A = a; B = b }

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
