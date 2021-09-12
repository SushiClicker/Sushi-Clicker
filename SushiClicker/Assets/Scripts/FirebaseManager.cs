using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Newtonsoft.Json;
using System;
using System.Linq;

public class FirebaseManager : MonoBehaviour
{
    private UIManager theUIManager;
    
    //Firebase variables
    [Header("Firebase")]
    [SerializeField] private DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;
    public FirebaseApp app;
    public DatabaseReference dbReference;
    private bool hasCheckedLogin = false;

    //Unity variables
    [SerializeField] private GameObject leaderboardElementPrefab;
    [SerializeField] private Transform leaderboardContent;
    [SerializeField] private GameObject linkButton;
    [SerializeField] private InputField searchBox;

    //Start function is called just before any of the Update methods is called the first time
    private void Start()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                app = FirebaseApp.DefaultInstance;
                FirebaseDatabase.GetInstance(app).SetPersistenceEnabled(false);
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });

        //setting theUIManager variable
        theUIManager = FindObjectOfType<UIManager>();
    }

    //This function is called every fixed framerate frame and the first time it's called it checks if autologin is needed
    private void FixedUpdate()
    {
        if (auth != null && hasCheckedLogin == false)
        {
            CheckAutoLogin();
            hasCheckedLogin = true;
        }
    }

    //Functions that are being called automatically if the user pause/quit/minimaze the app and call SaveData function
    private void OnApplicationPause()
    {
        SaveData();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveData();
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
    
    //Function to initialize Firebase auth and database reference
    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth and DB");
        //Set the authentication instance object
        auth = FirebaseAuth.DefaultInstance;
        //Set the Database reference to the root reference
        dbReference = FirebaseDatabase.DefaultInstance.RootReference;
        //Makes sure that the database reference is synced
        dbReference.KeepSynced(true);
    }

    //Function to check if there was a user logged in before if so autologs
    private void CheckAutoLogin()
    {
        if (auth.CurrentUser != null) // We have been automatically logged in
        {
            //Set the user to the last user that was logged in, load the user's data and load the welcome screen
            user = auth.CurrentUser;
            StartCoroutine(LoadUserData());
            theUIManager.WelcomeScreen();
        }
        //If there was no user logged in load the login screen
        else
        {
            theUIManager.LoginScreen();
        }
    }

    //Function to call all update functions to upload the user's data to the RealTime Database
    private void SaveData()
    {
        //Checks if there a logged in user
        if (dbReference != null && user != null)
        {
            Debug.Log("Save Function Called");
            StartCoroutine(UpdateShopItems(GameManager.userShop));
            StartCoroutine(UpdateStats(GameManager.userStats));
            StartCoroutine(UpdateName());
            StartCoroutine(UpdateProfilePic());
            StartCoroutine(UpdateLeaderboard(GameManager.userStats));

            //Updates the sound effect and background music variables
            PlayerPrefs.SetInt("sfxBool", Convert.ToInt32(SoundManager.sfxBool));
            PlayerPrefs.SetInt("bgmBool", Convert.ToInt32(SoundManager.bgmBool));
        }
    }

    //Function to sign in with email and password
    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            theUIManager.warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.Email);
            theUIManager.warningLoginText.text = "";
            theUIManager.confirmLoginText.text = "Logged In";

            StartCoroutine(LoadUserData());

            yield return new WaitForSeconds(2);

            theUIManager.WelcomeScreen();
        }
    }

    //Function to create a new user with username, email and passowrd
    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            theUIManager.warningRegisterText.text = "Missing Username";
        }
        else if (theUIManager.passwordRegisterField.text != theUIManager.passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            theUIManager.warningRegisterText.text = "Passwords Don't Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                    case AuthError.InvalidEmail:
                        message = "Invalid Email Format";
                        break;
                }
                theUIManager.warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                user = RegisterTask.Result;
                //Updates the user profile picture to default
                UserProfile userProfile = new UserProfile { PhotoUrl = new Uri("https://i.ibb.co/SnrPXwx/copy-833524087.png") };
                user.UpdateUserProfileAsync(userProfile);

                if (user != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = user.UpdateUserProfileAsync(profile);

                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        theUIManager.warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        theUIManager.warningRegisterText.text = "";
                        theUIManager.confirmRegisterText.text = "Account created successfully";

                        yield return new WaitForSeconds(2);

                        //Now return to login screen
                        theUIManager.LoginScreen();
                    }
                }
            }
        }
    }

    //Function to sign in anonymously, doesn't require username and password
    private IEnumerator GuestLogin()
    {
        //Call the Firebase auth signin anonymously function
        var LoginTask = auth.SignInAnonymouslyAsync();
        
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            if (LoginTask.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + LoginTask.Exception);
            }

            if (LoginTask.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + LoginTask.Exception);
            }
            theUIManager.warningLoginText.text = "Failed to login as guest";
        }
        else
        {
            //User is now logged in
            //Now get the result
            user = LoginTask.Result;

            //Updates the user profile picture to default and the guest's name
            string guestId = user.UserId;
            string guestNum = guestId.Substring(guestId.Length - 4);
            UserProfile userProfile = new UserProfile { DisplayName = "Guest-" + guestNum, PhotoUrl = new Uri("https://i.ibb.co/SnrPXwx/copy-833524087.png") };
            user.UpdateUserProfileAsync(userProfile);

            Debug.LogFormat("User signed in successfully: {0} ({1})", user.DisplayName, user.UserId);
            theUIManager.warningLoginText.text = "";
            theUIManager.confirmLoginText.text = "Logged In as Guest";

            yield return new WaitForSeconds(2);

            theUIManager.WelcomeScreen();
        }
    }

    //Function to send a reset password email
    private IEnumerator ResetPassword(string _email)
    {
        //Call the Firebase auth signin function passing the email and password
        var ResetTask = auth.SendPasswordResetEmailAsync(_email);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ResetTask.IsCompleted);

        if (ResetTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to reset password task with {ResetTask.Exception}");
            FirebaseException firebaseEx = ResetTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Reset Password Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;

            }
            theUIManager.passwordEmailWarningText.text = message;
        }
        else
        {
            //Reset password link has been sent
            theUIManager.passwordEmailWarningText.text = "";
            theUIManager.passwordEmailConfirmText.text = "A reset password link has been sent to your email.";
        }
    }

    //Function to receive the logged in user's data from the RealTime Database and load them into GameManager's variables
    public IEnumerator LoadUserData()
    {
        if (dbReference == null)
        {
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            dbReference.KeepSynced(true);
        }

        //Get the currently logged in user data from the RealTime Database
        var DBTask = dbReference.Child("users").Child(user.UserId).GetValueAsync();
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            //If there was errors
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //if there is no info for that user in the DB
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Setting GameManager variables using the task's result
            GameManager.userStats.totalSushi = double.Parse(snapshot.Child("stats").Child("totalSushi").Value.ToString());
            GameManager.userStats.sushiInBank = double.Parse(snapshot.Child("stats").Child("sushiInBank").Value.ToString());
            GameManager.userStats.sushiPerSec = double.Parse(snapshot.Child("stats").Child("sushiPerSec").Value.ToString());
            GameManager.userStats.sushiPerClick = int.Parse(snapshot.Child("stats").Child("sushiPerClick").Value.ToString());
            GameManager.userStats.fingerClicks = int.Parse(snapshot.Child("stats").Child("fingerClicks").Value.ToString());
            GameManager.userStats.startDate = Convert.ToDateTime(snapshot.Child("stats").Child("startDate").Value.ToString());

            for (int i = 0; i < GameManager.userShop.shopList.Count; i++)
            {
                GameManager.userShop.shopList[i].price = double.Parse(snapshot.Child("shop").Child(i.ToString()).Child("price").Value.ToString());
                GameManager.userShop.shopList[i].multiplier = double.Parse(snapshot.Child("shop").Child(i.ToString()).Child("multiplier").Value.ToString());
                GameManager.userShop.shopList[i].level = int.Parse(snapshot.Child("shop").Child(i.ToString()).Child("level").Value.ToString());
            }
        }
    }

    //Function to update the user's shop data in the RealTime Database
    private IEnumerator UpdateShopItems(Shop userShop)
    {
        //If there is a logged in user
        if (user != null)
        { 
            //Convert and set the shopList into Json
            string jsonArray = JsonConvert.SerializeObject(userShop.shopList);

            var DBTask = dbReference.Child("users").Child(user.UserId).Child("shop").SetRawJsonValueAsync(jsonArray);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //RealTime Database Shop is now updated
            }
        }
    }

    //Function to update the user's stats data in the RealTime Database
    private IEnumerator UpdateStats(Stats userStats)
    {
        //If there is a logged in user
        if (user != null)
        {
            //Convert and set the userStats into Json
            string jsonArray = JsonConvert.SerializeObject(userStats);

            var DBTask = dbReference.Child("users").Child(user.UserId).Child("stats").SetRawJsonValueAsync(jsonArray);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //RealTime Database Stats is now updated
            }
        }
    }

    //Function to update the user's name data in the RealTime Database
    public IEnumerator UpdateName()
    {
        //If there is a logged in user
        if (user != null)
        {
            var DBTask = dbReference.Child("users").Child(user.UserId).Child("name").SetValueAsync(user.DisplayName);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //RealTime Database name is now updated
            }
        }
    }

    //Function to update the user's profile picture data in the RealTime Database
    public IEnumerator UpdateProfilePic()
    {
        //If there is a logged in user
        if (user != null)
        {
            var DBTask = dbReference.Child("users").Child(user.UserId).Child("profile_pic").SetValueAsync(user.PhotoUrl.ToString());
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //RealTime Database profile picture is now updated
            }
        }
    }

    //Function to update the user's leaderboard data in the RealTime Database
    private IEnumerator UpdateLeaderboard(Stats userStats)
    {
        //If there is a logged in user
        if (user != null)
        {
            var DBTask = dbReference.Child("users").Child(user.UserId).Child("sushi_leaderboard").SetValueAsync(Math.Round(userStats.totalSushi));
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

            if (DBTask.Exception != null)
            {
                Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
            }
            else
            {
                //RealTime Database sushi leaderboard is now updated
            }
        }
    }

    //Function to receive and sort all the requested users data from the RealTime Database and create a new GameObject to each requested user
    private IEnumerator LoadLeaderboardData(string searchName)
    {
        if (dbReference == null)
        {
            Debug.Log("DB WAS NULL");
            dbReference = FirebaseDatabase.DefaultInstance.RootReference;
            dbReference.KeepSynced(true);
        }

        //Call save data function before loading the leaderboard from the RTDB
        SaveData();

        //Get all the users data ordered by sushi leaderboard amount
        var DBTask = dbReference.Child("users").OrderByChild("sushi_leaderboard").GetValueAsync();
        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing leaderboard elements
            foreach (Transform child in leaderboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            int i = 1;
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                int place = i++;
                string name = childSnapshot.Child("name").Value.ToString();
                double totalSushi = double.Parse(childSnapshot.Child("sushi_leaderboard").Value.ToString());
                string perSec = childSnapshot.Child("stats").Child("sushiPerSec").Value.ToString() + "/S";
                string profilePic = childSnapshot.Child("profile_pic").Value.ToString();
                //If the username matches the requested info
                if (searchName == "" || name.ToLower().Contains(searchName.ToLower()))
                {
                    //Instantiate new leaderboard elements
                    GameObject leaderboardElement = Instantiate(leaderboardElementPrefab, leaderboardContent);
                    leaderboardElement.GetComponent<LeaderboardElement>().NewLeaderboardElement(place, name, totalSushi, perSec, profilePic);
                }
            }
            //Use the SetActiveFacebookLink function to hide/show link Facebook button
            SetActiveFacebookLink();
        }
    }

    //Function which checks if the current user linked his Facebook account and disable the link option in the leaderboard menu
    public void SetActiveFacebookLink()
    {
        //Foreach loop to go over all of the user's providerdata
        foreach (IUserInfo z in auth.CurrentUser.ProviderData)
        {
            //If the user has linked this account with Facebook disable the button
            if (z.ProviderId == "facebook.com")
            {
                linkButton.SetActive(false);
                break;
            }
            else
            {
                linkButton.SetActive(true);
            }
        }
    }

    //Function for the reset password button for unity onClick
    public void ResetPassButton()
    {
        //Call the ResetPassword coroutine passing the email
        StartCoroutine(ResetPassword(theUIManager.passwordEmailField.text));
    }

    //Function for the login button for unity onClick
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(theUIManager.emailLoginField.text, theUIManager.passwordLoginField.text));
    }

    //Function for the guest register button for unity onClick
    public void GuestButton()
    {
        //Call the GuestLogin coroutine
        StartCoroutine(GuestLogin());
    }

    //Function for the register button for unity onClick
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(theUIManager.emailRegisterField.text, theUIManager.passwordRegisterField.text, theUIManager.usernameRegisterField.text));
    }

    //Function for the sign out leaderboard button for unity onClick
    public void LeaderboardButton()
    {
        //Call the LoadLeaderboardData coroutine passing empty string
        StartCoroutine(LoadLeaderboardData(""));
    }

    //Function for the sign out button for unity onClick
    public void SignOutButton()
    {
        //Uploads the user's data to the RTDB
        SaveData();
        auth.SignOut();
        user = null;

        //Resetting Stats and Shop variables
        GameManager.userShop = new Shop();
        GameManager.userShop.InitShop();
        GameManager.userStats.InitStats();
        theUIManager.LoginScreen();
    }
 
    //Function for the leaderboard search box button for unity onClick
    public void SearchBoxButton()
    {
        //Call the LoadLeaderboardData coroutine passing name to search
        StartCoroutine(LoadLeaderboardData(searchBox.text));
    }
}

