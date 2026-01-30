using System;

namespace Main.InventoryAssets;


/*
 * Used to create dataholders to be used in items. 
 */
public interface IDataHolder<TData>
{
    TData Data { get; set; }
}