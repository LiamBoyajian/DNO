using System;
using Godot;
using Main.InventoryAssets;
using Main.Package;

namespace Main.LabAssetts;

public partial class Pc : AbstractMachine
{
    public Pc()
    {
        //TODO replace this with a handler or something nicer idk
    }

    public override void _Ready()
    {
        var tempTextureButton = new TextureButton();
        tempTextureButton.TextureNormal =
            (FindChild("_box") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("Black", 0);
        tempTextureButton.TextureHover =
            (FindChild("_box") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("Selected", 0);
        tempTextureButton.TexturePressed =
            (FindChild("_box") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("Selected", 0);

        tempTextureButton.ToggleMode = true;
        tempTextureButton.Show();
        Inventory = new InventoryContainer(new Vector2(200, 50), 5, tempTextureButton);
        Inventory.Position = new Vector2(-100, -80);
        Inventory.GenNodeGrid(new Vector2(32, 32));
        Inventory.ZIndex = 1;
        Inventory.Hide();
        AddChild(Inventory);


        Texture2D temp = (FindChild("Vial") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("DNA", 0);
        Texture2D temp2 = (FindChild("Vial") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("Plant", 0);

        Inventory.AddItem(new Item<DNA>(temp2, new DNA(new RandomNumberGenerator())));
        while (-1 != Inventory.AddItem(new Item<DNA>(temp, new DNA(new RandomNumberGenerator()))))
            ; //Remove after testing
    }
}