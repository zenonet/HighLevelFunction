var path = require('path');
const MonacoWebpackPlugin = require('monaco-editor-webpack-plugin');
const HtmlWebpackPlugin = require('html-webpack-plugin')

module.exports = {
/*    output: {
        publicPath: "/js/",
        path: path.join(__dirname, '/wwwroot/js/'),
        filename: 'main.build.js',
    },*/

    entry: './src/index.js',
    //devtool: "source-map",
    dependencies:[
        "client-zip",
        "monaco-editor",
        "hlf-transpiler"
    ],
    mode: "development",
    //mode: "production",
    module: {
        rules: [
            {
                test: /\.css$/,
                use: ['style-loader', 'css-loader']
            },
            {
                test: /\.ttf$/,
                type: 'asset/resource'
            }
        ],
    },
    resolve: {
        extensions: ['.js', '.mjs'],
        alias: {
            hlf: require.resolve('./src/hlf.js'),
        }
    },
    parallelism: 12,
    plugins: [new MonacoWebpackPlugin({
        //publicPath: "/dist/",
        languages: [],
        monacoEditorPath: "C:\\Users\\zeno\\RiderProjects\\Hlf.Transpiler\\Hlf.Web\\wwwroot\\node_modules\\monaco-editor"
    }),
    new HtmlWebpackPlugin(
        {
            template: './src/index.html',
        }
    )],
    output: {
        clean: true
    },
    optimization: {
        splitChunks: {
            minSize: 10000,
            maxSize: 250000,
        },
        minimize:true,
    },
    performance: {
        hints: false,
    },
};