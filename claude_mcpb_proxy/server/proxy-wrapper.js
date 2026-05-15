#!/usr/bin/env node

const { spawn } = require("node:child_process");
const path = require("node:path");
const fs = require("node:fs");

const remoteUrl = (process.env.REMOTE_MCP_URL || "").trim();
const transport = process.env.REMOTE_TRANSPORT || "sse-only";
const allowHttp = (process.env.REMOTE_ALLOW_HTTP || "true").toLowerCase() === "true";
const bearerToken = (process.env.REMOTE_AUTH_BEARER_TOKEN || "").trim();

if (!remoteUrl) {
  console.error("REMOTE_MCP_URL is required. Set it via extension user configuration.");
  process.exit(1);
}

if (remoteUrl.includes("example.com")) {
  console.error("REMOTE_MCP_URL is still a placeholder. Set it to your real MCP endpoint URL.");
  process.exit(1);
}

let mcpRemoteEntry;

try {
  const mcpRemotePkgPath = require.resolve("mcp-remote/package.json");
  const mcpRemotePkg = JSON.parse(fs.readFileSync(mcpRemotePkgPath, "utf8"));
  const mcpRemoteBin =
    typeof mcpRemotePkg.bin === "string" ? mcpRemotePkg.bin : mcpRemotePkg.bin?.["mcp-remote"];

  if (!mcpRemoteBin) {
    throw new Error("mcp-remote package does not expose a mcp-remote bin entry");
  }

  mcpRemoteEntry = path.join(path.dirname(mcpRemotePkgPath), mcpRemoteBin);
} catch (err) {
  console.error("Failed to locate bundled mcp-remote.");
  console.error(err && err.stack ? err.stack : String(err));
  process.exit(1);
}

const args = [mcpRemoteEntry, remoteUrl, "--transport", transport];

if (allowHttp) {
  args.push("--allow-http");
}

if (bearerToken) {
  // Keep a single argument for the header to avoid command-line splitting issues.
  args.push("--header", `Authorization:Bearer ${bearerToken}`);
}

const child = spawn(process.execPath, args, {
  stdio: "inherit",
  shell: false,
  env: process.env,
});

child.on("error", (err) => {
  console.error("Failed to launch mcp-remote child process.");
  console.error(err && err.stack ? err.stack : String(err));
  process.exit(1);
});

child.on("exit", (code, signal) => {
  if (signal) {
    process.kill(process.pid, signal);
    return;
  }
  process.exit(code ?? 1);
});
