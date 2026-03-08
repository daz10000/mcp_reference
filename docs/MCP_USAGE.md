# MCP Usage Notes

This project uses the official ASP.NET Core integration from `ModelContextProtocol.AspNetCore`.

## Server wiring

In `src/Http/Web.fs` the server is configured with:

- `builder.Services.AddMcpServer().WithHttpTransport().WithToolsFromAssembly()`
- `app.MapMcp("/mcp")`

Tools are discovered from `src/Core/McpTools.fs` via attributes:

- `[<McpServerToolType>]`
- `[<McpServerTool>]`

## Manual probe examples

### MCP endpoint mapped check

A basic probe that should return **not 404** (typically `406` for malformed/partial request):

```powershell
Invoke-WebRequest -Method Post -Uri http://127.0.0.1:5000/mcp -ContentType "application/json" -Body '{ "jsonrpc":"2.0", "id":1, "method":"tools/list", "params":{} }'
```

### Typed sample endpoints

```powershell
Invoke-WebRequest -Method Post -Uri http://127.0.0.1:5000/mcp/echo -ContentType "application/json" -Body '{ "Text": "hello" }'
Invoke-WebRequest -Method Post -Uri http://127.0.0.1:5000/mcp/add -ContentType "application/json" -Body '{ "A": 2, "B": 3 }'
```

### Tool discovery via local registry endpoint

```powershell
Invoke-WebRequest -Method Get -Uri http://127.0.0.1:5000/mcp/tools
```

## Test coverage

`tests/McpMiddlewareTests.fs` validates that `POST /mcp` is handled by MCP middleware.
