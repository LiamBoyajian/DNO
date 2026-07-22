using Godot;

namespace Main.main.scripts.core.util.inventory;

public partial class InventoryDisplay : Control
{
    public static InventoryDisplay Instance { get; private set; }
    private int _money = 0;
    [Export] protected Label MoneyLabel;

    [Export] protected string MoneyLabelUnit = "kr";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void AddMoney(int amount, bool update = false)
    {
        if (amount > 0)
            _money += amount;

        if (update)
            UpdateMoney();
    }

    public int GetMoney()
    {
        return _money;
    }

    public bool UpdateMoney()
    {
        if (MoneyLabel == null)
        {
            GD.PrintErr("Money label is null");
            return false;
        }

        MoneyLabel.Text = GetMoney().ToString();
        return true;
    }

    public bool UpdateAll()
    {
        return UpdateMoney();
    }
}