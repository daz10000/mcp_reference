namespace MCPReference.Http

open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Giraffe
open ModelContextProtocol.Server
open MCPReference.Http.Hubs
open MCPReference.Http.ChatLoop
open MCPReference.Core.TypedTools
open MCPReference.Core

module Web =
    let private homePage = """
<!doctype html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width,initial-scale=1" />
    <title>MCP Chat</title>
    <style>
        body { font-family: Segoe UI, sans-serif; margin: 0; background: #f6f7fb; }
        .wrap { max-width: 840px; margin: 20px auto; background: white; border-radius: 10px; padding: 16px; }
        #log { border: 1px solid #ddd; border-radius: 8px; min-height: 320px; max-height: 460px; overflow: auto; padding: 12px; background: #fff; }
        .msg { margin: 10px 0; padding: 10px 12px; border-radius: 10px; white-space: pre-wrap; }
        .u { background: #e8f1ff; }
        .a { background: #f1f5f3; }
        .t { background: #fff7e6; font-size: 13px; }
        .row { margin-top: 12px; display: flex; gap: 8px; }
        textarea { flex: 1; min-height: 70px; padding: 10px; }
        button { padding: 10px 14px; }
    </style>
</head>
<body>
    <div class="wrap">
        <h2>MCP Reference Chat</h2>
        <p>Chat with an LLM loop that can call server tools (Echo, Add) via MCP registry.</p>
        <div id="log"></div>
        <div class="row">
            <textarea id="prompt" placeholder="Ask something, e.g. 'use Add to compute 7 + 9'"></textarea>
            <button id="send">Send</button>
        </div>
    </div>
<script>
const log = document.getElementById('log');
const promptEl = document.getElementById('prompt');
const sendBtn = document.getElementById('send');
const history = [];

function append(cls, text) {
    const d = document.createElement('div');
    d.className = 'msg ' + cls;
    d.textContent = text;
    log.appendChild(d);
    log.scrollTop = log.scrollHeight;
}

async function send() {
    const prompt = promptEl.value.trim();
    if (!prompt) return;
    append('u', 'You: ' + prompt);
    promptEl.value = '';
    sendBtn.disabled = true;
    try {
        const res = await fetch('/api/chat', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ prompt, history })
        });
        const data = await res.json();
        const reply = data.reply || 'No response';
        append('a', 'Assistant: ' + reply);
        history.push({ role: 'user', content: prompt });
        history.push({ role: 'assistant', content: reply });
        if (Array.isArray(data.toolCalls) && data.toolCalls.length > 0) {
            const called = data.toolCalls.map(t => t.name).join(', ');
            append('t', 'Tools called: ' + called);
        }
    } catch (e) {
        append('a', 'Assistant: Request failed.');
    } finally {
        sendBtn.disabled = false;
    }
}

if (sendBtn && promptEl) {
    sendBtn.addEventListener('click', send);
    promptEl.addEventListener('keydown', (e) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            send();
        }
    });
}
</script>
</body>
</html>
"""

    let webApp : HttpHandler =
        choose [
            GET >=> route "/" >=> htmlString homePage
            POST >=> route "/api/chat" >=> bindJson<ChatRequest> (fun req ->
                fun next ctx ->
                    task {
                        let! payload = ChatLoop.run req
                        return! json payload next ctx
                    }
            )
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
            .WithPromptsFromAssembly()
            .WithResourcesFromAssembly()
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
