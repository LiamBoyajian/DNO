extends "res://main/scripts/SceneControl/CloseWindow.gd"


@export var dna_strand_container: VBoxContainer
@export var panel_template: PackedScene
@export var gene_container_template: PackedScene
@export var gene_template: PackedScene
@export var plant_id: int = 1
@export var geneButtonGroup: ButtonGroup

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

	var plant = DbManager.GetPlant(plant_id)
	#var plant = DbManager.get_plant(plant_id)

	_display_strands_to_editor(Array(plant.GetChildren()))


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
		var genes: Array = strand.GetChildren()
		for gene in genes:
			if not gene:
				continue
			
			var temp_gene = gene_template.instantiate() #kills the link
			
			temp_gene_cont.add_child(temp_gene)
			temp_gene.button_group = geneButtonGroup
			
			
			temp_gene.toggle_mode = true
	
			var alphaC = .9
			var firstColor = Color(0.78431374, 0.21568628, 0.21568628, alphaC)
			var secondColor = Color(0.15686275, 0.3137255, 0.5882353, alphaC)
			
			var ttt = temp_gene.get_theme_stylebox("normal").duplicate()#$= Color(0.78431374, 0.21568628, 0.21568628).lerp(Color(0.15686275, 0.3137255, 0.5882353), colorWeight)
			ttt.bg_color = firstColor.lerp(secondColor, colorWeight)
			temp_gene.add_theme_stylebox_override("normal",ttt)
			
			
			var tempHover = temp_gene.get_theme_stylebox("hover")
			tempHover.bg_color = Color(1,1,1,.8)
			
			
			colorWeight += .33
