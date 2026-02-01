using System;
using System.Runtime.InteropServices.JavaScript;
using Godot;
using Main.InventoryAssets;

namespace Main.Package;

public class CricketVisuals
{
    //[SuppressMessage("ReSharper", "PossibleLossOfFraction")]

    /**
         * Function used to generate nodes in a grid pattern (left to right descending).
         * Argument [int count] nodes to be added to the scene.
         * Return [int result] value that holds the number of nodes which could not be created. Total requested - Total made.
         *
         *
         */
    public static int GenerateNodeGrid(Node node, Vector2 nodeSize, int nodePadding, int count,
        Node container, Vector2 containerSize, int? paddingLr)
    {
        //container.Get("position");
        var nodeCount = 0;
        var result = count;
        paddingLr ??= (int)(containerSize.X % (nodeSize.X + nodePadding) / 2);
        //var paddingLr = 0;//; I might add this back later. Basically just controls justification


        var nextBoxCoordinate =
            new Vector2((float)(nodeSize.X / 2 + paddingLr), (float)(nodeSize.Y / 2 + nodePadding)); //Shift 

        for (var i = 0; i < count; i++)
        {
            //TODO-Bug one extra horizontal line of boxes is being created

            if (nextBoxCoordinate.X + nodeSize.X + (nodeSize.X / 2) >
                (containerSize.X - paddingLr)) //if the next box's X coordinate is out of bounds.
            {
                nextBoxCoordinate.X = (float)(nodeSize.X / 2 + paddingLr);
                nextBoxCoordinate.Y += nodeSize.Y + nodePadding;
            }

            if (nextBoxCoordinate.Y + (nodeSize.Y / 2) >
                containerSize.Y) //if the next box's Y coordinate is out of bounds.
                return result; // No space remaining


            var newNode = node.Duplicate();
            newNode.Name = "node" + nodeCount;

            ++nodeCount;
            --result;

            newNode.Set("position", nextBoxCoordinate);

            nextBoxCoordinate.X += nodeSize.X + nodePadding;

            container.AddChild(newNode);
        }

        return result;
    }
}