namespace MCPReference.Core

module Protocol =
    type Message =
        { Id: int
          Text: string }

    type Result<'T> =
        | Ok of 'T
        | Error of string
