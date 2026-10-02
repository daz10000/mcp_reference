namespace MCPReference.Tests

open NUnit.Framework
open System.Net.Http
open System.Threading.Tasks

[<TestFixture>]
module HttpIntegrationTests =

    [<Test>]
    let ``Root endpoint returns home text`` () : Task =
        task {
            // start host on a free port
            let listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0)
            listener.Start()
            let port = (listener.LocalEndpoint :?> System.Net.IPEndPoint).Port
            listener.Stop()

            use app = MCPReference.Http.Web.createHost(port)
            let runTask = app.RunAsync()

            use client = new HttpClient()
            client.BaseAddress <- System.Uri(sprintf "http://127.0.0.1:%d" port)
            let! response = client.GetAsync("/")
            Assert.That(response.IsSuccessStatusCode, Is.True)

            do! app.StopAsync()
            do! runTask
        }
