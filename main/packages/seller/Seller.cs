using Godot;
using Main.main.packages.chatbubble;

namespace Main.main.packages.Seller;

public partial class Seller : AnimatedSprite2D
{
    [Export] protected PackedScene ChatBubbleScene;

    public override void _Ready()
    {
        base._Ready();
        var node = ChatBubbleScene.Instantiate();
        if (node is ChatBubble chatBubble)
        {
            chatBubble.Text.Text = "booty";
            chatBubble.Modulate = new Color(180, 180, 255);
        }


        AddChild(node);
    }
}