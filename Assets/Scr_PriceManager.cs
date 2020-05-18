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
    private int buyingPrice;
    private int[] prices = new int[12];

    private void Start()
    {
        Scr_ChartManager.Initiate();

        buyingPrice = PlayerPrefs.GetInt("input0");
        input[0].text = buyingPrice.ToString();

        for (int i = 0; i < 12; i++)
        {
            if (PlayerPrefs.HasKey("input" + (i + 1).ToString()))
                prices[i] = PlayerPrefs.GetInt("input" + (i + 1).ToString());
            input[i + 1].text = prices[i].ToString();
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
        if (num == 0)
        {
            buyingPrice = price > 0 ? price : 0;
        }
        else
        {
            prices[num - 1] = price > 0 ? price : 0;
        }
        PlayerPrefs.SetInt("input" + num.ToString(), price);
        Debug.Log(num + " : " + price);
        PredictPrices();
    }

    private void UpdateChart()
    {
        Scr_ChartManager.chartPoints[0].transform.position = new Vector3(0, buyingPrice * 0.01f);
        for (int i = 0; i < 12; i++)
        {
            Scr_ChartManager.chartPoints[(i * 2) + 1].transform.position = new Vector3((i + 1) * 0.45f, prices[i] * 0.01f);
            Scr_ChartManager.chartPoints[(i * 2) + 2].transform.position = new Vector3((i + 1) * 0.45f, prices[i] * 0.01f);
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
        if (buyingPrice == 0)
            return;
        int[] prediction = Scr_CalculatePrice.Calculate(myPattern, buyingPrice, prices);
        for (int i = 0; i < 12; i++)
            prices[i] = prediction[i + 2];
        UpdateChart();
    }

    public void ResetPrices()
    {
        PlayerPrefs.DeleteAll();

        pattern_DropDown.value = 0;
        myPattern = Patterns.Unknown;

        for (int i = 0; i <= 12; i++)
        {
            input[i].text = "0";
            if (i < 12)
                prices[i] = 0;
        }
    }
}