var admin = require("firebase-admin");
const { getStorage } = require('firebase-admin/storage');

var serviceAccount = require("./secure/ourworld-admin.json");

admin.initializeApp({
    credential: admin.credential.cert(serviceAccount),
    storageBucket: "ourworld-737cd.appspot.com"
});

var db = admin.firestore();

// get bucket from firebase storage
const bucket = getStorage().bucket();

function deleteObject(id) {

    // get data from firestore
    db.collection('objects').doc(id).get().then((doc) => {
        if (doc.exists) {

            var user = doc.data().user;
            console.log(user);

            // delete data from storage
            // 
            var file = bucket.file('users/' + user + '/objects/' + id + '.jpg');
            file.delete().then(() => {
                console.log('File deleted successfully');
            }).catch((error) => {
                console.log('Error deleting file:', error);
            });

            var file2 = bucket.file('users/' + user + '/objects/' + id + '.glb');
            file2.delete().then(() => {
                console.log('File deleted successfully');
            }).catch((error) => {
                console.log('Error deleting file:', error);
            });

            db.collection('users').doc(user).collection('objects').doc(id).delete().then(() => {
                console.log("Document successfully deleted!");
            }).catch((error) => {
                console.error("Error removing document: ", error);
            });

            // delete data from firestore
            db.collection('objects').doc(id).delete().then(() => {
                console.log("Document successfully deleted!");
            }).catch((error) => {
                console.error("Error removing document: ", error);
            });
        } else {
            console.log("No such document!");
        }
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
}

function deletePlane(id, type) {
    db.collection(type).doc(id).get().then((doc) => {
        if (doc.exists) {

            var user = doc.data().user;
            console.log(user);

            // delete data from storage
            // 
            var file = bucket.file('users/' + user + '/' + type + '/' + id + '.jpg');
            file.delete().then(() => {
                console.log('File deleted successfully');
            }).catch((error) => {
                console.log('Error deleting file:', error);
            });

            var file2 = bucket.file('users/' + user + '/' + type + '/' + id + '.png');
            file2.delete().then(() => {
                console.log('File deleted successfully');
            }).catch((error) => {
                console.log('Error deleting file:', error);
            });

            db.collection('users').doc(user).collection(type).doc(id).delete().then(() => {
                console.log("Document successfully deleted!");
            }).catch((error) => {
                console.error("Error removing document: ", error);
            });

            // delete data from firestore
            db.collection(type).doc(id).delete().then(() => {
                console.log("Document successfully deleted!");
            }).catch((error) => {
                console.error("Error removing document: ", error);
            });
        } else {
            console.log("No such document!");
        }
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
}

function deleteSticker(id) {
    deletePlane(id, 'stickers');
}

function deletePoster(id) {
    deletePlane(id, 'posters');
}

function deleteImage(id) {
    deletePlane(id, 'images');
}












deleteObject('men7BSvRZTIIcKehVvw5');
