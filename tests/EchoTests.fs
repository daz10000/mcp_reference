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
        Assert.IsTrue(echoed.Text.StartsWith("Echo: "))

    [<Test>]
    let ``Registry Echo accepts lowercase text property`` () =
        Registry.registerDefaults()
        use doc = JsonDocument.Parse("{ \"text\": \"wow\" }")
        let result = Registry.tryInvoke "Echo" (box doc.RootElement)

        match result with
        | Some (:? Message as m) -> Assert.AreEqual("Echo: wow", m.Text)
        | _ -> Assert.Fail("Expected Echo tool to return Message with echoed text")
