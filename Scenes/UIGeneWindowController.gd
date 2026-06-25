extends Window

var _databasePath: String = ProjectSettings.globalize_path("user://greenhouse.db")


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	close_requested.connect(_on_window_close_requested)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass


func _on_window_close_requested() -> void:
	hide()
	pass


func _get_dna_strands(plant_id: int) -> Array:

	var db = SQLite.new()
	db.path = _databasePath
	db.open_db()

	var query: String = "SELECT id, plant_id, gene_name FROM dna_strands WHERE plant_id = ? ORDER BY id ASC"
	db.query_with_bindings(query, [plant_id])

	var results: Array = db.query_result

	return results
	
