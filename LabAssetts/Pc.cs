using System;
using Godot;
using Main.InventoryAssets;
using Main.Package;

namespace Main.LabAssetts;

public partial class Pc : AbstractMachine
{
    public Pc()
    {
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
        Inventory = new InventoryContainer(new Vector2(200, 50), 6, tempTextureButton);


        Inventory.Position = new Vector2(-100, -80);
        Inventory.GenNodeGrid(new Vector2(32, 32));
        Inventory.ZIndex = 1;
        //Inventory.Hide();


        Texture2D temp = (FindChild("Vial") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("outdated_DNA", 0);
        Texture2D temp2 = (FindChild("Vial") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("Empty", 0);
        Console.WriteLine(Inventory.AddItem(new Item<outdated_DNA>(temp2, null)));
        Inventory.AddItem(new Item<outdated_DNA>(temp2, null)); //TODO first item is getting killed idk
        while (-1 != Inventory.AddItem(new Item<outdated_DNA>(temp, new outdated_DNA(new RandomNumberGenerator()))))
            Console.WriteLine("Size " + Inventory.Count());
        ; //Remove after testing
        RunOnReady();
        //Inventory.UpdatedBufferSlot += SlotSwappedEventWrapper;
    }
}