using UnityEngine;

public class Scr_CalculatePrice : MonoBehaviour
{
    public static void Calculate(Scr_PriceManager.Patterns pattern, int basePrice, int[] sellPrice)
    {
        PatternPercentage(pattern, basePrice, sellPrice);
    }

    private static void PatternPercentage(Scr_PriceManager.Patterns myPattern, int basePrice, int[] sellPrices)
    {
        int work;
        int decPhaseLen1, decPhaseLen2, peakStart;
        int hiPhaseLen1, hiPhaseLen2and3, hiPhaseLen3;
        float rate;

        switch (myPattern)
        {
            case Scr_PriceManager.Patterns.Dec:
                {
                    // PATTERN 2: consistently decreasing
                    rate = 0.9f;
                    rate -= Random.Range(0.0f, 0.05f);
                    for (work = 2; work < 14; work++)
                    {
                        sellPrices[work + 1] = Mathf.CeilToInt(rate * basePrice);
                        rate -= 0.03f;
                        rate -= Random.Range(0f, 0.02f);
                    }
                    break;
                }
            case Scr_PriceManager.Patterns.Fluc:
                {
                    break;
                }
            case Scr_PriceManager.Patterns.HighSpike:
                {
                    break;
                }
            case Scr_PriceManager.Patterns.SmlSpike:
                {
                    break;
                }
            default:
                {
                    break;
                }
        }
    }
}