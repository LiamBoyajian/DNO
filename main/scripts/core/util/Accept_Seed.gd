extends "res://main/common/ui/disappear slot/disappearSlot.gd"

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass


func _after_accepted(seed: Variant) -> Variant:


	if self.get_parent() is not ContainPlant:
		assert("parent is not type: ContainPlant")
		return null
	if seed is not Item_Seed:
		assert("param is not type: Item_Seed")
		return null

	var templatePlant = seed.get_plant_scene().instantiate()
	templatePlant.name = "soybean - id:" + str(seed.get_plant_db_id())
	templatePlant.set_script(seed.get_plant_type())
	templatePlant.get_child(0).sprite_frames = seed.get_frames()
	
	templatePlant.scale *= 4
	templatePlant.position = Vector2(0, -78)
	
	var newPlant = self.get_parent().AcceptSeed(templatePlant, seed.get_plant_db_id())

	templatePlant.Init()

	return null
