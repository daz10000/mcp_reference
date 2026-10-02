namespace MCPReference.Tests

open NUnit.Framework
open MCPReference.Core
open MCPReference.Core.Protocol
open System.Text.Json

[<TestFixture>]
module EchoTests =

    [<Test>]
    let ``Echo.echo prepends Echo:`` () =
        let msg = { Id = 42; Text = "ping" }
        let echoed = Echo.echo msg
        Assert.That(echoed.Text.StartsWith("Echo: "), Is.True)

    [<Test>]
    let ``Registry Echo accepts lowercase text property`` () =
        Registry.registerDefaults()
        use doc = JsonDocument.Parse("{ \"text\": \"wow\" }")
        let result = Registry.tryInvoke "Echo" (box doc.RootElement)

        match result with
        | Some (:? Message as m) -> Assert.That(m.Text, Is.EqualTo("Echo: wow"))
        | _ -> Assert.Fail("Expected Echo tool to return Message with echoed text")
