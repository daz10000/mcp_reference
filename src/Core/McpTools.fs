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
