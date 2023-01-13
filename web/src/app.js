import { createRoot } from 'react-dom/client'
import React, { useEffect, useRef, useState, Suspense } from 'react'
import { Canvas, useThree, useLoader, useFrame } from '@react-three/fiber'

import CircularProgress from '@mui/material/CircularProgress';

import { GLTFLoader } from 'three/examples/jsm/loaders/GLTFLoader'
import { OBJLoader } from 'three/examples/jsm/loaders/OBJLoader'
import { FBXLoader } from 'three/examples/jsm/loaders/FBXLoader'

import { OrbitControls } from '@react-three/drei'
// import { OrbitControls } from "three/examples/jsm/controls/OrbitControls";
import * as THREE from "three";

import { validateBytes, } from 'gltf-validator'

import Login from './elements/login.js'
import FilesDragAndDrop from './elements/FilesDragAndDrop.js'

import './assets/styles.scss'

import * as firebase from 'firebase/app';
// import 'firebase/auth';
import { getAuth, browserSessionPersistence, browserPopupRedirectResolver, initializeAuth, RecaptchaVerifier } from "firebase/auth";

const firebaseConfig = {
    apiKey: "AIzaSyCA6g-FDnRDgmR9zQCuIOOuKifEPkHnAhE",
    authDomain: "ourworld-737cd.firebaseapp.com",
    projectId: "ourworld-737cd",
    storageBucket: "ourworld-737cd.appspot.com",
    messagingSenderId: "635119417753",
    appId: "1:635119417753:web:ca417bc4ad6644b9033b08",
    measurementId: "G-8G0LSS6B1C"
};



const app = firebase.initializeApp(firebaseConfig);

const auth = initializeAuth(app, {
    persistence: browserSessionPersistence,
    popupRedirectResolver: browserPopupRedirectResolver,
});

console.log(auth);

auth.useDeviceLanguage();


function Box(props) {
    // This reference gives us direct access to the THREE.Mesh object
    const ref = useRef()
    // Hold state for hovered and clicked events
    const [hovered, hover] = useState(false)
    const [clicked, click] = useState(false)
    // Subscribe this component to the render-loop, rotate the mesh every frame
    useFrame((state, delta) => (ref.current.rotation.x += delta))
    // Return the view, these are regular Threejs elements expressed in JSX
    return (
        <mesh
            {...props}
            ref={ref}
            scale={clicked ? 1.5 : 1}
            onClick={(event) => click(!clicked)}
            onPointerOver={(event) => hover(true)}
            onPointerOut={(event) => hover(false)}>
            <boxGeometry args={[1, 1, 1]} />
            <meshStandardMaterial color={hovered ? 'hotpink' : 'orange'} />
        </mesh>
    )
}

function Canv(props) {

    const [file, setFile] = useState(null);

    const [model, setModel] = useState(null);
    const [type, setType] = useState(null);
    const [gltf, setGltf] = useState(null);

    useState(() => {
        console.log("loading model")
        setModel(null)
    }, [])

    useState(() => {
        console.log("loading model")
        console.log(model)
    }, [model])

    const onUpload = (files) => {
        var file = files[0]
        var path = URL.createObjectURL(file)
        console.log(file.name)
        if (file.name.endsWith(".gltf")) {
            new GLTFLoader().load(path, (gltfs) => {
                setGltf(gltfs)
                setModel(gltfs.scene)
            })
        } else if (file.name.endsWith(".glb")) {
            new GLTFLoader().load(path, (gltfs) => {
                setGltf(gltfs)
                setModel(gltfs.scene)
            })
        } else if (file.name.endsWith(".fbx")) {
            new FBXLoader().load(path, setModel)
        } else if (file.name.endsWith(".obj")) {
            new OBJLoader().load(path, setModel)
        } else {
            // the file is a zipped folder with a gltf file inside
            var zip = new JSZip();
        }

    };

    return (
        <div className='page'>
            {model != null ? <Canvas>
                <ambientLight />
                <pointLight position={[10, 10, 10]} />
                <OrbitControls />
                <Suspense fallback={null}>
                    <primitive object={model} rotation={[0, Math.PI * 3 / 4, 0]} />
                </Suspense>
            </Canvas> :
                <FilesDragAndDrop
                    onUpload={onUpload}
                    count={1}

                >
                    <div className='FilesDragAndDroparea'>
                        Hey, drop me some files
                        <span
                            role='img'
                            aria-label='emoji'
                            className='areaicon'
                        >
                            &#128526;
                        </span>
                    </div>
                </FilesDragAndDrop>
            }
        </div>
    );

    // return (
    //     <Canvas>
    //         <ambientLight />
    //         <pointLight position={[10, 10, 10]} />
    //         <Box position={[-1.2, 0, 0]} />
    //         <Box position={[1.2, 0, 0]} />
    //     </Canvas>
    // )
}

function Page(props) {
    return (<div className='page'>
        {props.children}
    </div>)
}

function App() {
    const [state, setState] = useState("loading")
    const [current, setCurrent] = useState(<Page>
        <CircularProgress />
    </Page>)

    useEffect(() => {
        auth.onAuthStateChanged(function (user) {
            if (user) {
                // User is signed in.
                console.log("signed in")
                setCurrent(<Canv />)
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