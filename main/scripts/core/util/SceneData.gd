extends Node
class_name SceneData

@export var head_data_node: Node 

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	var parent_scene_data = get_parent().get("scene_data")
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func has_head_node() -> bool:
	return head_data_node is not null
	
