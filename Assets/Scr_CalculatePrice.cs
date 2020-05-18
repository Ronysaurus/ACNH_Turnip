using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Scr_CalculatePrice : MonoBehaviour
{
    private const int RATE_MULTIPLIER = 10000;

    private static float range_length(float[] range)
    {
        return range[1] - range[0];
    }

    private static float clamp(float x, float min, float max)
    {
        return Mathf.Min(Mathf.Max(x, min), max);
    }

    private static float[] range_intersect(float[] range1, float[] range2)
    {
        if (range1[0] > range2[1] || range1[1] < range2[0])
        {
            return new float[1];
        }

        return new float[] { Mathf.Max(range1[0], range2[0]), Mathf.Min(range1[1], range2[1]) };
    }

    private static float range_intersect_length(float[] range1, float[] range2)
    {
        if (range1[0] > range2[1] || range1[1] < range2[0])
        {
            return 0;
        }
        return range_length(range_intersect(range1, range2));
    }

    private static float float_sum(float[] input)
    {
        // Uses the improved Kahan–Babuska algorithm introduced by Neumaier.
        float sum = 0;
        // The "lost bits" of sum.
        float c = 0;
        for (int i = 0; i < input.Length; i++)
        {
            float cur = input[i];
            float t = sum + cur;
            if (Mathf.Abs(sum) >= Mathf.Abs(cur))
            {
                c += (sum - t) + cur;
            }
            else
            {
                c += (cur - t) + sum;
            }
            sum = t;
        }
        return sum + c;
    }

    private static float[,] prefix_float_sum(float[] input)
    {
        float[,] prefix_sum = new float[input.Length + 1, 2];
        prefix_sum[0, 0] = 0;
        prefix_sum[0, 1] = 0;

        float sum = 0;
        float c = 0;
        for (int i = 0; i < input.Length; i++)
        {
            float cur = input[i];
            float t = sum + cur;
            if (Mathf.Abs(sum) >= Mathf.Abs(cur))
            {
                c += (sum - t) + cur;
            }
            else
            {
                c += (cur - t) + sum;
            }
            sum = t;
            prefix_sum[i + 1, 0] = sum;
            prefix_sum[i + 1, 1] = c;
        }
        return prefix_sum;
    }

    private class PDF
    {
        private int value_start, value_end;
        private float[] range;
        private float total_length;
        private float[] prob;
        /**
         * Initialize a PDF in range [a, b], a and b can be non-integer.
         * if uniform is true, then initialize the probability to be uniform, else initialize to a
         * all-zero (invalid) PDF.
         * @param {number} a - Left end-point.
         * @param {number} b - Right end-point end-point.
         * @param {boolean} uniform - If true, initialise with the uniform distribution.
         */

        private PDF(float a, float b, bool uniform = true)
        {
            // We need to ensure that [a, b] is fully contained in [value_start, value_end].
            /** @type {number} */
            this.value_start = Mathf.FloorToInt(a);
            /** @type {number} */
            this.value_end = Mathf.CeilToInt(b);
            this.range = new float[] { a, b };
            total_length = range_length(range);
            /** @type {number[]} */
            this.prob = new float[this.value_end - this.value_start];
            if (uniform)
            {
                for (int i = 0; i < this.prob.Length; i++)
                {
                    this.prob[i] = range_intersect_length(this.range_of(i), range) / total_length;
                }
            }
        }

        private float[] range_of(int idx)
        {
            return new float[] { this.value_start + idx, this.value_start + idx + 1 };
        }

        private float min_value()
        {
            return value_start;
        }

        private float max_value()
        {
            return value_end;
        }

        private float normalize()
        {
            float total_probability = float_sum(this.prob);
            for (int i = 0; i < this.prob.Length; i++)
            {
                this.prob[i] /= total_probability;
            }
            return total_probability;
        }

        private float range_limit(float[] range)
        {
            float start = range[0], end = range[1];
            start = Mathf.Max(start, this.min_value());
            end = Mathf.Min(end, this.max_value());
            if (start >= end)
            {
                // Set this to invalid values
                this.value_start = this.value_end = 0;
                this.prob = new float[1];
                return 0;
            }
            start = Mathf.Floor(start);
            end = Mathf.Ceil(end);

            int start_idx = (int)(start - this.value_start);
            int end_idx = (int)(end - this.value_start);
            for (int i = start_idx; i < end_idx; i++)
            {
                this.prob[i] *= range_intersect_length(this.range_of(i), range);
            }
            prob = (prob.Skip(start_idx).Take(end_idx)).ToArray();
            this.value_start = (int)start;
            this.value_end = (int)end;

            // The probability that the value was in this range is equal to the total
            // sum of "un-normalised" values in the range.
            return this.normalize();
        }

        private void decay(int rate_decay_min, int rate_decay_max)
        {
            // The sum of this distribution with a uniform distribution.
            // Let's assume that both distributions start at 0 and X = this dist,
            // Y = uniform dist, and Z = X + Y.
            // Let's also assume that X is a "piecewise uniform" distribution, so
            // x(i) = this.prob[Math.floor(i)] - which matches our implementation.
            // We also know that y(i) = 1 / max(Y) - as we assume that min(Y) = 0.
            // In the end, we're interested in:
            // Pr(i <= Z < i+1) where i is an integer
            // = int. x(val) * Pr(i-val <= Y < i-val+1) dval from 0 to max(X)
            // = int. x(floor(val)) * Pr(i-val <= Y < i-val+1) dval from 0 to max(X)
            // = sum val from 0 to max(X)-1
            //     x(val) * f_i(val) / max(Y)
            // where f_i(val) =
            // 0.5 if i-val = 0 or max(Y), so val = i-max(Y) or i
            // 1.0 if 0 < i-val < max(Y), so i-max(Y) < val < i
            // as x(val) is "constant" for each integer step, so we can consider the
            // integral in integer steps.
            // = sum val from max(0, i-max(Y)) to min(max(X)-1, i)
            //     x(val) * f_i(val) / max(Y)
            // for example, max(X)=1, max(Y)=10, i=5
            // = sum val from max(0, 5-10)=0 to min(1-1, 5)=0
            //     x(val) * f_i(val) / max(Y)
            // = x(0) * 1 / 10

            // Get a prefix sum / CDF of this so we can calculate sums in O(1).
            float[,] prefix = prefix_float_sum(this.prob);
            float max_X = this.prob.Length;
            float max_Y = rate_decay_max - rate_decay_min;
            float[] newProb = new float[] { (this.prob.Length + max_Y) };
            for (int i = 0; i < newProb.Length; i++)
            {
                // Note that left and right here are INCLUSIVE.
                int left = (int)Math.Max(0, i - max_Y);
                int right = (int)Math.Min(max_X - 1, i);
                // We want to sum, in total, prefix[right+1], -prefix[left], and subtract
                // the 0.5s if necessary.
                // This may involve numbers of differing magnitudes, so use the float sum
                // algorithm to sum these up.
                List<float> numbers_to_sum = new List<float> { prefix[right + 1, 0], prefix[right + 1, 1], -prefix[left, 0], -prefix[left, 1] };

                if (left == i - max_Y)
                {
                    // Need to halve the left endpoint.
                    numbers_to_sum.Add(-this.prob[left] / 2);
                }
                if (right == i)
                {
                    // Need to halve the right endpoint.
                    // It's guaranteed that we won't accidentally "halve" twice,
                    // as that would require i-max_Y = i, so max_Y = 0 - which is
                    // impossible.
                    numbers_to_sum.Add(-this.prob[right] / 2);
                }
                newProb[i] = float_sum(numbers_to_sum.ToArray()) / max_Y;
            }

            this.prob = newProb;
            this.value_start -= rate_decay_max;
            this.value_end -= rate_decay_min;
            // No need to normalise, as it is guaranteed that the sum of this.prob is 1.
        }
    }

    private class Predictor
    {
        private int fudge_factor;
        private int[] prices;
        private bool first_buy;
        private Scr_PriceManager.Patterns previous_pattern;

        private Predictor(int[] prices, bool firstBuy, Scr_PriceManager.Patterns previous)
        {
            fudge_factor = 0;
            this.prices = prices;
            first_buy = firstBuy;
            previous_pattern = previous;
        }

        private int intceil(float val)
        {
            return (int)Math.Truncate(val + 0.99999);
        }

        private float minimum_rate_from_given_and_base(float given_price, float buy_price)
        {
            return (float)(RATE_MULTIPLIER * (given_price - 0.99999) / buy_price);
        }

        private float maximum_rate_from_given_and_base(float given_price, float buy_price)
        {
            return (float)(RATE_MULTIPLIER * (given_price + 0.00001) / buy_price);
        }

        private float[] rate_range_from_given_and_base(float given_price, float buy_price)
        {
            return new float[] { this.minimum_rate_from_given_and_base(given_price, buy_price), this.maximum_rate_from_given_and_base(given_price, buy_price) };
        }

        private float get_price(float rate, float basePrice)
        {
            return this.intceil(rate * basePrice / RATE_MULTIPLIER);
        }

        private float generate_individual_random_price(float[] given_prices, List<float> predicted_prices, int start, float length, float rate_min, float rate_max)
        {
            rate_min *= RATE_MULTIPLIER;
            rate_max *= RATE_MULTIPLIER;

            float buy_price = given_prices[0];
            float[] rate_range = new float[] { rate_min, rate_max };
            float prob = 1;

            for (int i = start; i < start + length; i++)
            {
                float min_pred = this.get_price(rate_min, buy_price);
                float max_pred = this.get_price(rate_max, buy_price);
                if (!double.IsNaN(given_prices[i]))
                {
                    if (given_prices[i] < min_pred - this.fudge_factor || given_prices[i] > max_pred + this.fudge_factor)
                    {
                        // Given price is out of predicted range, so this is the wrong pattern
                        return 0;
                    }
                    // TODO: How to deal with probability when there's fudge factor?
                    // Clamp the value to be in range now so the probability won't be totally biased to fudged values.
                    float[] real_rate_range = this.rate_range_from_given_and_base(clamp(given_prices[i], min_pred, max_pred), buy_price);
                    prob *= range_intersect_length(rate_range, real_rate_range) /
                      range_length(rate_range);
                    min_pred = given_prices[i];
                    max_pred = given_prices[i];
                }

                predicted_prices.Add({ min: min_pred, max: max_pred,});
        }

    return prob;
  }

    public int[] Calculate(Scr_PriceManager.Patterns _pattern, int _basePrice, int[] _sellPrice)
    {
        return new int[1];
    }
}