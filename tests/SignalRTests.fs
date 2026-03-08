namespace MCPReference.Tests

open NUnit.Framework
open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR.Client
open System
open MCPReference.Core.Protocol
open MCPReference.Http

[<TestFixture>]
module SignalRTests =

    [<Test>]
    let ``SignalR CallEcho returns echoed message`` () : Task =
        task {
            // Start host on a free TCP port
            let listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0)
            listener.Start()
            let port = (listener.LocalEndpoint :?> System.Net.IPEndPoint).Port
            listener.Stop()

            use app = Web.createHost(port)
            let runTask = app.RunAsync()

            // Create SignalR HubConnection
            let conn = HubConnectionBuilder()
                        .WithUrl(sprintf "http://127.0.0.1:%d/hub" port)
                        .WithAutomaticReconnect()
                        .Build()

            do! conn.StartAsync()

            // Call KeepAlive
            let! keep = conn.InvokeAsync<string>("KeepAlive")
            Assert.AreEqual("OK", keep)

            // Call CallEcho with a Message
            let msg = { Id = 123; Text = "ping" }
            let! echoed = conn.InvokeAsync<Message>("CallEcho", msg)
            Assert.IsTrue(echoed.Text.StartsWith("Echo: "))

            do! conn.StopAsync()
            do! app.StopAsync()
            do! runTask
        }
