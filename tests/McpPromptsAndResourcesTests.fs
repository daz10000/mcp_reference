namespace MCPReference.Tests

open NUnit.Framework
open MCPReference.Core

[<TestFixture>]
module McpPromptsAndResourcesTests =

    [<Test>]
    let ``Greeting prompt returns GetPromptResult with message`` () =
        let result = McpPrompts.Greeting("Alice")
        Assert.IsNotNull(result)
        Assert.AreEqual("A friendly greeting prompt", result.Description)
        Assert.AreEqual(1, result.Messages.Count)
        let msg = result.Messages.[0]
        Assert.AreEqual(ModelContextProtocol.Protocol.Role.User, msg.Role)
        let textBlock = msg.Content :?> ModelContextProtocol.Protocol.TextContentBlock
        Assert.IsTrue(textBlock.Text.Contains("Alice"))

    [<Test>]
    let ``CodeReview prompt returns GetPromptResult with message`` () =
        let result = McpPrompts.CodeReview("let x = 1")
        Assert.IsNotNull(result)
        Assert.AreEqual("A prompt to request a code review", result.Description)
        Assert.AreEqual(1, result.Messages.Count)
        let msg = result.Messages.[0]
        Assert.AreEqual(ModelContextProtocol.Protocol.Role.User, msg.Role)
        let textBlock = msg.Content :?> ModelContextProtocol.Protocol.TextContentBlock
        Assert.IsTrue(textBlock.Text.Contains("let x = 1"))

    [<Test>]
    let ``ServerInfo resource returns TextResourceContents`` () =
        let result = McpResources.ServerInfo()
        Assert.IsNotNull(result)
        Assert.AreEqual("info://server", result.Uri)
        Assert.AreEqual("text/plain", result.MimeType)
        Assert.IsTrue(result.Text.Contains("MCP Reference Server"))

    [<Test>]
    let ``SampleData resource returns TextResourceContents`` () =
        let result = McpResources.SampleData()
        Assert.IsNotNull(result)
        Assert.AreEqual("data://sample", result.Uri)
        Assert.AreEqual("application/json", result.MimeType)
        Assert.IsTrue(result.Text.Contains("MCP Reference"))

    [<Test>]
    let ``ListResources tool returns resource listing`` () =
        let result = McpTools.ListResources()
        Assert.IsNotNull(result)
        Assert.IsTrue(result.Contains("info://server"))
        Assert.IsTrue(result.Contains("data://sample"))

    [<Test>]
    let ``GetResource tool returns server-info content`` () =
        let result = McpTools.GetResource("info://server")
        Assert.IsNotNull(result)
        Assert.IsTrue(result.Contains("MCP Reference Server"))

    [<Test>]
    let ``GetResource tool returns sample-data content`` () =
        let result = McpTools.GetResource("data://sample")
        Assert.IsNotNull(result)
        Assert.IsTrue(result.Contains("MCP Reference"))

    [<Test>]
    let ``GetResource tool returns not-found for unknown URI`` () =
        let result = McpTools.GetResource("unknown://resource")
        Assert.IsTrue(result.Contains("not found"))
