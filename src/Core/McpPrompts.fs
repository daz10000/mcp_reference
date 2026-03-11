namespace MCPReference.Core

open System.ComponentModel
open ModelContextProtocol.Server
open ModelContextProtocol.Protocol

[<McpServerPromptType>]
type McpPrompts =
    [<McpServerPrompt; Description("A simple greeting prompt that addresses the user by name.")>]
    static member Greeting([<Description("The name of the person to greet.")>] name: string) : GetPromptResult =
        GetPromptResult(
            Description = "A friendly greeting prompt",
            Messages = ResizeArray [
                PromptMessage(
                    Role = Role.User,
                    Content = TextContentBlock(Text = sprintf "Please greet %s in a warm and friendly way." name)
                )
            ]
        )

    [<McpServerPrompt; Description("A code review prompt that asks for a review of a code snippet.")>]
    static member CodeReview([<Description("The code snippet to review.")>] code: string) : GetPromptResult =
        GetPromptResult(
            Description = "A prompt to request a code review",
            Messages = ResizeArray [
                PromptMessage(
                    Role = Role.User,
                    Content = TextContentBlock(Text = sprintf "Please review the following code and provide constructive feedback:\n\n```\n%s\n```" code)
                )
            ]
        )
