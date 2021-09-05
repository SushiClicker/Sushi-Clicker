using System.Collections.Generic;

public class Shop
{
    //Setting an array of strings with the default values of the basic shop
    private string[] shopItems = { "10,1,1", "15,0.1,0", "100,0.3,0", "1000,1,0", "10000,3.1,0", "50000,6,0", "200000,20,0", "500000,40,0" };

    public List<Item> shopList;

    //Constructor
    public Shop()
    {
        shopList = new List<Item>();
    }

    //Function to initialize shop with the default values
    public void InitShop()
    {
        foreach (string cell in shopItems)
        {
            string[] words = cell.Split(',');
            double price = double.Parse(words[0]);
            double mul = double.Parse(words[1]);
            int level = int.Parse(words[2]);

            //Set new object of Item and add it to the shopList
            Item i = new Item(price, mul, level);
            shopList.Add(i);
        }
    }

    //Function to reset shop to the default values
    public void ResetShop()
    {
        shopList = new List<Item>();
        InitShop();
    }
}
