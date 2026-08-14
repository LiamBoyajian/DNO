namespace Main.main.packages.model.Dna;

public interface IProteins
{
    void RequestProteinSynthesis();
}

public interface IDirigent : IProteins
{
    double UptakeWaterAmount { get; set; }
    double UptakeWater(double uptakeAmount);
}