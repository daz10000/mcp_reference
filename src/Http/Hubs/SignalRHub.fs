namespace MCPReference.Http.Hubs

open System.Threading.Tasks
open Microsoft.AspNetCore.SignalR

type SignalRHub() =
    inherit Hub()

    member _.Echo(message: string) : Task<string> =
        Task.FromResult(sprintf "Echo: %s" message)
