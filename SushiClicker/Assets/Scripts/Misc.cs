using UnityEngine;
using UnityEngine.UI;

public class Misc : MonoBehaviour
{
    //Sound variables
    public GameObject bgmButton;
    public GameObject sfxButton;
    private SoundManager theSoundManager;

    //Start function is called just before any of the Update methods is called the first time
    private void Start()
    {
        theSoundManager = FindObjectOfType<SoundManager>();
        //If the sound effects are on load the "ON" sfx sprite button else "OFF" sprite
        if (SoundManager.sfxBool)
        {
            sfxButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SFXon");
        }
        else
        {
            sfxButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SFXoff");
        }
        //If the background music is on load the "ON" background music sprite button else "OFF" sprite
        if (SoundManager.bgmBool)
        {
            bgmButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("BGMON");
        }
        else
        {
            bgmButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("BGMOFF");
        }
    }

    //Function to reset user's game data
    public void WipeSave()
    {
        //Resetting both Shop and Stats
        GameManager.userShop.ResetShop();
        GameManager.userStats.InitStats();
    }

    //Function to share the app
    public void ClickShareButton()
    {
        string shareMessage = "I can't believe I already made " + GameManager.ShortenNumberToInt(GameManager.userStats.totalSushi) + " sushi in Sushi Clicker!\n" +
            "Click here to join me: https://play.google.com/store/apps/details?id=com.DefaultCompany.sushiclicker";
        new NativeShare().SetSubject("Sushi Clicker").SetText(shareMessage).Share();
    }

    //Function to mute/unmute the background music
    public void MuteBgmButton()
    {
        theSoundManager.BGMSound();
        //If the background music is on load the "ON" background music sprite button else "OFF" sprite
        if (bgmButton.GetComponent<Image>().sprite == Resources.Load<Sprite>("BGMOFF"))
        {
            bgmButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("BGMON");
        }
        else
        {
            bgmButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("BGMOFF");
        }
    }

    //Function to mute/unmute the sound effects
    public void MuteSfxButton()
    {
        theSoundManager.SFXSound();
        //If the sound effects are on load the "ON" sfx sprite button else "OFF" sprite
        if (sfxButton.GetComponent<Image>().sprite == Resources.Load<Sprite>("SFXoff"))
        {
            sfxButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SFXon");
        }
        else
        {
            sfxButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("SFXoff");
        }
    }

}
