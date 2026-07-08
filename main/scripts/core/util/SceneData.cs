using System;
using Godot;

namespace Main.main.scripts.core.util;

public partial class SceneData : Node
{
    [Export] protected SceneData HeadDataNode;

    public bool HasHeadNode()
    {
        return HeadDataNode != null;
    }

    public SceneData GetHeadNode(bool nullable)
    {
        if (nullable)
            return HeadDataNode;
        if (HeadDataNode == this)
            throw new InvalidOperationException("HeadDataNode is this");
        if (HeadDataNode == null)
            throw new InvalidOperationException("HeadDataNode is null");
        return HeadDataNode;
    }

    public SceneData GetHeadRoot()
    {
        if (HeadDataNode == null)
            return this;
        return HeadDataNode.GetHeadRoot();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }
}