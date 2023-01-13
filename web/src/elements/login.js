import { Container, Button, Form, Card } from "react-bootstrap";
import { FormControl, InputLabel, Input, FormHelperText, TextField } from "@mui/material";

import { RecaptchaVerifier, GoogleAuthProvider, signInWithRedirect, TwitterAuthProvider, createUserWithEmailAndPassword, sendSignInLinkToEmail, PhoneAuthProvider, signInWithPhoneNumber } from "firebase/auth";

// import firebase from "firebase";

import cube from "../assets/imgs/cube.gif";

import React, { useState, useRef, useEffect } from "react";

const googleProvider = new GoogleAuthProvider();
const twitterProvider = new TwitterAuthProvider();
const phoneProvider = new PhoneAuthProvider();

export default function Login(props) {

    const [showLoginModal, setShowLoginModal] = useState(false)
    const [showSignUpModal, setShowSignUpModal] = useState(false)

    function onDismiss() {
        setShowLoginModal(false)
        setShowSignUpModal(false)
    }

    return (

        <div className="login" >
            <LoginModal show={showLoginModal} dismiss={onDismiss} app={props.app} auth={props.auth} />

            <img src={back} className="back" />

            <div className="inline login-header">
                <h1><b>OurWorlds!</b></h1>
            </div>

            <h5>Join a community that brings worlds together </h5>

            <div app={props.app} onClick={() => { setShowLoginModal(true) }} className="third-party google">
                {/* <img src={google} alt="Google" /> */}
                <p>Sign in with a Phone Number</p>
            </div>

            {/* <br />

            <h4>Already have an account?</h4>
            <div className="third-party email" onClick={_ => { setShowLoginModal(true) }}>
                <p>Sign In</p>
            </div> */}

        </div >


    )
}

function AuthProviderButton(props) {

    function onSignIn() {
        signInWithRedirect(props.auth, props.provider);
    }

    return (
        <div className={"third-party " + props.className} onClick={onSignIn} >
            {props.children}
        </div>
    );
}

function LoginModal(props) {

    const [phone, setPhone] = useState("")
    const [authCode, setAuthCode] = useState("")

    const [isAuth, setIsAuth] = useState(false)

    const [error, setError] = useState(false)

    const authRef = useRef(null)

    function onChangePhone(e) {
        e.target.value = e.target.value.replace(/[^0-9]/g, '')
        setPhone(e.target.value)
    }

    function onChangeAuthCode(e) {
        e.target.value = e.target.value.replace(/[^0-9]/g, '')
        setAuthCode(e.target.value)
    }

    const [token, setToken] = useState("");

    useEffect(() => {
        window.recaptchaVerifier = new RecaptchaVerifier('sign-in-button', {
            'size': 'invisible',
            'callback': (response) => {
                // reCAPTCHA solved, allow signInWithPhoneNumber.
                onSignInSubmit();
            }
        }, props.auth);
    }, []);

    useEffect(() => {
        if (isAuth) {
            authRef.current.focus()
            authRef.current.value = ""
        }
    }, [isAuth])

    function onSignIn() {

        const appVerifier = window.recaptchaVerifier;
        setError(true)

        // +1(650)-555-3434

        // format phone into +1(650)-555-3434
        var newphone = "+1(" + phone.substring(0, 3) + ")-" + phone.substring(3, 6) + "-" + phone.substring(6, 10)

        signInWithPhoneNumber(props.auth, newphone, appVerifier)
            .then((confirmationResult) => {
                // SMS sent. Prompt user to type the code from the message, then sign the
                // user in with confirmationResult.confirm(code).
                window.confirmationResult = confirmationResult;
                console.log(confirmationResult)
                setError(false)
                setIsAuth(true)

                // ...
            }).catch((error) => {
                // Error; SMS not sent
                console.log(error)
                setError(false)
                // ...
            });
    }

    function onAuthCode() {
        setError(true)
        window.confirmationResult.confirm(authCode).then((result) => {
            // User signed in successfully.
            const user = result.user;
            setError(false)
            props.dismiss();
            // ...
        }).catch((error) => {
            // User couldn't sign in (bad verification code?)
            setError(false)
            // ...
        });
    }

    // 


    return (
        <div className={props.show ? "login-modal" : "login-modal none"} >
            <div className="login-modal-content" id="login-modal" onClick={_ => { }}>
                <h1>Sign In</h1>
                {!isAuth ? <>
                    <p>Enter your phone number to continue</p>
                    <div className="login-modal-body mt-2">
                        <TextField id="phone" label="Phone Number" variant="outlined" className="w-100" onChange={onChangePhone} />
                        {/* <TextField id="authCode" label="Auth Code" variant="outlined" className="w-100" onChange={onChangeAuthCode} /> */}
                        <img src={cube} />
                    </div>
                    <Button className="w-100 corner p-2" id="sign-in-button" onClick={onSignIn} disabled={(phone.length != 10 || error)} >
                        Sign Up
                    </Button>
                </> : <>
                    <p>Enter the code sent to your phone</p>
                    <div className="login-modal-body mt-2">
                        <TextField id="authCode" label="Auth Code" variant="outlined" className="w-100" onChange={onChangeAuthCode} inputRef={authRef} />
                        <img src={cube} />
                    </div>
                    <Button className="w-100 corner p-2" id="sign-in-button" onClick={onAuthCode} disabled={(authCode.length != 6 || error)} >
                        Verify
                    </Button>
                </>}
            </div>
            <div className="back" onClick={props.dismiss}>

            </div>
        </div>
    )

}
