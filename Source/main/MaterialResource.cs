using System;

namespace Main.Source.main;

public class MaterialResource(double amount, double max)
{
    /**
     * Storage here?
     */


    public double Max { get; } = max;

    public double Amount { get; private set; } = amount;

    public double ReturnPercent()
    {
        if (Max == 0.0) return 0;
        return Amount / Max;
    }

    /**
     * Param: amount to add to this.Amount
     * result: overflow total after addition
     */
    public double Give(double amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

        if (amount + Amount > Max)
        {
            Amount = Max;
            return amount - (Max - Amount);
        }

        Amount += amount;
        return 0.0;
    }

    /**
     * Param: amount to subtract from this.Amount
     * result: underflow total after removal
     */
    public double Take(double amount)
    {
        if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount));

        if (amount > Amount)
        {
            var result = amount - Amount;
            Amount = 0;
            return result;
        }

        Amount -= amount;
        return 0.0;
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
}