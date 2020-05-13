using UnityEngine;
using UnityEngine.UI;

public class Scr_PriceManager : MonoBehaviour
{
    public Dropdown pattern_DropDown;
    public InputField[] input;

    private enum Patterns
    {
        Unknown,
        SmlSpike,
        HighSpike,
        Fluc,
        Dec,
    };

    private Patterns myPattern;
    private int[] prices = new int[13];

    private void Start()
    {
        for (int i = 0; i <= 12; i++)
        {
            if (PlayerPrefs.HasKey("input" + i.ToString()))
                prices[i] = PlayerPrefs.GetInt("input" + i.ToString());
            input[i].text = prices[i].ToString();
        }
        if (PlayerPrefs.HasKey("pattern"))
            myPattern = (Patterns)PlayerPrefs.GetInt("pattern");
        pattern_DropDown.value = (int)myPattern;
        PredictPrices();
    }

    public void UpdatePatern()
    {
        int pattern = pattern_DropDown.value;
        myPattern = (Patterns)pattern;
        PlayerPrefs.SetInt("pattern", pattern);
        PredictPrices();
    }

    public void UpdatePrice(int num)
    {
        int price = int.Parse(input[num].text);
        prices[num] = price;
        PlayerPrefs.SetInt("input" + num.ToString(), price);
        Debug.Log(num + " : " + prices[num]);
        PredictPrices();
    }

    private void PredictPrices()
    {

    }

    public void ResetPrices()
    {
        PlayerPrefs.DeleteAll();

        pattern_DropDown.value = 0;
        myPattern = Patterns.Unknown;

        for (int i = 0; i <= 12; i++)
        {
            input[i].text = "0";
            prices[i] = 0;
        }
    }
}