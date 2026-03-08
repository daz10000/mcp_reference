namespace MCPReference.Tests

open NUnit.Framework
open System.Threading.Tasks
open System.Net.Http
open System.Text
open System

[<TestFixture>]
module McpTypedTests =

    [<Test>]
    let ``POST /mcp/echo returns YAML result`` () : Task =
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

            let json = "{ \"Text\": \"hello typed\" }"
            use content = new StringContent(json, Encoding.UTF8, "application/json")
            let! resp = client.PostAsync("/mcp/echo", content)
            let! body = resp.Content.ReadAsStringAsync()
            Assert.IsTrue(body.Contains("Echo: hello typed"))

            do! app.StopAsync()
            do! runTask
        }

    [<Test>]
    let ``POST /mcp/add returns YAML result`` () : Task =
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

            let json = "{ \"A\": 5, \"B\": 7 }"
            use content = new StringContent(json, Encoding.UTF8, "application/json")
            let! resp = client.PostAsync("/mcp/add", content)
            let! body = resp.Content.ReadAsStringAsync()
            Assert.IsTrue(body.Contains("result: 12"))

            do! app.StopAsync()
            do! runTask
        }
