namespace Main.main.packages.model.Dna;

public interface IProteins
{
    void RequestProteinSynthesis();
}

public interface IDirigent : IProteins
{
    double UptakeWaterAmount { get; }
    double UptakeWater(double uptakeAmount);
}