namespace MCPReference.Core

module Echo =
    open System
    open Protocol

    let echo (msg: Message) =
        { msg with Text = sprintf "Echo: %s" msg.Text }

    // Return a small descriptor for registry use
    let descriptor () = ("Echo", "Echo tool - echoes messages")
