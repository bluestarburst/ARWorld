import { createRoot } from 'react-dom/client'
import React, { useEffect, useRef, useState, Suspense } from 'react'
import { Canvas, useThree, useLoader, useFrame } from '@react-three/fiber'

import CircularProgress from '@mui/material/CircularProgress';

import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader'
import { FBXLoader } from 'three/examples/jsm/loaders/FBXLoader'
import { USDZLoader } from 'three/examples/jsm/loaders/USDZLoader'

import { GLTFExporter } from 'three/examples/jsm/exporters/GLTFExporter'
import { OBJExporter } from 'three/examples/jsm/exporters/OBJExporter'
// import { downloadJson } from './utils.js'

import { useControls, button, folder } from 'leva'
import JSZip from 'jszip'

// import computeBoundingSphere function from three.js
import { computeBoundingSphere } from 'three/src/math/Sphere.js'

import { OrbitControls } from '@react-three/drei'
// import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import * as THREE from "three";

import { validateBytes } from 'gltf-validator'

import Login from './elements/login.js'
import FilesDragAndDrop from './elements/FilesDragAndDrop.js'

import './assets/styles.scss'

import * as firebase from 'firebase/app';
// import 'firebase/auth';
import { getAuth, browserSessionPersistence, browserLocalPersistence, browserPopupRedirectResolver, initializeAuth, RecaptchaVerifier } from "firebase/auth";
import { getStorage, ref, uploadBytes, uploadBytesResumable } from "firebase/storage";
import { getFirestore, collection, addDoc, doc, setDoc, updateDoc, Timestamp } from "firebase/firestore";


import { Button, TextField } from '@mui/material';

import { WebIO } from '@gltf-transform/core';
import { DracoMeshCompression, KHRONOS_EXTENSIONS } from '@gltf-transform/extensions';
import { DRACOLoader } from 'three/examples/jsm/loaders/DRACOLoader.js';
import { Mesh } from 'three';

const firebaseConfig = {
    apiKey: "AIzaSyCA6g-FDnRDgmR9zQCuIOOuKifEPkHnAhE",
    authDomain: "ourworld-737cd.firebaseapp.com",
    projectId: "ourworld-737cd",
    storageBucket: "ourworld-737cd.appspot.com",
    messagingSenderId: "635119417753",
    appId: "1:635119417753:web:ca417bc4ad6644b9033b08",
    measurementId: "G-8G0LSS6B1C"
};

const gltfLoader = new GLTFLoader();
gltfLoader.setDRACOLoader(new DRACOLoader());



const app = firebase.initializeApp(firebaseConfig);

// create auth with local persistence

const auth = initializeAuth(app, {
    persistence: browserLocalPersistence,
    popupRedirectResolver: browserPopupRedirectResolver,
});

// (async () => {
//     await setPersistence(auth, browserLocalPersistence);
// })();

const storage = getStorage();
const db = getFirestore(app);

console.log(auth);

auth.useDeviceLanguage();



function Scene(props) {
    const gl = useThree((state) => state.gl)

    const { scene } = useThree()

    useEffect(() => {
        if (props.screenshot) {
            setTimeout(() => {

                var thing = gl.domElement.toDataURL('image/jpg').replace('image/jpg', 'image/octet-stream');



                var img = new Image();
                img.src = thing;
                img.onload = function () {
                    var canvas = document.createElement('canvas');
                    canvas.width = 200;
                    canvas.height = 200;
                    var ctx = canvas.getContext('2d');
                    ctx.drawImage(img, img.width / 2 - 100, img.height / 2 - 100, 200, 200, 0, 0, canvas.width, canvas.height);
                    var dataURL = canvas.toDataURL('image/jpg');
                    console.log(dataURL);
                    props.setThumbnail(dataURL);

                    // turn the dataURL into a blob
                    fetch(dataURL)
                        .then(res => res.blob())
                        .then(blob => {
                            // create a file from the blob
                            const file = new File([blob], "thumbnail.jpg", { type: 'image/jpg' });
                            console.log(file)
                            props.setThumbnailData(file);
                        });

                }

                console.log(thing);
                props.setScreenshot(false)
            }, 500)
        }
    }, [props.screenshot])

    useEffect(() => {
        console.log("setting scene")
        console.log(scene)
        props.setScene(scene)
    }, [scene])

    return (
        <>
            {props.children}
        </>
    )
}

const io = new WebIO()
    .registerExtensions([DracoMeshCompression])
    .registerDependencies({
        'draco3d.encoder': await new DracoEncoderModule(),
        'draco3d.decoder': await new DracoDecoderModule(),
    });

