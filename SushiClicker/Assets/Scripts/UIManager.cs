using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    //Screen object variables
    [Header("Screens")]
    [SerializeField] private GameObject loginUI;
    [SerializeField] private GameObject registerUI;
    [SerializeField] private GameObject resetPassUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject menusUI;
    [SerializeField] private GameObject bgUI;
    [SerializeField] private GameObject welcomeUI;
    [SerializeField] private GameObject leaderboardUI;

    //Login variables
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;
    public Text warningLoginText;
    public Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public InputField usernameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField passwordRegisterVerifyField;
    public Text warningRegisterText;
    public Text confirmRegisterText;

    //Reset Password variables
    [Header("Reset Password")]
    public InputField passwordEmailField;
    public Text passwordEmailWarningText;
    public Text passwordEmailConfirmText;

    //Function to turn off all screens
    public void ClearScreen()
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        gameUI.SetActive(false);
        menusUI.SetActive(false);
        bgUI.SetActive(false);
        welcomeUI.SetActive(false);
        resetPassUI.SetActive(false);
    }

    //Function to turn on welcome screen
    public void WelcomeScreen()
    {
        ClearScreen();
        welcomeUI.SetActive(true);
        //After 2 seconds delay turn on gamescreen function
        Invoke(nameof(GameScreen), 2.0f);
    }

    //Function to turn on login screen
    public void LoginScreen()
    {
        ClearScreen();
        ClearLoginFields();
        loginUI.SetActive(true);
    }

    //Function to turn on register screen
    public void RegisterScreen()
    {
        ClearScreen();
        ClearRegisterFields();
        registerUI.SetActive(true);
    }

    //Function to turn on game screen
    public void GameScreen()
    {
        ClearScreen();
        gameUI.SetActive(true);
        menusUI.SetActive(true);
        bgUI.SetActive(true);
    }

    //Function to turn on leaderboard screen
    public void LeaderboardScreen()
    {
        leaderboardUI.SetActive(true);
    }

    //Function to turn off leaderboard screen
    public void CloseLeaderboardScreen()
    {
        leaderboardUI.SetActive(false);
    }

    //Function to turn on reset password screen
    public void ResetPasswordScreen()
    {
        ClearScreen();
        ClearResetPasswordFields();
        resetPassUI.SetActive(true);
    }
    
    //Function to show/hide password
    public void RevealPassword(InputField passwordInput)
    {
        //setting boolean variable if the password is currently hidden
        bool isCurrentlyPassword = passwordInput.inputType == InputField.InputType.Password;

        passwordInput.inputType = isCurrentlyPassword ? InputField.InputType.Standard : InputField.InputType.Password;

        passwordInput.ForceLabelUpdate();
    }

    //Function to change the icon of show/hide password button
    public void RevealPasswordIconChanger(GameObject button)
    {
        //If the current icon is RevealOFF change it to RevealON
        if (button.GetComponent<Image>().sprite == Resources.Load<Sprite>("RevealOFF"))
            button.GetComponent<Image>().sprite = Resources.Load<Sprite>("RevealON");
        //else change it to RevealOFF
        else
            button.GetComponent<Image>().sprite = Resources.Load<Sprite>("RevealOFF");
    }

    //Function to clear the login screen input fields
    public void ClearLoginFields()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
        warningLoginText.text = "";
        confirmLoginText.text = "";
    }

    //Function to clear the reset password screen input fields
    public void ClearResetPasswordFields()
    {
        passwordEmailField.text = "";
        passwordEmailWarningText.text = "";
        passwordEmailConfirmText.text = "";
    }

    //Function to clear the register screen input fields
    public void ClearRegisterFields()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
        warningRegisterText.text = "";
        confirmRegisterText.text = "";
    }
}
