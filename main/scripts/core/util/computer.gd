extends Node

@export var plantId: int = 1
@export var strandId: int
@export var geneId: int


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass

func set_data(plant: int, strand: int, gene: int) -> void:
	plantId = plant
	strandId = strand
	geneId = gene
	
	
func has_plant_id() -> bool:
	return plantId > 0
func has_strand_id() -> bool:
	return plantId > 0
func has_gene_id() -> bool:
	return plantId > 0
	
func get_plant_data_string() -> String:
	return "%d.%d.%d" % [plantId, strandId, geneId]
