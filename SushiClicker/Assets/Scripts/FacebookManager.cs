using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Facebook.Unity;

public class FacebookManager : MonoBehaviour
{
    private UIManager theUIManager;
    private FirebaseManager theFirebaseManager;

    //Awake function is called when the script is being loaded, initializing Facebook
    private void Awake()
    {
        theUIManager = FindObjectOfType<UIManager>();
        theFirebaseManager = FindObjectOfType<FirebaseManager>();

        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            //Already initialized
            FB.ActivateApp();
        }
    }

    //Callback function to initialize Facebook
    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
        }
        else
        {
            Debug.Log("Something went wrong to Initialize the Facebook SDK");
        }
    }

    //Function to pause the game while connecting to Facebook
    private void OnHideUnity(bool isGameScreenShown)
    {
        if (!isGameScreenShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    //Function to Login Button asking read permissions of email and public proflie from user
    public void LoginButtonForFacebook()
    {
        var permissons = new List<string> { "email", "public_profile" };
        FB.LogInWithReadPermissions(permissons, AuthStatusCallbackLogin);
    }

    //Function to Link Button asking read permissions of email and public proflie from user
    public void LinkButtonForFacebook()
    {
        var permissons = new List<string> { "email", "public_profile" };
        FB.LogInWithReadPermissions(permissons, AuthStatusCallbackLink);
    }

    //Callback function to start coroutine of LoginViaFirebaseFacebook
    private void AuthStatusCallbackLogin(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            AccessToken accessToken = AccessToken.CurrentAccessToken;
            // current access token's User ID : aToken.UserId
            StartCoroutine(LoginViaFirebaseFacebook(accessToken));
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    //Callback function to start coroutine of LinkFacebook
    private void AuthStatusCallbackLink(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            AccessToken accessToken = AccessToken.CurrentAccessToken;
            // current access token's User ID : aToken.UserId
            StartCoroutine(LinkFacebook(accessToken));
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    //Function to sign in using Facebook's accessToken
    private IEnumerator LoginViaFirebaseFacebook(AccessToken accessToken)
    {
        //Call the Firebase auth signin function passing the accessToken
        Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);
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

    //Function to link an existing account using Facebook's accessToken
    private IEnumerator LinkFacebook(AccessToken accessToken)
    {
        //Call the Firebase auth link function passing the accessToken
        Credential credential = FacebookAuthProvider.GetCredential(accessToken.TokenString);
        var LoginTask = theFirebaseManager.auth.CurrentUser.LinkWithCredentialAsync(credential);

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
            //User is now linked
            theFirebaseManager.user = LoginTask.Result;
            FB.API("/me?fields=name", HttpMethod.GET, GetFacebookData);

            //Call the Firebase auth signin function passing credential
            var LoginTask2 = theFirebaseManager.auth.SignInWithCredentialAsync(credential);

            //Wait until the task completes
            yield return new WaitUntil(predicate: () => LoginTask2.IsCompleted);

            //Update the link button sprite and close the leaderboard menu
            theFirebaseManager.SetActiveFacebookLink();
            theUIManager.CloseLeaderboardScreen();

            Debug.LogFormat("User linked to facebook in successfully: {0} ({1})", theFirebaseManager.user.DisplayName, theFirebaseManager.user.Email);
        }
    }

    //Function to get the name and profile picture from Facebook and update the user's name and picture
    private void GetFacebookData(IGraphResult result)
    {
        //Get the name
        string name = result.ResultDictionary["name"].ToString();

        //Get the picture
        string url = "https://graph.facebook.com/" + result.ResultDictionary["id"].ToString() + "/picture";

        //Update both
        UserProfile userProfile = new UserProfile { DisplayName = name, PhotoUrl = new System.Uri(url) };
        theFirebaseManager.auth.CurrentUser.UpdateUserProfileAsync(userProfile);
        StartCoroutine(theFirebaseManager.UpdateProfilePic());
        StartCoroutine(theFirebaseManager.UpdateName());
    }






}
