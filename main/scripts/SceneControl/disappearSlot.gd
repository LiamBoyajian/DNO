@abstract class_name disappearSlot
extends Panel

var accepting: bool = true


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _can_drop_data(at_position: Vector2, data: Variant) -> bool:
	return accepting


func _drop_data(at_position: Vector2, data: Variant) -> void:
	data.reparent(self)
	data.position = Vector2.ZERO

	accepting = false
	_after_accepted(data)
	self.queue_free()
	pass


@abstract func _after_accepted(seed: Variant) -> Variant
	
