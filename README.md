# MCP Reference (F#)

Reference MCP server implemented in F# on ASP.NET Core with:

- official `ModelContextProtocol.AspNetCore` middleware (`/mcp`)
- SignalR hub (`/hub`) for realtime interactions
- typed HTTP sample endpoints (`/mcp/echo`, `/mcp/add`)

## Requirements

- .NET SDK 9.0+

## Quick start

```powershell
dotnet restore src/mcp_reference.fsproj
dotnet build src/mcp_reference.fsproj -c Debug
dotnet run --project src/mcp_reference.fsproj
```

Server listens on `http://127.0.0.1:5000`.

## Endpoints

- `POST /mcp` — official MCP streamable HTTP endpoint (via `MapMcp("/mcp")`)
- `GET /` — health/home text
- `GET /mcp/tools` — local registry tool descriptors
- `POST /mcp/tools/{name}` — local registry invoke (JSON in, YAML/plain out)
- `POST /mcp/echo` — typed sample tool endpoint (`{ "Text": "hi" }`)
- `POST /mcp/add` — typed sample tool endpoint (`{ "A": 1, "B": 2 }`)
- `GET/POST /hub` — SignalR hub endpoint

## MCP tools exposed

The official MCP middleware discovers tools from `src/Core/McpTools.fs`:

- `Echo(text: string) -> string`
- `Add(a: int, b: int) -> int`

## Tests

```powershell
dotnet test tests/mcp_reference.Tests.fsproj -c Debug
```

Includes integration coverage for root HTTP, typed endpoints, SignalR, and MCP middleware mapping.

## MCPInspector quick connect

1. Run the server:

```powershell
dotnet run --project src/mcp_reference.fsproj
```

2. Point MCPInspector to:

- `http://127.0.0.1:5000/mcp`

3. Connect and call `tools/list`.

Expected: tool list includes `Echo` and `Add`.

## Notes on dependencies

- NuGet references are in `src/mcp_reference.fsproj`
- Paket references are in `paket.dependencies` and `src/paket.references`
