using System;
using System.Runtime.InteropServices.JavaScript;
using Godot;

namespace Main.Package;

public class CricketVisuals
{
    //[SuppressMessage("ReSharper", "PossibleLossOfFraction")]
    public static int GenerateSpriteGrid<TG>(Node sprite, Vector2 spriteSize, int spritePadding, int count,
        Node container, Vector2 containerSize)
    {
        if (sprite is not TG)
            throw new ArgumentException("sprite param is not passed a sprite or animated sprite");

        //container.Get("position");
        var spriteCount = 0;
        var result = count;
        var paddingLr = (int)(containerSize.X % (spriteSize.X + spritePadding) / 2);


        var nextBoxCoordinate =
            new Vector2((float)(spriteSize.X / 2 + paddingLr), (float)(spriteSize.Y / 2 + spritePadding)); //Shift 

        for (var i = 0; i < count; i++)
        {
            //TODO-Bug one extra horizontal line of boxes is being created

            if (nextBoxCoordinate.X + spriteSize.X + (spriteSize.X / 2) >
                (containerSize.X - paddingLr)) //if the next box's X coordinate is out of bounds.
            {
                nextBoxCoordinate.X = (float)(spriteSize.X / 2 + paddingLr);
                nextBoxCoordinate.Y += spriteSize.Y + spritePadding;
            }

            if (nextBoxCoordinate.Y + (spriteSize.Y / 2) >
                containerSize.Y) //if the next box's Y coordinate is out of bounds.
                return result; // No space remaining


            var newSprite = sprite.Duplicate();
            newSprite.Name = "box" + spriteCount;

            ++spriteCount;
            --result;

            newSprite.Set("position", nextBoxCoordinate);

            nextBoxCoordinate.X += spriteSize.X;

            container.AddChild(newSprite);
        }

        return result;
    }
}