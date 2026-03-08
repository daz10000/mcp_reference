open System
open MCPReference.Core.Protocol
open MCPReference.Core
open MCPReference.Http

[<EntryPoint>]
let main _ =
    printfn "MCP Reference (single-project scaffold)"
    // register default tools
    Registry.registerDefaults()

    let msg = { Id = 1; Text = "hello" }
    let echoed = Echo.echo msg
    printfn "Echoed: %s" echoed.Text
    Web.start ()
    0
