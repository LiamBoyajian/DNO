using System;

namespace Main.Source.main;

public interface IMaterialResource
{
    public double Max { get; }

    public double Amount { get; }
}

public class MaterialResource(double amount, double max) : IMaterialResource
{
    /**
     * Storage here?
     */


    public double Max { get; private set; } = max;

    public double Amount { get; private set; } = amount;

    public double ReturnPercent()
    {
        if (Max == 0.0) return 0;
        return Amount / Max;
    }

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

    public bool Increment()
    {
        if (Amount + 1.0 >= Max) return false;
        Amount++;
        return true;
    }

    public bool Decrement()
    {
        if (Amount - 1.0 <= 0.0) return false;
        Amount--;
        return true;
    }

    public void SetEmpty()
    {
        Amount = 0;
    }

    public bool ChangeMax(double change)
    {
        if (Max + change < 0) return false;
        Max += change;
        return true;
    }
}