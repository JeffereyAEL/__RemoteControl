using System;
using Godot;

public partial class WindowCapturer : Node
{
	private Control RipContainer;
	private GraphicsCapture Ripper;
	private double ElapsedTime;
	private TextureRect RipDest;
	private Camera2D Camera;
	private CompressedTexture2D DebugImage;
	[Export] private const double FrameRate = 1.0 / 30.0;
	[Export] public string WindowToRipTitle = "__slime_castle__";
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("In WindowRipper._Ready()");

		Ripper = GDICapture.Factory(WindowToRipTitle);

		RipContainer = GetNode<Control>("winRipContainer");
		RipDest = GetNode<TextureRect>("winRipContainer/winRipDest");
		var vp_size = GetViewport().GetWindow().Size;
		RipContainer.SetSize(vp_size);
		GetViewport().GetWindow().SizeChanged += this.OnViewportSizeChanged;

		ElapsedTime = 0.0;
	}

	public void OnViewportSizeChanged()
	{
		RipContainer.SetSize(GetViewport().GetWindow().Size);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{	
		ElapsedTime += delta;
		if (ElapsedTime >= FrameRate)
		{
			SetTexture();

			ElapsedTime -= FrameRate;
		}
	}

	public void SetTexture()
	{
		var texture = Ripper.CaptureWindow();
		if (texture == null) 
			throw new Exception("Capture Window didn't return a valid texture");

		RipDest.Texture = texture;
	}
}