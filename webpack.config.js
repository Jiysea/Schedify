const path = require("path");

module.exports = {
    entry: "./wwwroot/js/site.js",
    output: {
        path: path.resolve(__dirname, "wwwroot/dist"),
        filename: "bundle.js",
    },
    mode: "development",
    module: {
        rules: [
            {
                test: /\.css$/i,
                use: ["style-loader", "css-loader"], // Handles CSS imports
            },
        ],
    },
};
