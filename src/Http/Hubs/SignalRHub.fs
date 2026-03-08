namespace MCPReference.Http.Hubs

open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR
open MCPReference.Core
open MCPReference.Core.Protocol

type SignalRHub() =
    inherit Hub()

    /// Keep the connection alive; clients can call to check server readiness.
    member _.KeepAlive() : Task<string> =
        Task.FromResult("OK")

    /// Return a list of available tools via the registry.
    member _.ListTools() : Task<string []> =
        Registry.listTools() |> Task.FromResult

    /// Generic invocation: toolName and a single argument (dynamic object).
    member _.CallTool(toolName: string, arg: obj) : Task<obj> =
        match Registry.tryInvoke toolName arg with
        | Some res -> Task.FromResult(res)
        | None -> Task.FromResult(null)

    /// Backwards-compatible CallEcho that uses the registry.
    member _.CallEcho(msg: Message) : Task<Message> =
        match Registry.tryInvoke "Echo" (box msg) with
        | Some o -> Task.FromResult(o :?> Message)
        | None -> Task.FromResult(msg)