function Canv(props) {

    const [file, setFile] = useState(null);
    const [fileName, setFileName] = useState(null);

    const [model, setModel] = useState(null);
    const [type, setType] = useState(null);
    const [gltf, setGltf] = useState(null);

    const [thumbnail, setThumbnail] = useState(null);
    const [thumbnailData, setThumbnailData] = useState(null);

    const [screenshot, setScreenshot] = useState(false);

    const link = document.createElement('a');

    const onUpload = (files) => {
        setRendered(false)
        var file = files[0]
        setFile(file)
        setFileName(file.name.substring(0, file.name.lastIndexOf('.')));
        var path = URL.createObjectURL(file)
        console.log(file.name)
        console.log(path)
        if (file.name.endsWith(".gltf")) {

            gltfLoader.load(path, (model) => {
                setGltf(model)
                setModel(model.scene)
                setScreenshot(true)
            })


        } else if (file.name.endsWith(".glb")) {

            gltfLoader.load(path, (model) => {
                setGltf(model)
                setModel(model.scene)
                setScreenshot(true)
            })

        } else if (file.name.endsWith(".fbx")) {
            new FBXLoader().load(path, (model) => {
                setModel(model)
            })
        } else if (file.name.endsWith(".obj")) {
            new OBJLoader().load(path, (model) => {
                setModel(model)
            })
        } else if (file.name.endsWith(".usdz")) {
            new USDZLoader().load(path, (model) => {
                setModel(model)
            })
        } else {
            // the file is a zipped folder with a gltf file inside
            var zip = new JSZip();
            // get all files in zipped folder
            zip.loadAsync(file).then(function (zip) {
                // check if there is a .obj file in the zip
                // var gltfFile = zip.file(/\.gltf$/i)[0];
                console.log(zip)
            });

        }

    };

    const [tags, setTags] = useState([])

    function changeTags(e) {
        var tags = e.target.value.split(", ")
        setTags(tags)
    }

    function onKeyPressed(e) {
        console.log(e.key)
        // if (e.key === ' ') {
        //     e.preventDefault()
        //     e.stopPropagation()
        //     if (tags.length > 2) {
        //         return;
        //     }
        //     var lastTwoChars = e.target.value.substring(e.target.value.length - 2, e.target.value.length)
        //     if (lastTwoChars === ", ") {
        //         return;
        //     } else {
        //         e.target.value = e.target.value + ", "
        //         console.log(tags)
        //     }
        // } 
        if (e.key === ",") {
            e.preventDefault()
            e.stopPropagation()
            if (tags.length > 2) {
                return;
            }
            var lastTwoChars = e.target.value.substring(e.target.value.length - 2, e.target.value.length)
            if (lastTwoChars === ", ") {
                return;
            } else {
                e.target.value = e.target.value + ", "
                console.log(tags)
            }
        } else if (e.key === "Backspace") {

            var lastChar = e.target.value.substring(e.target.value.length - 2, e.target.value.length - 1)
            if (lastChar === ",") {
                e.preventDefault()
                e.stopPropagation()
                e.target.value = e.target.value.substring(0, e.target.value.length - 2)
            }
        }
    }

    const sceneRef = useRef();
    const scaleRef = useRef();

    const [scene, setScene] = useState(null);

    function saveArrayBuffer(buffer, filename) {
        save(new Blob([buffer], { type: 'application/octet-stream' }), filename);
    }

    async function save(blob, filename) {

        // turn blob into a file object
        const files = new File([blob], filename, { type: blob.type });
        const url = URL.createObjectURL(files);

        const document = await io.read(url);

        // Configure compression settings.
        document.createExtension(DracoMeshCompression)
            .setRequired(true)
            .setEncoderOptions({
                method: DracoMeshCompression.EncoderMethod.EDGEBREAKER,
                encodeSpeed: 5,
                decodeSpeed: 5,
            });

        // Create compressed GLB, in an ArrayBuffer.
        const arrayBuffer = await io.writeBinary(document); // ArrayBuffer

        // create firestore document
        var docs = await addDoc(collection(db, "objects"), {
            name: fileName,
            user: auth.currentUser.uid,
            creations: 0,
            tags: tags,
            timestamp: Timestamp.now()
        }).then((docRef) => {
            console.log("Document written with ID: ", docRef.id);
            // add file to user document: users/{user}/objects/{object}
            setDoc(doc(db, "users", auth.currentUser.uid, "objects", docRef.id), {
                name: fileName,
                user: auth.currentUser.uid,
                tags: tags,
                id: docRef.id,
                timestamp: Timestamp.now()
            })


            updateDoc(doc(db, "objects", docRef.id), {
                id: docRef.id
            })

            // add file to storage
            var storageRef = ref(storage, "users/" + auth.currentUser.uid + "/objects/" + docRef.id + ".glb");
            var uploadTask = uploadBytesResumable(storageRef, arrayBuffer);
            uploadTask.on('state_changed',
                (snapshot) => {
                    // Observe state change events such as progress, pause, and resume
                    // Get task progress, including the number of bytes uploaded and the total number of bytes to be uploaded
                    var progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
                    console.log('Upload is ' + progress + '% done');
                    switch (snapshot.state) {
                        case 'paused':
                            console.log('Upload is paused');
                            break;
                        case 'running':
                            console.log('Upload is running');
                            break;
                    }
                },
                (error) => {
                    // Handle unsuccessful uploads
                }
            );

            //upload thumbnail to same locataion as object
            var storageRef2 = ref(storage, "users/" + auth.currentUser.uid + "/objects/" + docRef.id + ".jpg");

            var uploadTask2 = uploadBytesResumable(storageRef2, thumbnailData);
            uploadTask2.on('state_changed',
                (snapshot) => {
                    // Observe state change events such as progress, pause, and resume
                    // Get task progress, including the number of bytes uploaded and the total number of bytes to be uploaded
                    var progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
                    console.log('Upload is ' + progress + '% done');
                    switch (snapshot.state) {
                        case 'paused':
                            console.log('Upload is paused');
                            break;
                        case 'running':
                            console.log('Upload is running');
                            break;
                    }
                },
                (error) => {
                    // Handle unsuccessful uploads
                }
            );

        }).catch((error) => {
            console.error("Error adding document: ", error);
        });

        setModel(null);

        // Save the compressed GLB.
        var blobs = new Blob([arrayBuffer], { type: 'application/octet-stream' })

        link.href = URL.createObjectURL(blobs);
        link.download = "scene.glb";
        // link.click();
    }

    function saveString(text, filename) {
        save(new Blob([text], { type: "text/plain" }), filename);
    }

    const handleExport = () => {

        const exporter = new GLTFExporter();
        exporter.parse(sceneRef.current || scene, (gltf) => {
            if (gltf instanceof ArrayBuffer) {
                saveArrayBuffer(gltf, "scene.glb");
            } else {
                console.log(gltf)
                const gltfString = JSON.stringify(gltf);
                saveString(gltfString, "scene.gltf");
            }
        }, { binary: true });
    };

    const geomUpper = new THREE.SphereGeometry(1, 28, 28)
    const geomLower = new THREE.SphereGeometry(0.3, 28, 28)
    const orange = new THREE.MeshLambertMaterial({ color: "orange" })
    const orange2 = new THREE.MeshLambertMaterial({ color: "orange", transparent: true, opacity: 0.1 })
    const pink2 = new THREE.MeshLambertMaterial({ color: "pink", transparent: true, opacity: 0.5 })

    const [rendered, setRendered] = useState(false)

    function onceRendered() {

        var mroot = scaleRef.current;
        var bbox = new THREE.Box3().setFromObject(mroot);
        var cent = bbox.getCenter(new THREE.Vector3());
        var size = bbox.getSize(new THREE.Vector3());

        //Rescale the object to normalized space
        var maxAxis = Math.max(size.x, size.y, size.z);
        mroot.scale.multiplyScalar(1.0 / maxAxis);
        bbox.setFromObject(mroot);
        bbox.getCenter(cent);
        bbox.getSize(size);
        //Reposition to 0,halfY,0
        mroot.position.copy(cent).multiplyScalar(-1);
        mroot.position.y += (size.y * 0.5);
        setRendered(true)
        setScreenshot(true)
    }

    return (
        <div className='page'>
            {model != null ? <div className='page'>
                <Canvas gl={{ preserveDrawingBuffer: true }} camera={{ position: [-3, 3, -3] }}>

                    <Scene setScreenshot={setScreenshot} screenshot={screenshot} setThumbnail={setThumbnail} thumbnail={thumbnail} setScene={setScene} setThumbnailData={setThumbnailData}>
                        <ambientLight />
                        <pointLight position={[10, 10, 10]} />
                        <OrbitControls target={[0, 0.5, 0]} />
                        {!screenshot ? <>
                            <gridHelper />
                            <axesHelper scale={[3, 3, 3]} />
                        </> : <></>}

                        <Suspense fallback={null}>
                            {/* <group ref={gltf} ></group> */}
                            <group ref={sceneRef} onAfterRender={onceRendered} >
                                <group ref={scaleRef}>
                                    <Model object={model} onRender={onceRendered} rendered={rendered} />
                                </group>

                            </group>


                            {/* <primitive group={gltf} /> */}
                        </Suspense>
                    </Scene>

                </Canvas>
                <div className='screenshot'>
                </div>
                <div className='instructions'>
                    <b>
                        <p>✅   Upload 3D Model (<a onClick={_ => { setModel(null) }}>choose another</a>)</p>
                        <p>{thumbnail ? "✅" : <CircularProgress />} Take Screenshot for Thumbnail</p>
                    </b>
                </div>
                <div className='preview'>
                    <img className={thumbnail ? "" : "hidden"} src={thumbnail} />
                    <Button variant="contained" color="primary" onClick={_ => { setThumbnail(null); setScreenshot(true) }}>Take Screenshot</Button>
                </div>
                <div className='upload'>
                    <TextField required id="outlined-basic" label="Title" variant="outlined" className='w-100' autoCorrect={false} autoComplete={false} defaultValue={fileName} />
                    <TextField id="outlined-basic" label="Tags (separated by commas)" variant="outlined" autoCorrect={false} autoComplete={false} className='w-100' onChange={changeTags} onKeyDown={onKeyPressed} />
                    <Button variant="contained" color="primary" onClick={handleExport} >Upload</Button>
                </div>

            </div> : <>
                <FilesDragAndDrop
                    onUpload={onUpload}
                    count={1}

                >
                    <div className='FilesDragAndDroparea'>
                        Hey, drop me some files
                        <br />
                        (.glb)
                        <span
                            role='img'
                            aria-label='emoji'
                            className='areaicon'
                        >
                            &#128526;
                        </span>
                    </div>

                </FilesDragAndDrop>
                <div className='signout'>
                    <Button variant="contained" color="primary" onClick={_ => {
                        auth.signOut()
                    }}>Sign Out</Button>
                </div>
            </>
            }

        </div>
    );

}

