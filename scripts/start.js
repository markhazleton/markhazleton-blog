const concurrently = require("concurrently");
const path = require("path");
const upath = require("upath");

// Detect platform
const isWindows = process.platform === "win32";

// Set BrowserSync executable based on platform
const browserSyncPath = isWindows
    ? upath.resolve(__dirname, "../node_modules/.bin/browser-sync.cmd")
    : upath.resolve(__dirname, "../node_modules/.bin/browser-sync");

// Set SSL key and cert paths
const sslKeyPath = isWindows
    ? "C:\\Program Files\\OpenSSL-Win64\\bin\\localhost.key"
    : "/usr/local/etc/openssl/localhost.key";

const sslCertPath = isWindows
    ? "C:\\Program Files\\OpenSSL-Win64\\bin\\localhost.crt"
    : "/usr/local/etc/openssl/localhost.crt";

// BrowserSync command for Windows and macOS/Linux
const browserSyncCommand = isWindows
    ? `powershell -Command "& '${browserSyncPath}' --reload-delay 2000 --reload-debounce 2000 docs -w --no-online --https --key '${sslKeyPath}' --cert '${sslCertPath}'"`
    : `${browserSyncPath} --reload-delay 2000 --reload-debounce 2000 docs -w --no-online --https --key ${sslKeyPath} --cert ${sslCertPath}`;

// Run concurrently
concurrently(
    [
        {
            command: "node scripts/sb-watch.js",
            name: "SB_WATCH",
            prefixColor: "bgBlue.bold",
        },
        {
            command: browserSyncCommand,
            name: "SB_BROWSER_SYNC",
            prefixColor: "bgGreen.bold",
        },
    ],
    {
        prefix: "name",
        killOthers: ["failure", "success"],
        restartTries: 3, // Optional: Set restart attempts if failure occurs
    }
);

// Success and failure handlers
function success() {
    console.log("Success");
}

function failure() {
    console.log("Failure");
}