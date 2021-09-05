using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using UnityEngine.UI;
using System;
using Google;
using System.Threading.Tasks;

public class GoogleManager : MonoBehaviour
{
    private UIManager theUIManager;
    private FirebaseManager theFirebaseManager;

    //Google variables
    [Header("Google variables")]
    [SerializeField] private string webClientId = "<your client id here>";
    private GoogleSignInConfiguration configuration;

    //Awake function is called when the script is being loaded, configuration of Google
    private void Awake()
    {
        theUIManager = FindObjectOfType<UIManager>();
        theFirebaseManager = FindObjectOfType<FirebaseManager>();
        configuration = new GoogleSignInConfiguration { WebClientId = webClientId, RequestEmail = true, RequestIdToken = true };
    }

    //Function to Login Button using sign in method of Google
    public void LoginButtonForGoogle()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }

    //Function to complete the login task
    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        //If there was an error
        if (task.IsFaulted)
        {
            using (IEnumerator<Exception> enumerator = task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error = (GoogleSignIn.SignInException)enumerator.Current;
                    theUIManager.warningLoginText.text = "Got Error: " + error.Status + " " + error.Message;
                }
                else
                {
                    theUIManager.warningLoginText.text = "Got Unexpected Exception?!?" + task.Exception;
                }
            };
        }
        //If the task was canceled
        else if (task.IsCanceled)
        {
            theUIManager.warningLoginText.text = "Canceled";
        }
        //If the task was completed successfully
        else
        {
            theUIManager.confirmLoginText.text = "Welcome: " + task.Result.DisplayName + "!";
            StartCoroutine(LoginViaFirebaseGoogle(task.Result.IdToken));
        }
    }

    //Function to sign in using Google's idToken
    private IEnumerator LoginViaFirebaseGoogle(string idToken)
    {
        //Call the Firebase auth signin function passing the idToken
        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        var LoginTask = theFirebaseManager.auth.SignInWithCredentialAsync(credential);

        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            theUIManager.warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            theFirebaseManager.user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", theFirebaseManager.user.DisplayName, theFirebaseManager.user.Email);
            theUIManager.warningLoginText.text = "";
            theUIManager.confirmLoginText.text = "Logged In";

            StartCoroutine(theFirebaseManager.LoadUserData());

            yield return new WaitForSeconds(2);

            theUIManager.WelcomeScreen();
        }
    }

    /*
    public void OnSignInSilently()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        
        Debug.Log("Calling SignIn Silently");

        GoogleSignIn.DefaultInstance.SignInSilently().ContinueWith(OnAuthenticationFinished);
    }

    public void OnGamesSignIn()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = true;
        GoogleSignIn.Configuration.RequestIdToken = false;

        Debug.Log("Calling Games SignIn");

        GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnAuthenticationFinished);
    }
    */

}
