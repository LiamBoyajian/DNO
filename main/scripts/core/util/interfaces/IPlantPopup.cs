using Godot;

namespace Main.main.scripts.core.util.interfaces;

public interface IPlantPopup
{
    /**
     * Remove all information displays from the window.
     */
    public bool ClearElements();

    /**
     * Display a node to the window
     */
    public void Popup(Node parent);
}