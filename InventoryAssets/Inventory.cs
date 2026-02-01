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

    public Item<TRemove> SwapAtIndex<TI, TRemove>(int index, Item<TI> item)
    {
        var result = RemoveNode<TRemove>(index);
        AddNode(index, item);
        return (Item<TRemove>)result;
    }

    public int AddNode<TI>(Item<TI> item)
    {
        if (_ensureCapacity()) return -1;
        return _array.Add(item);
    }

    public int AddNode<TI>(int index, Item<TI> item)
    {
        if (_ensureCapacity()) return -1;
        _array.Insert(index, item);
        return index;
    }

    public Node RemoveNode<TI>(int index)
    {
        if (_array.Count == 0) throw new ArgumentOutOfRangeException(nameof(index), "Inventory already empty.");
        if (index >= _array.Count)
            throw new IndexOutOfRangeException("Index is greater than the Inventory size or negative.");
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

    public int Count()
    {
        return _array.Count;
    }
    //public ArrayList Clone()
    //{
    //    return (ArrayList) _array.Clone(); //this might be terrible I haven't checked
    //}
}