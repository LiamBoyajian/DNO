using System;
using Godot;

namespace Main.main.scripts.core.util;

public partial class SceneData<THeadType>(SceneData<THeadType> head) : Node
    where THeadType : SceneData<THeadType>
{
    public SceneData() : this(null)
    {
    }

    [Export] protected SceneData<THeadType> HeadDataNode = head;


    public bool HasHeadNode()
    {
        return HeadDataNode != null;
    }

    public SceneData<THeadType> SetHeadNode(SceneData<THeadType> head)
    {
        HeadDataNode = head;
        return HeadDataNode;
    }

    public bool HasValidHeadRoot()
    {
        return GetHeadRoot(true) != null;
    }

    public SceneData<THeadType> GetHeadNode(bool nullable)
    {
        if (nullable)
            return HeadDataNode;
        if (HeadDataNode == this)
            throw new InvalidOperationException("HeadDataNode is this");
        if (HeadDataNode == null)
            throw new InvalidOperationException("HeadDataNode is null");
        return HeadDataNode;
    }

    /**
     * Cannot be null
     */
    public THeadType GetHeadRoot(bool nullable)
    {
        if (this is THeadType)
            return (THeadType)this; //might throw an error idk; I suspect not

        if (HeadDataNode == null)
        {
            return nullable ? null : throw new InvalidOperationException("HeadDataNode is this");
        }

        return HeadDataNode.GetHeadRoot(true);
    }

    public void Updated()
    {
        EmitSignal(SignalName.WasUpdated);
        //throw new NotImplementedException();
    }

    [Signal]
    public delegate void WasUpdatedEventHandler();
}