function Model(props) {

    useEffect(() => {
        if (!props.rendered) {
            props.onRender()
        }
    })

    return (
        <Suspense>
            {props.object != null ? <primitive object={props.object} /> : <></>}
        </Suspense>
    )
}

function Page(props) {
    return (<div className='page'>
        {props.children}
    </div>)
}

function App() {
    const [state, setState] = useState("loading")
    const [user, setUser] = useState(null)
    const [current, setCurrent] = useState(<Page>
        <CircularProgress />
    </Page>)

    useEffect(() => {
        auth.onAuthStateChanged(function (user) {
            if (user) {
                // User is signed in.
                setUser(user)
                console.log("signed in")
                setCurrent(<Canv user={user} />)
            } else {
                // No user is signed in.
                console.log("signed out")
                setCurrent(<Login app={app} auth={auth} />)
            }
        });
    }, [])

    return (current)
}

createRoot(document.getElementById('root')).render(<App />)

/*
onClick={() => {
                        console.log("uploading")
                        var storageRef = firebase.storage().ref();
                        var fileRef = storageRef.child('models/' + props.user.uid + '/thumbnail.png');
                        var uploadTask = fileRef.putString(thumbnail, 'data_url');
                        uploadTask.on('state_changed', function (snapshot) {
                            // Observe state change events such as progress, pause, and resume
                            // Get task progress, including the number of bytes uploaded and the total number of bytes to be uploaded
                            var progress = (snapshot.bytesTransferred / snapshot.totalBytes) * 100;
                            console.log('Upload is ' + progress + '% done');
                            switch (snapshot.state) {
                                case firebase.storage.TaskState.PAUSED: // or 'paused'
                                    console.log('Upload is paused');    
                                    break;
                                case firebase.storage.TaskState.RUNNING: // or 'running'
                                    console.log('Upload is running');
                                    break;
                            }
                        }, function (error) {
                            // Handle unsuccessful uploads
                        }, function () {
                            // Handle successful uploads on complete
                            // For instance, get the download URL: https://firebasestorage.googleapis.com/...
                            uploadTask.snapshot.ref.getDownloadURL().then(function (downloadURL) {
                                console.log('File available at', downloadURL);
                                props.setThumbnail(downloadURL)
                            });
                        });
                    }}
                    */