using System;
using System.Collections;
using Godot;

namespace Main.InventoryAssets;
/**
 * Can be used for any object that has storage. Nodes are required so extending node for each item or using sub children is required. 
 */
public partial class Inventory(int max) : Node
{
    //Consider replacing with a new object; something like item or something. Maybe not needed...
    private ArrayList _array = new ArrayList();
    private int _maxItems = max; //create a new inventory to upgrade... or I'll make an addition system im not sure.

    public Node SwapAtIndex(int index, Node givenNode)
    {
        var result = RemoveNode(index);
        AddNode(index, givenNode);
        return result;
    }

    public int AddNode(Node node)
    {
        if (_ensureCapacity()) return -1;
        return _array.Add(node);
    }

    public int AddNode(int index, Node node)
    {
        if (_ensureCapacity()) return -1;
        _array.Insert(index, node);
        return index;
    }
    
    public Node RemoveNode(int index)
    {
        if (_array.Count == 0) throw new ArgumentOutOfRangeException(nameof(index),"Inventory already empty.") ;
        if (index >= _array.Count) throw new IndexOutOfRangeException("Index is greater than the Inventory size or negative.");
        var result = (Node)_array[index];
        _array.RemoveAt(index);
        return result;
    }
    
    private bool _ensureCapacity()
    {
        return _array.Count >= _maxItems;
    }

    public int Search()
    {
        return -1; //unimplemented
    }
    public int Sort()
    {
        return -1; //unimplemented
    }
}