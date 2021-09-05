using UnityEngine;

public class SoundManager : MonoBehaviour
{
    //Sound GameObjects
    [SerializeField] private GameObject bgm;
    [SerializeField] private GameObject shop;
    [SerializeField] private GameObject clicker;
    [SerializeField] private GameObject menus;
    [SerializeField] private GameObject buy;
    [SerializeField] private GameObject bonusTimer;
    [SerializeField] private GameObject bonusPop;

    //Setting two public variables for Mute/Unmute purposes
    public static bool sfxBool;
    public static bool bgmBool;

    //Start function is called just before any of the Update methods is called the first time
    private void Start()
    {
        //If the player has ever saved his preferences of sound, update sound variables
        if (PlayerPrefs.HasKey("sfxBool") && PlayerPrefs.HasKey("bgmBool"))
        {
            if (PlayerPrefs.GetInt("sfxBool") == 1)
                sfxBool = true;
            else
                sfxBool = false;

            if (PlayerPrefs.GetInt("bgmBool") == 1)
                bgmBool = true;
            else
                bgmBool = false;
        }
        //else set them to default true
        else
        {
            sfxBool = true;
            bgmBool = true;
        }

        bgm.SetActive(bgmBool);
    }

    //Function to turn on/off the background music
    public void BGMSound()
    {
        bgmBool = !bgmBool;
        bgm.SetActive(bgmBool);
    }

    //Function to turn on/off the sound effects
    public void SFXSound()
    {
        sfxBool = !sfxBool;
    }

    //Function to play Clicker Sound
    public void ClickerSound()
    {
        if (sfxBool)
            clicker.GetComponent<AudioSource>().Play();
    }

    //Function to play Shop Sound
    public void ShopSound()
    {
        if (sfxBool)
            shop.GetComponent<AudioSource>().Play();
    }

    //Function to play Buy Sound
    public void BuySound()
    {
        if (sfxBool)
            buy.GetComponent<AudioSource>().Play();
    }

    //Function to play Menu Sound
    public void MenuSound()
    {
        if (sfxBool)
            menus.GetComponent<AudioSource>().Play();
    }

    //Function to play Bonus Timer Sound
    public void BonusTimerSound()
    {
        if (sfxBool)
            bonusTimer.GetComponent<AudioSource>().Play();
    }

    //Function to play Bonus Pop Sound
    public void BonusPopSound()
    {
        if (sfxBool)
            bonusPop.GetComponent<AudioSource>().Play();
    }
}
