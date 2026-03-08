namespace MCPReference.Tests

open NUnit.Framework
open System.Threading.Tasks
open System.Net.Http
open System.Text
open System.Net

[<TestFixture>]
module McpMiddlewareTests =

    [<Test>]
    let ``POST /mcp is mapped by MCP middleware`` () : Task =
        task {
            let listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0)
            listener.Start()
            let port = (listener.LocalEndpoint :?> System.Net.IPEndPoint).Port
            listener.Stop()

            use app = MCPReference.Http.Web.createHost(port)
            let runTask = app.RunAsync()

            use client = new HttpClient()
            client.BaseAddress <- System.Uri(sprintf "http://127.0.0.1:%d" port)

            let probeBody = "{ \"jsonrpc\":\"2.0\", \"id\": 1, \"method\": \"tools/list\", \"params\": {} }"
            use content = new StringContent(probeBody, Encoding.UTF8, "application/json")
            let! resp = client.PostAsync("/mcp", content)

            Assert.AreNotEqual(HttpStatusCode.NotFound, resp.StatusCode)

            do! app.StopAsync()
            do! runTask
        }
