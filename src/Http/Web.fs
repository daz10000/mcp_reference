namespace MCPReference.Http

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open ModelContextProtocol.Server
open MCPReference.Http.Hubs
open MCPReference.Core.TypedTools
open MCPReference.Core

module Web =
    let webApp : HttpHandler =
        choose [
            GET >=> route "/" >=> text "MCP Reference Home"
            GET >=> route "/mcp/tools" >=> fun next ctx ->
                let descriptors = Registry.listDescriptors()
                json descriptors next ctx

            POST >=> routef "/mcp/tools/%s" (fun name ->
                fun next ctx ->
                    task {
                        // bind raw JSON to JsonElement
                        let! je = ctx.BindJsonAsync<System.Text.Json.JsonElement>()
                        match Registry.tryInvoke name (box je) with
                        | Some res ->
                            // convert result to simple YAML
                            let yaml =
                                match res with
                                | :? string as s -> sprintf "result: \"%s\"\n" s
                                | :? int as i -> sprintf "result: %d\n" i
                                | :? MCPReference.Core.Protocol.Message as m -> sprintf "id: %d\ntext: \"%s\"\n" m.Id m.Text
                                | _ -> System.Text.Json.JsonSerializer.Serialize(res)
                            return! (setStatusCode 200 >=> setHttpHeader "Content-Type" "text/yaml" >=> text yaml) next ctx
                        | None -> return! (setStatusCode 404 >=> text "tool not found") next ctx
                    })
            POST >=> route "/mcp/echo" >=> bindJson<EchoRequest> (fun req ->
                let result = echo req
                // respond with simple YAML string
                setStatusCode 200 >=> setHttpHeader "Content-Type" "text/yaml" >=> text (sprintf "result: \"%s\"\n" result)
            )
            POST >=> route "/mcp/add" >=> bindJson<AddRequest> (fun req ->
                let sum = add req
                setStatusCode 200 >=> setHttpHeader "Content-Type" "text/yaml" >=> text (sprintf "result: %d\n" sum)
            )
        ]
    let createHost (port: int) : Microsoft.AspNetCore.Builder.WebApplication =
        // ensure default tools are registered for typed endpoints and SignalR
        MCPReference.Core.Registry.registerDefaults()

        let builder = WebApplication.CreateBuilder()
        builder.Services
            .AddMcpServer()
            .WithHttpTransport()
            .WithToolsFromAssembly()
        |> ignore

        builder.Services.AddGiraffe() |> ignore
        builder.Services.AddCors(fun options -> options.AddDefaultPolicy(fun policy -> policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() |> ignore) |> ignore) |> ignore
        builder.Services.AddSignalR() |> ignore

        let app = builder.Build()
        app.UseCors() |> ignore
        app.UseRouting() |> ignore
        app.MapMcp("/mcp") |> ignore

        // Use Giraffe to handle HTTP routes
        app.UseGiraffe webApp |> ignore

        app.UseEndpoints(fun endpoints ->
            endpoints.MapHub<MCPReference.Http.Hubs.SignalRHub>("/hub") |> ignore
        ) |> ignore

        // ensure we listen on the requested port
        app.Urls.Clear()
        app.Urls.Add(sprintf "http://127.0.0.1:%d" port)
        app

    let start () =
        let app = createHost(5000)
        // Run the app (blocking) so the host stays up when invoked directly.
        app.Run()
