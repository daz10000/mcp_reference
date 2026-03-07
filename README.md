# MCP Reference (Single-project scaffold)

This repo is a reference MCP server scaffold implemented in F#.

Quick start (after moving `mcp_reference.fsproj` into `src/` or updating paths):

```powershell
dotnet restore
dotnet build src/mcp_reference.fsproj -c Debug
dotnet run --project src/mcp_reference.fsproj
```

The scaffold includes:
- `src/Core` — protocol and Echo example
- `src/Http` — HTTP host placeholders
- `tests/` — test scaffolding (to add)
- `build/` — build helpers (to add)
