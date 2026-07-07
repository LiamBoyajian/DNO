extends ItemList

@export var sceneData: Node

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	update()
	item_selected.connect(update_scene_data)
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func update() -> void:
	clear()
	for plant in DbManager.GetPlant(false):
		add_item(str(plant.Id) + "." + plant.Species)
	pass
	
func string_to_texture(plantSpecies: String) -> Texture:
	return null #STUB TODO 
	
func update_scene_data() -> void:
	
	pass