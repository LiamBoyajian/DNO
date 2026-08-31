namespace Main.main.packages.containers;

public interface IContainerActions
{
}

public interface IWaterable : IContainerActions
{
    public double GiveWater(double amount);
}

public interface IInjectable : IContainerActions
{
    public void InjectDbId(int id);
}