class_name GDManiaSkinModule extends Node

## A module for [class ManiaNoteSkin]. Attaches itself to a [class ManiaNoteController].

var controller : ManiaNoteController ## The parent controller.

func _ready() -> void:
	controller = get_parent()
	
	controller.NoteSpawned.connect(on_note_spawned)
	controller.NoteHit.connect(on_note_hit)
	controller.PressedPlayed.connect(on_pressed_animation)
	controller.NeutralPlayed.connect(on_neutral_animation)
	
func on_note_spawned(_note : Note) -> void: ## Invoked when a note spawns.
	pass
	
func on_note_hit(_result : NoteResult) -> void: ## Invoked when a note is hit. (A miss also counts.)
	pass
	
func on_pressed_animation() -> void: ## Invoked when the pressed animation is played.
	pass
	
func on_neutral_animation() -> void: ## Invoked when the neutral animation is played.
	pass