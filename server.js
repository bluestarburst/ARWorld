var admin = require("firebase-admin");

var serviceAccount = require("./secure/ourworld-admin.json");

admin.initializeApp({
    credential: admin.credential.cert(serviceAccount)
});