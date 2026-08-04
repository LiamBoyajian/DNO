namespace Main.main.scripts.core.util.inventory;

public interface IPlantAbilities
{
}

public interface IShearable : IPlantAbilities
{
    public int GetShear();
    public int Shear(int sheared);
}