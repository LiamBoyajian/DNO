class_name AbstractWindow
extends Window

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	close_requested.connect(_on_window_close_requested)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _on_window_close_requested() -> void:
	hide()
	pass
