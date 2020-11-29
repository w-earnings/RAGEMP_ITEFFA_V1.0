const webpack = require('webpack');

module.exports = {
  entry: {
    browser: './interface/index.js',
    client: './interface/client.js',
  },
  module: {
    rules: [{
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: ['babel-loader']
      },
      {
        test: /\.css$/,
        use: ['style-loader', 'css-loader'],
      },
      {
        test: /\.(jpe?g|png|gif|woff|woff2|eot|ttf|svg)(\?[a-z0-9=.]+)?$/,
        loader: 'url-loader?limit=100000',
      },
    ]
  },
  resolve: {
    extensions: ['*', '.js', '.jsx']
  },
  output: {
    path: __dirname + '/client_packages',
    publicPath: '/',
    filename: '[name].js'
  },
  plugins: [],
  devServer: {
    contentBase: './client_packages',
  }
};