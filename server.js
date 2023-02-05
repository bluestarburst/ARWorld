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

// delete collection from firestore
function deleteCollection(db, collectionPath, batchSize) {
    var collectionRef = db.collection(collectionPath);
    var query = collectionRef.orderBy('__name__').limit(batchSize);

    return new Promise((resolve, reject) => {
        deleteQueryBatch(db, query, batchSize, resolve, reject);
    });
}

function deleteQueryBatch(db, query, batchSize, resolve, reject) {
    query.get()
        .then((snapshot) => {
            // When there are no documents left, we are done
            if (snapshot.size == 0) {
                return 0;
            }

            // Delete documents in a batch
            var batch = db.batch();
            snapshot.docs.forEach((doc) => {
                batch.delete(doc.ref);
            });

            return batch.commit().then(() => {
                return snapshot.size;
            });
        }).then((numDeleted) => {
            if (numDeleted === 0) {
                resolve();
                return;
            }

            // Recurse on the next process tick, to avoid
            // exploding the stack.
            process.nextTick(() => {
                deleteQueryBatch(db, query, batchSize, resolve, reject);
            });
        })
        .catch(reject);
}

function deleteMap(id) {
    db.collection('maps').doc(id).get().then((doc) => {
        if (doc.exists) {
            db.collection('maps').doc(id).collection('chunks').get().then((snapshot) => {
                snapshot.forEach((doc) => {
                    deleteCollection(db, 'maps/' + id + '/chunks/' + doc.id + '/objects', 100);
                    deleteCollection(db, 'maps/' + id + '/chunks/' + doc.id + '/posters', 100);
                    deleteCollection(db, 'maps/' + id + '/chunks/' + doc.id + '/filters', 100);
                    deleteCollection(db, 'maps/' + id + '/chunks/' + doc.id + '/spotlights', 100);
                });
                deleteCollection(db, 'maps/' + id + '/chunks', 100);
            }).catch((error) => {
                console.log("Error getting document:", error);
            });

            db.collection('maps').doc(id).delete().then(() => {
                console.log("Document successfully deleted!");
            }).catch((error) => {
                console.error("Error removing document: ", error);
            });

            // delete data from storage
            var file = bucket.file('maps/' + id + '.worldmap');
            file.delete().then(() => {
                console.log('File deleted successfully');
            }).catch((error) => {
                console.log('Error deleting file:', error);
            });

        } else {
            console.log("No such document!");
        }
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
}


function deleteAllMaps() {
    db.collection('maps').get().then((snapshot) => {
        snapshot.forEach((doc) => {
            deleteMap(doc.id);
        });
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
}

function deleteUserObjOfType(user, type) {
    db.collection('users').doc(user).collection(type).get().then((snapshot) => {
        snapshot.forEach((doc) => {
            deletePlane(doc.id, type);
        });
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
}

function deleteAllOfType(type) {
    db.collection(type).get().then((snapshot) => {
        snapshot.forEach((doc) => {
            deletePlane(doc.id, type);
        });
    }).catch((error) => {
        console.log("Error getting document:", error);
    });
}




// deleteObject('Mug42xLHsrR11lgYdGeX');
// deleteObject('maF0DgAK7ysXv7x1Lv0L');
// deleteObject('catoon_coffee');
// deleteObject('rAeEtemmVDrlQrDpyW43');
// deleteObject('vwlguVspiK13pnNyMDhA');

deleteAllMaps();
// deleteAllOfType('images');
// deleteAllOfType('stickers');
// deleteAllOfType('posters');