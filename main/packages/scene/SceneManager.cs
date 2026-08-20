using Godot;
using System.Collections.Generic;

namespace Main.main.packages.scene;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }

    private readonly Dictionary<string, Node> _loadedScenes = new();
    private Node _currentScene;

    public override void _Ready()
    {
        Instance = this;
        // Grab whatever scene Godot initially loaded as current
        _currentScene = GetTree().CurrentScene;
    }

    public void ChangeScene(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PrintErr("SceneManager: Target scene path is empty!");
            return;
        }

        // 1. Detach current scene from Root without freeing it
        if (_currentScene != null && _currentScene.GetParent() != null)
        {
            _currentScene.GetParent().RemoveChild(_currentScene);
        }

        // 2. Retrieve cached instance or instantiate a new one
        if (!_loadedScenes.TryGetValue(scenePath, out Node targetScene))
        {
            var packedScene = ResourceLoader.Load<PackedScene>(scenePath);
            if (packedScene == null)
            {
                GD.PrintErr($"SceneManager: Failed to load scene at {scenePath}");
                return;
            }

            targetScene = packedScene.Instantiate();
            _loadedScenes[scenePath] = targetScene;
        }

        // 3. Attach the cached scene to Root and update CurrentScene
        GetTree().Root.AddChild(targetScene);
        GetTree().CurrentScene = targetScene;
        _currentScene = targetScene;
    }
}