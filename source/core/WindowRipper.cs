using Godot;

public partial class WindowRipper : Node
{
	private WindowRippingWindowsExtensions Ripper;
	private double ElapsedTime;
	[Export] private const double FrameRate = 1.0 / 30.0;
	private MeshInstance3D Quad;
	private Camera3D Camera;
	private CompressedTexture2D DebugImage;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("In WindowRipper._Ready()");

		Ripper = new WindowRippingWindowsExtensions("__slime_castle__");
		Quad = GetNode<MeshInstance3D>("/root/demoWindowRipper/windowRip");
		Camera = GetNode<Camera3D>("/root/demoWindowRipper/mainCamera");
		Camera.MakeCurrent();
		DebugImage = GD.Load<CompressedTexture2D>("res://assets/debug/Hell.png");
		ElapsedTime = 0.0;

		// debug
		SetProcess(false);
		SetTexture();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
		ElapsedTime += delta;
		if (ElapsedTime >= FrameRate)
		{
			// SetTexture();

			ElapsedTime -= FrameRate;
		}
	}

	public void SetTexture()
	{
		var mat = (StandardMaterial3D)Quad.MaterialOverride;
		var albedo_tex = (ImageTexture)mat.AlbedoTexture;
		GD.Print($"Albedo size: {albedo_tex.GetSize()}");
		
		albedo_tex.SetImage(Ripper.CaptureWindow().GetImage());
		if (albedo_tex != null)
		{
			mat.AlbedoTexture = albedo_tex;
			Quad.MaterialOverride = mat;
		}
		else
		{
			mat.AlbedoTexture = DebugImage;
			Quad.MaterialOverride = mat;
		}
	}
}


/*
	window size: 762, 1315
	Albedo size: (0, 0)
	Format: Format32bppRgb
	stride = -3048 bytes
	Current Size = -4008120
*/