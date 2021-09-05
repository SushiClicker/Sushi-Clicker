using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class LeaderboardElement : MonoBehaviour
{
    [SerializeField] private Text placeText;
    [SerializeField] private Text usernameText;
    [SerializeField] private Text totalSushiText;
    [SerializeField] private Text perSecText;
    [SerializeField] private Image profilePic;

    //Function to update the texts variables
    public void NewLeaderboardElement(int _place, string _name, double _totalSushi, string _perSec, string _profilePic)
    {
        //If the string is Hebrew or Arabic reverse the name string
        if (IsStringHebrewOrArabic(_name))
        {
            usernameText.text = RightToLeft(_name);
        }
        else
        {
            usernameText.text = _name;
        }
        placeText.text = _place.ToString();
        totalSushiText.text = GameManager.ShortenNumberToInt(_totalSushi);
        perSecText.text = _perSec;
        StartCoroutine(UpdateProfilePic(_profilePic));
    }

    //Function to update the profile picture of leaderboard element using URL
    private IEnumerator UpdateProfilePic(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        Texture2D tex = ((DownloadHandlerTexture)request.downloadHandler).texture;
        Sprite sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(tex.width / 2, tex.height / 2));
        profilePic.GetComponent<Image>().overrideSprite = sprite;
    }

    //Boolean function which returns true if the given char is written in Herbrew or Arabic
    private bool IsCharHebrewOrArabic(char c)
    {
        char FirstHebChar = (char)1488;
        char LastHebChar = (char)1514;
        char FirstArabicChar = (char)1575;
        char LastArabicChar = (char)1603;
        return (c >= FirstArabicChar && c <= LastArabicChar) || (c >= FirstHebChar && c <= LastHebChar);
    }

    //Boolean function which returns true if the given string is written in Herbrew or Arabic
    private bool IsStringHebrewOrArabic(string s)
    {
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (IsCharHebrewOrArabic(c))
            {
                return true;
            }
        }
        return false;
    }

    //Function to return reversed string to use in case of Hebrew or Arabic text
    private string RightToLeft(string s)
    {
        string str = "";
        for (int i = s.Length - 1; i >= 0; i--)
        {
            str += s[i];
        }
        return str;
    }


}
