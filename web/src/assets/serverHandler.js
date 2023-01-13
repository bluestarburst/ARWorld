import { auth } from '../firebase-service.js';
import axios from 'axios';

export const httpPostAsync = async (theUrl, data, callback = console.log, error = console.log, unAuth = null) => {
    const token = await auth.currentUser.getIdToken();
    // console.log(token);

    return axios.post(theUrl, data, {
        headers:
            { authorization: `Bearer ${token}` }
    })
        .then(res => console.log(res.data)).then(callback);
}

export const httpPostImg = async (theUrl, data, img, callback = console.log, error = console.log, unAuth = null) => {
    const token = await auth.currentUser.getIdToken();
    const formData = new FormData();
    formData.append("selectedFile", img);
    formData.append("data", JSON.stringify(data));
    const response = await axios({
        method: "post",
        url: theUrl,
        data: formData,
        headers: { "Content-Type": "multipart/form-data", authorization: `Bearer ${token}` },
    });
    console.log(response.data);
    callback(response.data);
}

export function httpGetAsync(theUrl, data, callback = console.log) {
    var xmlHttp = new XMLHttpRequest();

    xmlHttp.open("GET", theUrl, true); // true for asynchronous 
    xmlHttp.setRequestHeader('Authorization', 'stuff');
    xmlHttp.setRequestHeader('Content-type', 'application/x-www-form-urlencoded');

    xmlHttp.onreadystatechange = function () {
        if (xmlHttp.readyState == 4 && xmlHttp.status == 200)
            callback(xmlHttp.responseText);
    }

    xmlHttp.send(data);
}

var cors_api_url = 'https://cors-anywhere.herokuapp.com/';
export function httpCORSPost(theUrl, data, callback = console.log, error = console.log, unAuth = null) {
    var x = new XMLHttpRequest();
    x.open("POST", cors_api_url + theUrl, true); // true for asynchronous 
    x.onload = x.onerror = function () {
        console.log("error")
    };
    x.setRequestHeader('Content-Type', 'application/x-www-form-urlencoded');

    x.onreadystatechange = function () {
        if (x.readyState == 4 && x.status == 200) {
            callback(x.responseText);
        }
        else if (x.status == 401 && unAuth) {
            unAuth(x.status)
        }
        else if (x.status != 200) {
            // console.log(xmlHttp.readyState);
            // console.log(xmlHttp.status);
            error(x.status)
        }
    }

    x.send(data);
}