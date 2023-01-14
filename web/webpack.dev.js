const path = require('path');
const HtmlWebpackPlugin = require('html-webpack-plugin');
const CopyWebpackPlugin = require('copy-webpack-plugin');

function createCopy(pathW = '') {
    return ({
        entry: './src/app.js',
        output: {
            path: path.resolve(__dirname, 'docs/public' + pathW),
            filename: 'appBundle.js',
        },
        module: {
            rules: [
                {
                    test: /\.(js|jsx)$/,
                    exclude: /node_modules/,
                    loader: 'babel-loader',
                },
                {
                    test: /\.(scss|css)$/,
                    use: ['style-loader', 'css-loader'],
                },
                {
                    test: /\.(jpg|png|svg|ico|icns|glb|gif)$/,
                    loader: 'file-loader',
                    options: {
                        name: '[path][name].[ext]',
                    },
                }
            ],
        },
        plugins: [
            new HtmlWebpackPlugin({
                filename: 'index.html',
                template: path.resolve(__dirname, './public/index.html'),
            }),
            new CopyWebpackPlugin({
                patterns: [
                    { from: 'assets' }
                ]
            })
        ],
        experiments: {
            topLevelAwait: true
        }
    })
}

module.exports = [
    createCopy()
];
