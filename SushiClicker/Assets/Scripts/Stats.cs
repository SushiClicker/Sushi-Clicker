using System;

public class Stats
{
    public double totalSushi;
    public double sushiInBank;
    public double sushiPerSec;
    public int sushiPerClick;
    public int fingerClicks;
    public DateTime startDate;

    //Constructor
    public Stats()
    {
        totalSushi = 0;
        sushiInBank = 0;
        sushiPerSec = 0;
        sushiPerClick = 1;
        fingerClicks = 0;
        startDate = DateTime.Today;
    }

    //Function to set the variables back to the default values
    public void InitStats()
    {
        totalSushi = 0;
        sushiInBank = 0;
        sushiPerSec = 0;
        sushiPerClick = 1;
        fingerClicks = 0;
        startDate = DateTime.Today;
    }

}
