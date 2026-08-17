using System;

namespace Main.Source.main;

public interface IMaterialResource
{
    public double Max { get; }

    public double Amount { get; }

    public bool IsEmpty()
    {
        return Amount == 0;
    }

    public bool IsMaxed()
    {
        return Amount >= Max;
    }

    /**
    * returns the val of amount is > val; otherwise returns the current amount
    */
    public double ReturnPercent() => Max <= 0.0 ? 0 : Amount / Max;

    public double HasValue(double val) => val <= Amount ? val : Amount;
}

public class MaterialResource(double amount, double max) : IMaterialResource
{
    /**
     * Storage here?
     */


    public double Max { get; private set; } = max;

    public double Amount { get; private set; } = amount;

    public double ReturnPercent() => Max <= 0.0 ? 0 : Amount / Max;
    public double HasValue(double val) => val <= Amount ? val : Amount;

    /**
     * Param: amount to add to this.Amount
     * result: how much was given
     */
    public double Give(double amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

        if (amount + Amount > Max)
        {
            var result = Max - Amount;
            Amount = Max;
            return result;
        }

        Amount += amount;
        return amount;
    }

    /**
     * Param: amount to subtract from this.Amount
     * result: how much was taken
     */
    public double Take(double amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

        if (amount > Amount)
        {
            var result = Amount;
            Amount = 0;
            return result;
        }

        Amount -= amount;
        return amount;
    }

    public bool IsEmpty()
    {
        return Amount <= 0;
    }

    public bool IsMaxed()
    {
        return Amount >= Max;
    }

    public void Increment()
    {
        if (Amount + 1.0 >= Max)
        {
            Amount = Max;
        }
        else
        {
            Amount++;
        }
    }

    public void Decrement()
    {
        if (Amount - 1.0 <= 0.0)
        {
            Amount = 0;
        }
        else
        {
            Amount--;
        }
    }

    public void SetEmpty()
    {
        Amount = 0;
    }

    /**
     * param double: amount to change (increase or decrease) by.
     * returns: amount changed by.
     */
    public double ChangeMax(double change)
    {
        if (Max + change < 0)
        {
            var result = Max;
            Max = 0;
            return -1.0 * result;
        }

        Max += change;

        if (Amount > Max)
            Amount = Max;

        return change;
    }
}