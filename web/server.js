var fs = require('fs');
var http = require('http');
var https = require('https');
var path = require('path');
// const { v4: uuidv4 } = require('uuid');
const cors = require('cors')
var cookieParser = require('cookie-parser');
var logger = require('morgan');
var upload = require('express-fileupload');
const admin = require('firebase-admin')
const { getStorage } = require('firebase-admin/storage');
const sharp = require('sharp');



const httpsEnabled = !!process.env.HTTPS

const port = process.env.PORT || (httpsEnabled ? 443 : 80);
const sslCertificatePath = process.env.SSLPATH || process.cwd()

var express = require('express');
const { storage } = require('firebase-admin');
var app = express();

app.use(cors())
// app.use(logger('dev'));
app.use(express.json());
app.use(express.urlencoded({ extended: false }));
app.use(cookieParser());
app.use(upload())
app.use(express.static(path.join(__dirname, 'docs/public')));
app.use('/messages', express.static(path.join(__dirname, 'docs/public')));
app.use('/post', express.static(path.join(__dirname, 'docs/public')));
app.use('/', express.static(path.join(__dirname, 'docs')));

var user = express.Router();


let server;
if (httpsEnabled) {
    server = https.createServer({
        key: fs.readFileSync(join(sslCertificatePath, 'privkey.pem')),
        cert: fs.readFileSync(join(sslCertificatePath, 'fullchain.pem'))
    }, app);
} else {
    var privateKey = fs.readFileSync('secure/key.pem', 'utf8');
    var certificate = fs.readFileSync('secure/cert.pem', 'utf8');

    var credentials = { key: privateKey, cert: certificate, passphrase: 'blue' };

    server = https.createServer(credentials, app);
}


server.listen(5000);
console.log("Server running on both ports!");