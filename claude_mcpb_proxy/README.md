# Example Claude Remote HTTP MCP Proxy (MCPB)

This MCP Bundle installs into Claude Desktop and forwards local stdio MCP traffic to a remote MCP endpoint using `mcp-remote`.  Customize the `manifest.json` and `package.json` where it says `EDIT THIS` to create your own bundle that wraps `mcp-remote` with your desired configuration and branding.

## What it does

- Launches a local stdio command process in Claude Desktop.
- Uses `mcp-remote` as a stdio-to-HTTP/SSE proxy.
- Supports optional bearer auth header.

## Edit the config files.

Bare minimum you need to edit the URL you want to proxy against but the name and description fields are also important for a good user experience.

## Build the bundle

From this project directory:

```powershell
npm install
npx @anthropic-ai/mcpb pack
```

The command produces a `.mcpb` file in the project directory.

## Install in Claude Desktop

1. Open Claude Desktop Settings.
2. Go to Extensions.
3. Open the Advanced section.
4. Select the bundled `.mcpb` file you just created.

(NB: if you're interating config as a developer, you can skip the pack step and load the unpacked extension directly from the project directory.)

## Notes

- Under the hood it's using plain-http. This should only be enabled for trusted private network endpoints e.g. local / vpn.
- If your server expects OAuth, `mcp-remote` will manage its local auth callback flow.
