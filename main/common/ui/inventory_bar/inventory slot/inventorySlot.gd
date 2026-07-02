extends Panel

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _can_drop_data(at_position: Vector2, data: Variant) -> bool:
	return true


func _drop_data(at_position: Vector2, data: Variant) -> void:
	data.reparent(self)
	data.position = Vector2.ZERO
	pass


func _get_drag_data(at_position: Vector2) -> Variant:

	var temp = get_child(0)

	if temp == null:
		return

	set_drag_preview(get_child(0).duplicate())
	#remove_child(temp)

	return temp
	
