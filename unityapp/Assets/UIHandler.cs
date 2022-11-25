using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
// using UnityEngine.UI;
using UnityEngine.UIElements;

public class UIHandler : MonoBehaviour
{
    public UIDocument Document;

    public string phone = "";

    public string receivedCode = "";

    Firebase.Auth.FirebaseAuth auth;

    Firebase.Auth.FirebaseUser user;

    // Set the phone authentication timeout to a minute.
    private uint phoneAuthTimeoutMs = 60 * 1000;

    // The verification id needed along with the sent code for phone authentication.
    private string phoneAuthVerificationId;

    // Whether to sign in / link or reauthentication *and* fetch user profile data.
    protected bool signInAndFetchProfile = false;

    // Handle initialization of the necessary firebase modules:
    void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        // auth.StateChanged += AuthStateChanged;
        // AuthStateChanged(this, null);
    }

    private void Awake()
    {
        InitializeFirebase();

        // get document root and add a button
        var root = Document.rootVisualElement;
        var body = root.Q<GroupBox>("Body");

        var input = new TextField("Input");
        input.value = "phone number";
        input.label = "";
        input.AddToClassList("my-input");
        input
            .RegisterCallback<ChangeEvent<string>>(evt =>
            {
                Debug.Log("Input changed: " + evt.newValue);
                if (evt.newValue.Length == 1 && evt.newValue[0] != '(')
                {
                    input.value = "(" + evt.newValue;
                    using (
                        var evts =
                            KeyboardEventBase<KeyDownEvent>
                                .GetPooled('\0',
                                KeyCode.RightArrow,
                                EventModifiers.FunctionKey)
                    )
                    {
                        input.SendEvent (evts);
                    }
                }
                else if (evt.newValue.Length == 4)
                {
                    input.value = evt.newValue + ")";
                    using (
                        var evts =
                            KeyboardEventBase<KeyDownEvent>
                                .GetPooled('\0',
                                KeyCode.RightArrow,
                                EventModifiers.FunctionKey)
                    )
                    {
                        input.SendEvent (evts);
                    }
                }
                else if (evt.newValue.Length == 8)
                {
                    input.value = evt.newValue + "-";
                    using (
                        var evts =
                            KeyboardEventBase<KeyDownEvent>
                                .GetPooled('\0',
                                KeyCode.RightArrow,
                                EventModifiers.FunctionKey)
                    )
                    {
                        input.SendEvent (evts);
                    }
                }
                string a = input.value;
                phone = string.Empty;
                int val;

                for (int i = 0; i < a.Length; i++)
                {
                    if (Char.IsDigit(a[i])) phone += a[i];
                }
            });
        BlinkingCursor (input);

        body.Add (input);

        var button = new Button();
        button.text = "next";
        button.AddToClassList("button");
        button.AddToClassList("my-input");
        button
            .RegisterCallback<ClickEvent>(evt =>
            {
                VerifyPhoneNumber();
            });
        body.Add (button);

        var group = new GroupBox();
        group.AddToClassList("h");
        body.Add (group);

        addInputs(group, 6);

        var button2 = new Button();
        button2.text = "verify";
        button2.AddToClassList("button");
        button2.AddToClassList("my-input");
        button2
            .RegisterCallback<ClickEvent>(evt =>
            {
                VerifyReceivedPhoneCode();
            });
        body.Add (button2);
    }

    // Begin authentication with the phone number.
    protected void VerifyPhoneNumber()
    {
        var phoneAuthProvider =
            Firebase.Auth.PhoneAuthProvider.GetInstance(auth);
        Debug.Log("phone: " + phone);
        phoneAuthProvider
            .VerifyPhoneNumber(phone,
            phoneAuthTimeoutMs,
            null,
            verificationCompleted: (cred) =>
            {
                Debug.Log("Phone Auth, auto-verification completed");
                if (signInAndFetchProfile)
                {
                    auth
                        .SignInAndRetrieveDataWithCredentialAsync(cred)
                        .ContinueWithOnMainThread(HandleSignInWithSignInResult);
                }
                else
                {
                    auth
                        .SignInWithCredentialAsync(cred)
                        .ContinueWithOnMainThread(HandleSignInWithUser);
                }
            },
            verificationFailed: (error) =>
            {
                Debug.Log("Phone Auth, verification failed: " + error);
            },
            codeSent: (id, token) =>
            {
                phoneAuthVerificationId = id;
                Debug.Log("Phone Auth, code sent");
            },
            codeAutoRetrievalTimeOut: (id) =>
            {
                Debug.Log("Phone Auth, auto-verification timed out");
            });
    }

    // Sign in using phone number authentication using code input by the user.
    protected void VerifyReceivedPhoneCode()
    {
        var phoneAuthProvider =
            Firebase.Auth.PhoneAuthProvider.GetInstance(auth);

        // receivedCode should have been input by the user.
        var cred =
            phoneAuthProvider
                .GetCredential(phoneAuthVerificationId, receivedCode);
        if (signInAndFetchProfile)
        {
            auth
                .SignInAndRetrieveDataWithCredentialAsync(cred)
                .ContinueWithOnMainThread(HandleSignInWithSignInResult);
        }
        else
        {
            auth
                .SignInWithCredentialAsync(cred)
                .ContinueWithOnMainThread(HandleSignInWithUser);
        }
    }

    // Called when a sign-in without fetching profile data completes.
    void HandleSignInWithUser(Task<Firebase.Auth.FirebaseUser> task)
    {
        //   EnableUI();
        if (LogTaskCompletion(task, "Sign-in"))
        {
            Debug.Log(String.Format("{0} signed in", task.Result.DisplayName));
        }
    }

    // Called when a sign-in with profile data completes.
    void HandleSignInWithSignInResult(Task<Firebase.Auth.SignInResult> task)
    {
        //   EnableUI();
        if (LogTaskCompletion(task, "Sign-in"))
        {
            // DisplaySignInResult(task.Result, 1);
            Debug
                .Log(String
                    .Format("{0} signed in", task.Result.User.DisplayName));
        }
    }

    private void addInputs(GroupBox group, int num)
    {
        for (int i = 0; i < num; i++)
        {
            var input = new TextField("Input");
            input.name = "input" + i;
            input.value = "0";
            input.label = "";
            input.AddToClassList("my-input");
            input.AddToClassList("digit");

            input
                .RegisterCallback<FocusEvent>(evt =>
                {
                    if (input.value == "0  " || input.value == "")
                    {
                        input.value = " ";

                        // 123456
                        // set cursor to the end of the text field
                        input.SelectRange(0, 1);

                        // var fromIndex = 1;
                        // var toIndex = 1;
                        // input.SelectRange (fromIndex, toIndex);
                        using (
                            var evts =
                                KeyboardEventBase<KeyDownEvent>
                                    .GetPooled('\0',
                                    KeyCode.RightArrow,
                                    EventModifiers.FunctionKey)
                        )
                        {
                            input.SendEvent (evts);
                        }

                        // input.SelectRange(1, 1);
                    }
                    Debug.Log("input focused");
                });

            input
                .RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    int nextNum = Int32.Parse(input.name.Substring(5, 1)) + 1;

                    if (nextNum == 1)
                    {
                        if (input.value.Length >= 6)
                        {
                            var save = input.value;
                            for (int i = 0; i < 6; i++)
                            {
                                input.parent.Q<TextField>("input" + i).value =
                                    " " + save[i].ToString();
                                input.parent.Q<TextField>("input" + 5).Focus();
                            }
                            input.parent.Q<TextField>("input5").Focus();
                            return;
                        }
                    }

                    if (
                        input.value != "" &&
                        input.value != " " &&
                        input.value != "0  "
                    )
                    {
                        input.parent.Q<TextField>("input" + nextNum).Focus();
                    }
                });

            input
                .RegisterCallback<KeyDownEvent>(evt =>
                {
                    if (evt.keyCode == KeyCode.Backspace)
                    {
                        if (input.value == "")
                        {
                            input.value = "0  ";
                            input
                                .parent
                                .Q<TextField>("input" +
                                (Int32.Parse(input.name.Substring(5, 1)) - 1))
                                .Focus();
                        }
                    }
                });

            BlinkingCursor (input);
            group.Add (input);
        }
    }

    public static void BlinkingCursor(TextField tf)
    {
        tf
            .schedule
            .Execute(() =>
            {
                if (tf.ClassListContains("transparentCursor"))
                    tf.RemoveFromClassList("transparentCursor");
                else
                    tf.AddToClassList("transparentCursor");
            })
            .Every(800);
    }

    // Log the result of the specified task, returning true if the task
    // completed successfully, false otherwise.
    protected bool LogTaskCompletion(Task task, string operation)
    {
        bool complete = false;
        if (task.IsCanceled)
        {
            Debug.Log(operation + " canceled.");
        }
        else if (task.IsFaulted)
        {
            Debug.Log(operation + " encounted an error.");
            foreach (Exception
                exception
                in
                task.Exception.Flatten().InnerExceptions
            )
            {
                string authErrorCode = "";
                Firebase.FirebaseException firebaseEx =
                    exception as Firebase.FirebaseException;
                if (firebaseEx != null)
                {
                    authErrorCode =
                        String
                            .Format("AuthError.{0}: ",
                            ((Firebase.Auth.AuthError) firebaseEx.ErrorCode)
                                .ToString());
                }
                Debug.Log(authErrorCode + exception.ToString());
            }
        }
        else if (task.IsCompleted)
        {
            Debug.Log(operation + " completed");
            complete = true;
        }
        return complete;
    }
}
