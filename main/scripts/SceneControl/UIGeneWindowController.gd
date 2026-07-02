extends "res://CloseWindow.gd"

var _databasePath: String = ProjectSettings.globalize_path("user://greenhouse.db")

@export var dna_strand_container: VBoxContainer
@export var panel_template: PackedScene
@export var gene_container_template: PackedScene
@export var gene_template: PackedScene


# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	super._ready()

	if not dna_strand_container:
		push_error("dna_strand_container has no reference.")
	if not panel_template:
		push_error("panel_template has no reference.")
	if not gene_container_template:
		push_error("gene_container_template has no reference.")
	if not gene_template:
		push_error("gene_template has no reference.")

	var strands = _get_dna_strands(1)

	_display_strands_to_editor(strands)


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(_delta: float) -> void:
	pass


func _on_window_close_requested() -> void:
	hide()
	pass


func _display_strands_to_editor(strands: Array):

	for strand in strands:
		if not strand:
			continue
		var temp_panel     = panel_template.instantiate()
		var temp_gene_cont = gene_container_template.instantiate()

		temp_panel.add_child(temp_gene_cont)
		dna_strand_container.add_child(temp_panel)

		#could optimize by preloading all the genes for that plant
		#needs very minimal sql
		var colorWeight = 0;
		var genes: Array = _get_genes(strand["id"])
		for gene in genes:
			if not gene:
				continue
			var temp_gene = gene_template.instantiate()
			temp_gene_cont.add_child(temp_gene)
			temp_gene.self_modulate = Color (0.78431374, 0.21568628, 0.21568628).lerp(Color(0.15686275, 0.3137255, 0.5882353), colorWeight);
			colorWeight += .33


func _get_dna_strands(plant_id: int) -> Array:

	var db = SQLite.new()
	db.path = _databasePath
	db.open_db()

	var query: String = "SELECT id, plant_id, name FROM dna_strands WHERE plant_id = ? ORDER BY id ASC"
	db.query_with_bindings(query, [plant_id])

	var results: Array = db.query_result

	db.close_db()
	return results


func _get_genes(dna_id: int) -> Array:
	var db = SQLite.new()
	db.path = _databasePath
	db.open_db()

	var query: String = "SELECT id, strand_id FROM genes WHERE strand_id = ? ORDER BY id ASC"

	db.query_with_bindings(query, [dna_id])

	var results: Array = db.query_result

	db.close_db()
	return results;
