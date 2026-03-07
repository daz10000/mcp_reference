namespace MCPReference.Http

open System
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Builder
open Giraffe
open MCPReference.Http.Hubs

module Web =
    let webApp : HttpHandler =
        choose [
            GET >=> route "/" >=> text "MCP Reference Home"
        ]

    let start () =
        let builder = WebApplication.CreateBuilder()
        builder.Services.AddGiraffe() |> ignore
        builder.Services.AddCors(fun options -> options.AddDefaultPolicy(fun policy -> policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin() |> ignore) |> ignore) |> ignore
        builder.Services.AddSignalR() |> ignore

        let app = builder.Build()
        app.UseCors() |> ignore
        app.UseRouting() |> ignore

        // Use Giraffe to handle HTTP routes
        app.UseGiraffe webApp |> ignore

        app.UseEndpoints(fun endpoints ->
            endpoints.MapHub<MCPReference.Http.Hubs.SignalRHub>("/hub") |> ignore
        ) |> ignore

        // Run the app (blocking) so the host stays up when invoked directly.
        app.Run()
