using System;
using System.Collections.Generic;
using System.Numerics;
using Godot;
using Main.Package;
using Vector2 = Godot.Vector2;

namespace Main.Source;

public struct SizePos(Vector2 pos, Vector2 size)
{
    public Vector2 Position = pos;
    public Vector2 Size = size;

    public override string ToString()
    {
        return $"{Position}, {Size}";
    }
    //public ReadOnlySpan<>
}

public struct Machines
{
    public readonly SizePos[] Elements;

    public Machines(Godot.Collections.Array<AbstractMachine> machines)
    {
        Elements = new SizePos[machines.Count];
        for (var i = 0; i < machines.Count; i++)
        {
            var machineSprite = machines[i];
            if (machineSprite == null) continue;

            Elements[i] = new SizePos(machineSprite.GetSpritePosition(), machineSprite.GetSpriteSize());
        }
    }

    public Machines(Godot.Collections.Array<Node> machines)
    {
        Elements = new SizePos[machines.Count];
        for (var i = 0; i < machines.Count; i++)
        {
            var machineSprite = machines[i] as AbstractMachine;
            if (machineSprite == null) continue;

            Elements[i] = new SizePos(machineSprite.GetSpritePosition(), machineSprite.GetSpriteSize());
        }
    }
}