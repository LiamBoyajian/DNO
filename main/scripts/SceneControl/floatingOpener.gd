extends "res://Source/main/SceneControl/display_element.gd"

@export var float_speed: float = 2.0
@export var float_amplitude: float = 2

var time_passed
var base_y: float


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	super._ready()
	time_passed = randf_range(0.0, 100.0)
	base_y = position.y


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	time_passed += delta * float_speed
	var offset_y = sin(time_passed) * float_amplitude
	#print(offset_y)
	#print("time" + str(time_passed))
	position.y = base_y + offset_y
