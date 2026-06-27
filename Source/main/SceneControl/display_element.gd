extends TextureButton

enum DisplayMode{
	SHOW,
	HIDE,
	TOGGLE,
}

@export var target_node: Node
@export var hide_self: bool
@export var click_behavior: DisplayMode
# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pressed.connect(_button_pressed)
	if not target_node:
		push_error("Missing display mode")


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func _button_pressed():
	if not target_node:
		return
		
	match click_behavior:
		DisplayMode.SHOW:
			target_node.show()
		DisplayMode.HIDE:
			target_node.hide()
		DisplayMode.TOGGLE:
			target_node.visible = !target_node.visible
	
	if hide_self:
			hide()
