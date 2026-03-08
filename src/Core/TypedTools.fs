namespace MCPReference.Core

module TypedTools =
    /// Simple typed request for Echo
    type EchoRequest = { Text: string }

    /// Simple typed request for adding two numbers
    type AddRequest = { A: int; B: int }

    /// Typed echo implementation returning a string
    let echo (req: EchoRequest) : string =
        sprintf "Echo: %s" req.Text

    /// Add two numbers and return the sum
    let add (req: AddRequest) : int =
        req.A + req.B
