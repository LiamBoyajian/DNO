using System;
using Godot;
using Main.InventoryAssets;
using Main.Package;

namespace Main.LabAssetts;

public partial class Pc(Vector2 size, int slots, TextureButton button) : AbstractMachine(size, slots, button)
{
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
        //this = new InventoryContainer(new Vector2(200, 50), 6, tempTextureButton);
        Size = new Vector2(200, 50);
        MaxItems = 6;
        Button = tempTextureButton;
        Position = new Vector2(-100, -80);
        GenNodeGrid(new Vector2(32, 32));
        ZIndex = 1;
        Hide();

        Texture2D temp = (FindChild("Vial") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("DNA", 0);
        Texture2D temp2 = (FindChild("Vial") as AnimatedSprite2D)?.SpriteFrames.GetFrameTexture("Plant", 0);
        //Inventory.AddItem(new Item<DNA>(temp2, new DNA(new RandomNumberGenerator())));
        while (-1 != AddItem(new Item<DNA>(temp, new DNA(new RandomNumberGenerator()))))
            Console.WriteLine("Size " + Count());
        ; //Remove after testing
        UpdateItemsDisplay();
    }
}