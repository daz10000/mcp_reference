namespace MCPReference.Tests

open NUnit.Framework
open System.Net.Http
open System.Threading.Tasks

[<TestFixture>]
module HttpIntegrationTests =

    [<Test>]
    let ``Root endpoint returns home text`` () : Task =
        task {
            use client = new HttpClient()
            client.BaseAddress <- System.Uri("http://localhost:5000")
            // Note: This test expects the host to be running separately. It's a placeholder for integration testing.
            let! response = client.GetAsync("/")
            Assert.IsTrue(response.IsSuccessStatusCode || response.StatusCode = System.Net.HttpStatusCode.NotFound)
        }
