namespace MCPReference.Core

open System.ComponentModel
open ModelContextProtocol.Server
open ModelContextProtocol.Protocol

[<McpServerResourceType>]
type McpResources =
    [<McpServerResource(UriTemplate = "info://server", Name = "server-info", Title = "Server Information", MimeType = "text/plain")>]
    [<Description("Returns general information about this MCP reference server.")>]
    static member ServerInfo() : TextResourceContents =
        TextResourceContents(
            Uri = "info://server",
            MimeType = "text/plain",
            Text = "MCP Reference Server\nVersion: 1.0.0\nTools: Echo, Add, ListResources, GetResource\nPrompts: Greeting, CodeReview\nResources: server-info, sample-data"
        )

    [<McpServerResource(UriTemplate = "data://sample", Name = "sample-data", Title = "Sample Data", MimeType = "application/json")>]
    [<Description("Returns a sample JSON data object for demonstration purposes.")>]
    static member SampleData() : TextResourceContents =
        TextResourceContents(
            Uri = "data://sample",
            MimeType = "application/json",
            Text = """{"name":"MCP Reference","items":["Echo","Add","Greeting","CodeReview"],"version":"1.0.0"}"""
        )
