const concurrently = require("concurrently");
const upath = require("upath");
const browserSyncPath = upath.resolve(
    upath.dirname(__filename),
    "../node_modules/.bin/browser-sync.cmd"
);
// Using PowerShell command in the correct format
const browserSyncCommand = `powershell -Command "& '${browserSyncPath}' --reload-delay 2000 --reload-debounce 2000 docs -w --no-online --https --key 'C:\\Program Files\\OpenSSL-Win64\\bin\\localhost.key' --cert 'C:\\Program Files\\OpenSSL-Win64\\bin\\localhost.crt'"`;
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
function success() {
    console.log("Success");
}
function failure() {
    console.log("Failure");
}
