namespace MCPReference.Tests

open NUnit.Framework
open MCPReference.Core

[<TestFixture>]
module McpPromptsAndResourcesTests =

    [<Test>]
    let ``Greeting prompt returns GetPromptResult with message`` () =
        let result = McpPrompts.Greeting("Alice")
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Description, Is.EqualTo("A friendly greeting prompt"))
        Assert.That(result.Messages.Count, Is.EqualTo(1))
        let msg = result.Messages.[0]
        Assert.That(msg.Role, Is.EqualTo(ModelContextProtocol.Protocol.Role.User))
        let textBlock = msg.Content :?> ModelContextProtocol.Protocol.TextContentBlock
        Assert.That(textBlock.Text.Contains("Alice"), Is.True)

    [<Test>]
    let ``CodeReview prompt returns GetPromptResult with message`` () =
        let result = McpPrompts.CodeReview("let x = 1")
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Description, Is.EqualTo("A prompt to request a code review"))
        Assert.That(result.Messages.Count, Is.EqualTo(1))
        let msg = result.Messages.[0]
        Assert.That(msg.Role, Is.EqualTo(ModelContextProtocol.Protocol.Role.User))
        let textBlock = msg.Content :?> ModelContextProtocol.Protocol.TextContentBlock
        Assert.That(textBlock.Text.Contains("let x = 1"), Is.True)

    [<Test>]
    let ``ServerInfo resource returns TextResourceContents`` () =
        let result = McpResources.ServerInfo()
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Uri, Is.EqualTo("info://server"))
        Assert.That(result.MimeType, Is.EqualTo("text/plain"))
        Assert.That(result.Text.Contains("MCP Reference Server"), Is.True)

    [<Test>]
    let ``SampleData resource returns TextResourceContents`` () =
        let result = McpResources.SampleData()
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Uri, Is.EqualTo("data://sample"))
        Assert.That(result.MimeType, Is.EqualTo("application/json"))
        Assert.That(result.Text.Contains("MCP Reference"), Is.True)

    [<Test>]
    let ``ListResources tool returns resource listing`` () =
        let result = McpTools.ListResources()
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Contains("info://server"), Is.True)
        Assert.That(result.Contains("data://sample"), Is.True)

    [<Test>]
    let ``GetResource tool returns server-info content`` () =
        let result = McpTools.GetResource("info://server")
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Contains("MCP Reference Server"), Is.True)

    [<Test>]
    let ``GetResource tool returns sample-data content`` () =
        let result = McpTools.GetResource("data://sample")
        Assert.That(result, Is.Not.Null)
        Assert.That(result.Contains("MCP Reference"), Is.True)

    [<Test>]
    let ``GetResource tool returns not-found for unknown URI`` () =
        let result = McpTools.GetResource("unknown://resource")
        Assert.That(result.Contains("not found"), Is.True)
