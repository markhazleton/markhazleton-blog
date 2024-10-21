const concurrently = require("concurrently");
const upath = require("upath");
const path = require("path");

// Detect platform
const isWindows = process.platform === "win32";

// Set BrowserSync path based on platform
const browserSyncPath = isWindows
    ? upath.resolve(upath.dirname(__filename), "../node_modules/.bin/browser-sync.cmd")
    : upath.resolve(upath.dirname(__filename), "../node_modules/.bin/browser-sync");

// BrowserSync command
const browserSyncCommand = `${browserSyncPath} docs -w --no-online`;

concurrently(
    [
        {
            command: "node --inspect scripts/sb-watch.js",
            name: "SB_WATCH",
            prefixColor: "bgBlue.bold",
        },
        {
            command: browserSyncCommand,
            name: "SB_BROWSER_SYNC",
            prefixColor: "bgGreen.bold", // Changed to green for consistency with your other example
        },
    ],
    {
        prefix: "name",
        killOthers: ["failure", "success"],
    }
);

// Success and failure handlers
function success() {
    console.log("Success");
}

function failure() {
    console.log("Failure");
}