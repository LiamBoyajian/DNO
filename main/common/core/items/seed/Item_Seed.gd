class_name Item_Seed
extends item_base

#SHOULD NOT BE EXPORTS; CURRENTLY USED FOR TESTING; REMOVE IF YOU ARE READING THIS
@export var _plant_type: Script
@export var _frames: SpriteFrames
@export var _plant_scene: PackedScene


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func assign_species(species: Script) -> bool:
	if _plant_type.is_ancestor_of_count(species):
		_plant_type = species
		return true
	assert("species (param) is not child of abstract plant")
	return false


func get_plant_type() -> Script:
	return _plant_type


func get_frames() -> SpriteFrames:
	return _frames


func get_plant_scene() -> PackedScene:
	return _plant_scene
