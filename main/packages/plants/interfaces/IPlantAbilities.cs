namespace Main.main.packages.plants.interfaces;

public interface IPlantAbilities
{
}

public interface IShearable : IPlantAbilities
{
    public int GetShear();
    public int Shear(int sheared);
}