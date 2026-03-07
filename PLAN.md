# Reference MCP server

```markdown
# Reference MCP server — Single-project Plan

This plan keeps a single F# application project (one `.fsproj`) and organizes source under `src/` to remain one-level for code while providing clear folders for protocol logic and the HTTP host.

Goals
- Implement a minimal MCP reference server in F# (Giraffe + SignalR placeholders).
- Provide an `Echo` tool example and minimal unit-test scaffold.
- Keep the repo simple: one primary project, easy to extend into multiple projects later.

Recommended layout (single-project)
- `src/` — all F# source files and the existing `mcp_reference.fsproj` (move here when ready).
	- `Core/` — protocol types and tool implementations (`Protocol.fs`, `Echo.fs`).
	- `Http/` — HTTP host and SignalR hub skeleton (`Web.fs`, `Hubs/SignalRHub.fs`).
- `tests/` — optional test project(s) for `Echo` and protocol behavior.
- `build/` — simple build scripts (`build.ps1`, `build.sh`) and CI helpers.
- `README.md`, `.gitignore`, `PLAN.md` (this file)

Implementation steps (ordered)
1. Scaffold folders and starter files under `src/` (Core, Http, Hubs).
2. Add minimal implementations: `Protocol.fs`, `Echo.fs`, `Web.fs`, `SignalRHub.fs`, and a `src/Program.fs` example to wire them together.
3. Add `README.md` and `.gitignore`.
4. Optionally add `tests/` with a simple unit test for `Echo`.
5. Add lightweight CI (`.github/workflows/ci.yml`) and build wrappers in `build/`.

> Local run notes
- Default development URL: `http://localhost:5000`.
- Build: `dotnet restore` then `dotnet build src/mcp_reference.fsproj -c Debug`.
- Run: `dotnet run --project src/mcp_reference.fsproj` (after moving/updating the `.fsproj` as needed).

Tasks (short)
- Scaffold folders and starter files.
- Update `PLAN.md` (this change).
- Add README and `.gitignore`.
- Implement Core and Http modules (minimal, compile-safe placeholders).
- Create tests scaffold (optional).

If you want, I will now create the scaffold files for steps 1 and 2. If you'd rather I move the existing `.fsproj` into `src/` immediately, tell me and I will do that as the next step.

```

