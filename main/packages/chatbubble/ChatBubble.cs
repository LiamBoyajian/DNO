using System;
using Godot;
using Main.addons.EnumToIcon.EnumToStringDatabase;
using Main.addons.EnumToIcon.EnumToStringDatabase.main;

namespace Main.main.packages.chatbubble;

public partial class ChatBubble(string text = "...") : PanelContainer
{
    public ChatBubble() : this("...")
    {
    }

    [Export] public TextureRect Icon;
    [Export] public Label Text;

    public override void _Ready()
    {
        base._Ready();
        if (Icon == null) throw new Exception("Icon is null");
        if (Text == null) throw new Exception("Text is null");
        Text.Text = text;
    }

    public bool SetTexture(Enum @enum)
    {
        var texture2D = MemoryToDb.GetTextureFromEntry(new Entry(@enum));

        return texture2D != null;
    }
}