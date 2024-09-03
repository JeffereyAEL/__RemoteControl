extends Node

var _RippingScript: CSharpScript  = preload("res://source/core/WindowRippingWindowsExtensions.cs");
# var _DebugImage: CompressedTexture2D = preload("res://assets/debug/hell.png");

var _Ripper: Object
var _ElapsedTime: float

@export
var FrameRate: float = 1.0 / 30.0

@onready
var _Quad: MeshInstance3D = $windowRip

# Called when the node enters the scene tree for the first time.
func _ready():
	print("In window_ripper._Ready()")
	
	_Ripper = _RippingScript.Factory("__slime_castle__")
	var camera: Camera3D = $mainCamera
	if not camera.current:
		camera.make_current()
	_ElapsedTime = 0.0

	# debug
	set_process(false)
	_set_texture()

# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	_ElapsedTime += delta
	if _ElapsedTime >= FrameRate:
		# _set_texture()

		_ElapsedTime -= FrameRate

func _set_texture():
	var mat: StandardMaterial3D = StandardMaterial3D.new()
	if mat.albedo_texture != null:
		print("albedo_tex size: ", mat.albedo_texture.get_size())


	mat.albedo_texture = _Ripper.CaptureWindow()
	if mat.albedo_texture != null:
		# debugging
		# mat.albedo_texture = _DebugImage
		print("albedo_tex new size: ", mat.albedo_texture.get_size())
		_Quad.material_override = mat


