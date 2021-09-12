using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private SoundManager theSoundManager;

    //Game screen text variables
    [Header("Game screen text variables")]
    [SerializeField] private Text[] shopText = new Text[24];
    [SerializeField] private Text[] statsText = new Text[6];
    [SerializeField] private Text bankText;
    [SerializeField] private Text perSecText;

    //Bonus variables
    [Header("Bonus variables")]
    [SerializeField] private GameObject gameCanvas;
    [SerializeField] private GameObject bonusButton;
    [SerializeField] private GameObject bonusCounter;
    [SerializeField] private Text bonusCounterText;
    private bool bonusActive = false;
    private bool bonusButtonActive;
    private double bonusLength;
    private double bonusButtonTimer;
    private double perSecMul;
    public int perClickAdd;

    //General Variables
    public static Shop userShop = new Shop();
    public static Stats userStats = new Stats();
    private int gameSeconds = 1;

    //Start function is called just before any of the Update methods is called the first time
    private void Start()
    {
        theSoundManager = FindObjectOfType<SoundManager>();
        Application.targetFrameRate = 60; //sets the frames per second to 60 frames
        userShop.InitShop(); //Resetting shop
    }

    //Update function is called every frame
    private void Update()
    {
        //If game screen is loaded
        if (gameCanvas.activeInHierarchy)
        {
            //Setting the bonus button variables every minute and a half
            if (gameSeconds % 90 == 0)
            {
                theSoundManager.BonusPopSound();
                bonusButtonActive = true;
                bonusButtonTimer = 5;
            }
            //If the bonus button boolean variable is true
            if (bonusButtonActive)
            {
                //Activate the bonus button for 5 seconds
                bonusButton.SetActive(true);
                bonusButtonTimer -= Time.deltaTime;
                //if 5 seconds passed
                if (bonusButtonTimer <= 0)
                {
                    bonusButtonActive = false;
                }
            }
            else
            {
                bonusButton.SetActive(false);
            }
            //If bonus effect is active update perSec/perClick variables
            if (bonusActive)
            {
                bonusCounter.SetActive(true);
                if (bonusButton.activeInHierarchy)
                {
                    bonusButton.SetActive(false);
                }
                bonusCounterText.text = Convert.ToInt64(bonusLength) + "s";

                perSecMul = userStats.sushiPerSec * 2;
                perClickAdd = userStats.sushiPerClick * 2;

                bonusLength -= Time.deltaTime;
                //If the bonus timer has run out 
                if (bonusLength <= 0)
                {
                    bonusActive = false;
                }
            }
            //If bonus effect is off update perSec/perClick variables back to the original values
            else
            {
                perSecMul = userStats.sushiPerSec;
                perClickAdd = userStats.sushiPerClick;
                bonusCounterText.text = "";
                bonusCounter.SetActive(false);
            }
        }

        //Updates and adds to totalSushi and sushiInBank every second the value of per second multiplier variable
        if (Time.time >= gameSeconds)
        {
            gameSeconds = Mathf.FloorToInt(Time.time) + 1;
            userStats.totalSushi += perSecMul;
            userStats.sushiInBank += perSecMul;
        }

        //Updates texts every frame
        UpdateShopText();
        UpdateStatsText();
        UpdateGameText();
    }

    //Function to increment the totalSushi and sushiInBank variables by perClickAdd and promoting fingerClicks
    public void Increment()
    {
        userStats.totalSushi += perClickAdd;
        userStats.sushiInBank += perClickAdd;
        userStats.fingerClicks += 1;
    }

    //Function to buy an item from the shop
    public void Buy(int itemNumber)
    {
        //If the item is active income updates sushiPerClick
        if (itemNumber == 0)
        {
            //If the user has enough sushi in the bank to buy the item
            if (userStats.sushiInBank >= userShop.shopList[itemNumber].price)
            {
                userStats.sushiInBank -= userShop.shopList[itemNumber].price;
                userShop.shopList[itemNumber].price *= 1.5;
                userStats.sushiPerClick += 1;
                userShop.shopList[itemNumber].level += 1;
                theSoundManager.BuySound();
            }
        }
        //If the item is passive income updates sushiPerSec
        else
        {
            //If the user has enough sushi in the bank to buy the item
            if (userStats.sushiInBank >= userShop.shopList[itemNumber].price)
            {
                userStats.sushiInBank -= userShop.shopList[itemNumber].price;
                userShop.shopList[itemNumber].price *= 1.3;
                userStats.sushiPerSec += userShop.shopList[itemNumber].multiplier;
                userShop.shopList[itemNumber].level += 1;
                theSoundManager.BuySound();
            }
        }
    }

    //Function to activate the bonus effect for 10 seconds
    public void ActiveBonus()
    {
        bonusActive = true;
        bonusLength = 10;
    }

    //Function to convert double num to string and shorten it with two decimal digits
    public static string ShortenNumber(double num)
    {
        int million = 1000000;
        int thousand = 1000;
        if (num < thousand)
            return num.ToString();
        else if (num >= thousand && num < million)
            return Math.Round(num / thousand, 2) + "K";
        return Math.Round(num / million, 2) + "M";
    }

    //Function to convert double num to string and shorten it with no decimal digits
    public static string ShortenNumberToInt(double num)
    {
        int million = 1000000;
        int thousand = 1000;
        num = Convert.ToInt64(num);
        if (num < thousand)
            return num.ToString();
        else if (num >= thousand && num < million)
            return Math.Round(num / thousand, 2) + "K";
        return Math.Round(num / million, 2) + "M";
    }

    //Functions to update main screen texts
    private void UpdateShopText()
    {
        for (int i = 0; i < shopText.Length; i += 3)
        {
            int index = (i == 0) ? 0 : i / 3;
            int Price = i;
            int Mul = i + 1;
            int Level = i + 2;
            //Price
            shopText[Price].text = ShortenNumberToInt(userShop.shopList[index].price) + " SUSHI"; ;

            //Mul
            if (index != 0)
            {
                if (userShop.shopList[index].multiplier < 10 && userShop.shopList[index].multiplier == Math.Floor(userShop.shopList[index].multiplier))
                    shopText[Mul].text = "S/sec: +" + userShop.shopList[index].multiplier + ".0";
                else
                    shopText[Mul].text = "S/sec: +" + userShop.shopList[index].multiplier;
            }
            else
            {
                shopText[Mul].text = "Click: +" + userShop.shopList[index].multiplier + ".0";
            }

            //Level
            shopText[Level].text = "Level: " + userShop.shopList[index].level;
        }
    }

    private void UpdateStatsText()
    {
        statsText[0].text = ShortenNumberToInt(userStats.totalSushi) + " SUSHI";
        statsText[1].text = ShortenNumberToInt(userStats.sushiInBank) + " SUSHI";
        statsText[2].text = ShortenNumber(userStats.sushiPerSec) + "/S";
        statsText[3].text = ShortenNumber(userStats.sushiPerClick) + "/CLICK";
        statsText[4].text = ShortenNumber(userStats.fingerClicks);
        statsText[5].text = userStats.startDate.ToString("dddd, dd MMMM yyyy", new System.Globalization.CultureInfo("en-US"));
    }

    private void UpdateGameText()
    {
        bankText.text = ShortenNumberToInt((userStats.sushiInBank)) + " Sushi";
        perSecText.text = ShortenNumber(perSecMul) + "/sec";
    }

}
