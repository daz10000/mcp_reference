namespace MCPReference.Tests

open NUnit.Framework
open MCPReference.Core
open MCPReference.Core.Protocol

[<TestFixture>]
module EchoTests =

    [<Test>]
    let ``Echo.echo prepends Echo:`` () =
        let msg = { Id = 42; Text = "ping" }
        let echoed = Echo.echo msg
        Assert.IsTrue(echoed.Text.StartsWith("Echo: "))
