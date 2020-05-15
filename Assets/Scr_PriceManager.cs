using UnityEngine;
using UnityEngine.UI;

public class Scr_PriceManager : MonoBehaviour
{
    public Dropdown pattern_DropDown;
    public InputField[] input;

    public enum Patterns
    {
        Unknown,
        SmlSpike,
        HighSpike,
        Fluc,
        Dec,
    };

    private Patterns myPattern;
    private readonly int[] prices = new int[13];

    private void Start()
    {
        Scr_ChartManager.Initiate();

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
        prices[num] = price > 0 ? price : 0;
        PlayerPrefs.SetInt("input" + num.ToString(), price);
        Debug.Log(num + " : " + prices[num]);
        UpdateChart();
        PredictPrices();
    }

    private void UpdateChart()
    {
        for (int i = 0; i < 13; i++)
        {
            if (prices[i] != 0)
            {
                Scr_ChartManager.chartPoints[i * 2].transform.position = new Vector3(i * 0.45f, prices[i] * 0.01f);
                if (i > 0)
                {
                    Scr_ChartManager.chartPoints[(i * 2) - 1].transform.position = new Vector3(i * 0.45f, prices[i] * 0.01f);
                }
            }
        }

        for (int i = 0; i < 24; i++)
        {
            if (i > 0)
            {
                Scr_ChartManager.lines[i].SetPosition(0, Scr_ChartManager.chartPoints[i + 1].transform.position);
                Scr_ChartManager.lines[i].SetPosition(1, Scr_ChartManager.chartPoints[i - 1].transform.position);
            }
            else
            {
                Scr_ChartManager.lines[i].SetPosition(0, Scr_ChartManager.chartPoints[i + 1].transform.position);
                Scr_ChartManager.lines[i].SetPosition(1, Scr_ChartManager.chartPoints[i].transform.position);
            }
        }
    }

    private void PredictPrices()
    {
        Scr_CalculatePrice.Calculate(myPattern, prices[0], prices);
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