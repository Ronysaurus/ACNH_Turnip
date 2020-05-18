using UnityEngine;

public class Scr_CalculatePrice : MonoBehaviour
{
    private struct TurnipPrices
    {
        public int basePrice;
        public int[] sellPrices; // = new int [14];
        public int whatPattern;

        public void calculate()
        {
            /*
            if (checkGlobalFlag("FirstKabuBuy")) {
              if (!checkGlobalFlag("FirstKabuPattern")) {
                setGlobalFlag("FirstKabuPattern", true);
                whatPattern = 3;
              }
            }
            */

            int work;
            int decPhaseLen1, decPhaseLen2, peakStart;
            int hiPhaseLen1, hiPhaseLen2and3, hiPhaseLen3;
            float rate;

            switch (whatPattern)
            {
                case 0:
                    // PATTERN 0: high, decreasing, high, decreasing, high
                    work = 2;
                    decPhaseLen1 = (Random.value > 0.5f) ? 3 : 2;
                    decPhaseLen2 = 5 - decPhaseLen1;

                    hiPhaseLen1 = Random.Range(0, 6);
                    hiPhaseLen2and3 = 7 - hiPhaseLen1;
                    hiPhaseLen3 = Random.Range(0, hiPhaseLen2and3 - 1);

                    // high phase 1
                    for (int i = 0; i < hiPhaseLen1; i++)
                    {
                        sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * basePrice);
                    }

                    // decreasing phase 1
                    rate = Random.Range(0.8f, 0.6f);
                    for (int i = 0; i < decPhaseLen1; i++)
                    {
                        sellPrices[work++] = Mathf.CeilToInt(rate * basePrice);
                        rate -= 0.04f;
                        rate -= Random.Range(0f, 0.06f);
                    }

                    // high phase 2
                    for (int i = 0; i < (hiPhaseLen2and3 - hiPhaseLen3); i++)
                    {
                        sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * basePrice);
                    }

                    // decreasing phase 2
                    rate = Random.Range(0.8f, 0.6f);
                    for (int i = 0; i < decPhaseLen2; i++)
                    {
                        sellPrices[work++] = Mathf.CeilToInt(rate * basePrice);
                        rate -= 0.04f;
                        rate -= Random.Range(0, 0.06f);
                    }

                    // high phase 3
                    for (int i = 0; i < hiPhaseLen3; i++)
                    {
                        sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * basePrice);
                    }
                    break;

                case 1:
                    // PATTERN 1: decreasing middle, high spike, random low
                    peakStart = Random.Range(3, 9);
                    rate = Random.Range(0.9f, 0.85f);
                    for (work = 2; work < peakStart; work++)
                    {
                        sellPrices[work] = Mathf.CeilToInt(rate * basePrice);
                        rate -= 0.03f;
                        rate -= Random.Range(0f, 0.02f);
                    }
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * basePrice);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(1.4f, 2.0f) * basePrice);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(2.0f, 6.0f) * basePrice);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(1.4f, 2.0f) * basePrice);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * basePrice);
                    for (; work < 14; work++)
                    {
                        sellPrices[work] = Mathf.CeilToInt(Random.Range(0.4f, 0.9f) * basePrice);
                    }
                    break;

                case 2:
                    // PATTERN 2: consistently decreasing
                    rate = 0.9f;
                    rate -= Random.Range(0f, 0.05f);
                    for (work = 2; work < 14; work++)
                    {
                        sellPrices[work] = Mathf.CeilToInt(rate * basePrice);
                        rate -= 0.03f;
                        rate -= Random.Range(0f, 0.02f);
                    }
                    break;

                case 3:
                    // PATTERN 3: decreasing, spike, decreasing
                    peakStart = Random.Range(2, 9);

                    // decreasing phase before the peak
                    rate = Random.Range(0.9f, 0.4f);
                    for (work = 2; work < peakStart; work++)
                    {
                        sellPrices[work] = Mathf.CeilToInt(rate * basePrice);
                        rate -= 0.03f;
                        rate -= Random.Range(0f, 0.02f);
                    }

                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * (float)basePrice);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(0.9f, 1.4f) * basePrice);
                    rate = Random.Range(1.4f, 2.0f);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(1.4f, rate) * basePrice) - 1;
                    sellPrices[work++] = Mathf.CeilToInt(rate * basePrice);
                    sellPrices[work++] = Mathf.CeilToInt(Random.Range(1.4f, rate) * basePrice) - 1;

                    // decreasing phase after the peak
                    if (work < 14)
                    {
                        rate = Random.Range(0.9f, 0.4f);
                        for (; work < 14; work++)
                        {
                            sellPrices[work] = Mathf.CeilToInt(rate * basePrice);
                            rate -= 0.03f;
                            rate -= Random.Range(0, 0.02f);
                        }
                    }
                    break;
            }

            sellPrices[0] = 0;
            sellPrices[1] = 0;
        }
    };

    public static int[] Calculate(Scr_PriceManager.Patterns pattern, int basePrice, int[] sellPrice)
    {
        TurnipPrices turnips = new TurnipPrices
        {
            whatPattern = (int)pattern > 0 ? (int)pattern - 1 : 0,
            basePrice = basePrice,
            sellPrices = new int[14]
        };

        for (int i = 0; i < 14; i++)
        {
            if (i < 2)
                turnips.sellPrices[i] = basePrice;
            else
                turnips.sellPrices[i] = sellPrice[i - 2];
        }

        turnips.calculate();
        /*
        printf("Pattern %d:\n", turnips.whatPattern);
        printf("Sun  Mon  Tue  Wed  Thu  Fri  Sat\n");
        printf("%3d  %3d  %3d  %3d  %3d  %3d  %3d\n",
               turnips.basePrice,
               turnips.sellPrices[2], turnips.sellPrices[4], turnips.sellPrices[6],
               turnips.sellPrices[8], turnips.sellPrices[10], turnips.sellPrices[12]);
        printf("     %3d  %3d  %3d  %3d  %3d  %3d\n",
               turnips.sellPrices[3], turnips.sellPrices[5], turnips.sellPrices[7],
               turnips.sellPrices[9], turnips.sellPrices[11], turnips.sellPrices[13]);
               */
        return turnips.sellPrices;
    }
}