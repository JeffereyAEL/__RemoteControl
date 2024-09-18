using Godot;
using Utils;

public partial class WindowRemote : TextureRect
{
	private bool[] _TouchEvents;
	private Rect2 _Rect;
	private EAspect _Aspect;
	private FontFile Roboto = GD.Load<FontFile>("assets/fonts/roboto/Roboto-Small.ttf"); // TODO: maybe figure out how to make this shit smaller
	private SlimeCastleMining _Mining;
	private TRectContext[] Draws; 
	public WindowRemote()
	{
		GD.Print("WindowRemote Constructor");
		_TouchEvents = new bool[1]{false};
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("InputFetcher _Ready()");
		_Rect = new(new(0,0), Size);
		_Aspect = Util.Aspect(Size);
		WindowReference.Instance.aspectChange += OnCaptureAspectChanged;
		WindowReference.Instance.sizeChange += OnCaptureSizeChanged;
		WindowReference.Instance.textureCapture += OnTextureCapture;
		_Mining = new SlimeCastleMining();
		Draws = System.Array.Empty<TRectContext>();
	}

	private Vector2 GlobalToRelative(Vector2 globalPos)
	{
		GD.Print($"Global Position: {globalPos}");
		Vector2 relative = globalPos - GlobalPosition - _Rect.Position;
		GD.Print($"Texture Position: {relative}");
		relative.X /= _Rect.Size.X;
		relative.Y /= _Rect.Size.Y;
		GD.Print($"RelSpace Position: {relative}");
		return relative;
	}

    public override void _Input(InputEvent e)
    {
		if (e is InputEventScreenTouch touch_event)
		{
			// TODO: modify this to account for our _Aspect
			if (touch_event.Index >= _TouchEvents.Length) return;
			if (!_Rect.HasPoint(touch_event.Position)) return;
			if (touch_event.IsPressed() && !_TouchEvents[touch_event.Index])
			{
				GD.Print("Touch Event Started");
				WindowReference.Instance.SendMouseDown(GlobalToRelative(touch_event.Position));
			}
			if (touch_event.IsReleased())
			{
				GD.Print("Touch Event Ended");
				WindowReference.Instance.SendMouseUp(GlobalToRelative(touch_event.Position));
				_TouchEvents[touch_event.Index] = false;
			}
		}
		else if (e is InputEventScreenDrag drag_event)
		{
			if (drag_event.Index >= _TouchEvents.Length) return;
			if (!_Rect.HasPoint(drag_event.Position)) return;
			else if (!_TouchEvents[drag_event.Index])
			{
				GD.Print("Touch Event Dragging");
				WindowReference.Instance.SendMouseMove(GlobalToRelative(drag_event.Position));
				_TouchEvents[drag_event.Index] = true;
			}
		}
    }

    // public override void _Process(double delta)
    // {
	// 	GD.Print($"---Sizing and Rotation---");
	// 	var parent = GetParent<Control>();
	// 	GD.Print($"Parent Size: {parent.Size}");
	// 	GD.Print($"Rect Size: {Size}");
	// 	GD.Print($"Rect Pos: {Position}");
	// 	GD.Print($"Rect GlobPos: {GlobalPosition}");
	// 	GD.Print($"Rect Offset: {PivotOffset}");
	// 	GD.Print($"Texture Size: {Texture.GetSize()}");
    // }

    public void OnCaptureAspectChanged(EAspect ascpet)
	{
		var parent = GetParent<Control>();
		
		if (ascpet is EAspect.Portrait)
		{
			RotationDegrees = 0f;
			Size = parent.Size;
			SetAnchorsPreset(LayoutPreset.TopLeft, true);
		}
		else  // ascpet is EAspect.Landscape or Square
		{
			RotationDegrees = 90f;
			Size = new Vector2(parent.Size.Y, parent.Size.X);
			SetAnchorsPreset(LayoutPreset.TopRight, true);
		}
		_Aspect = Util.Aspect(Size);

		GD.Print($"---Aspect Update---");
		GD.Print($"Parent Size: {GetParent<Control>().Size}");
		GD.Print($"Container GlobPos: {GlobalPosition}");
		GD.Print($"Container Size: {Size}");
	}

	public void OnCaptureSizeChanged(Vector2I intTexSize)
	{
		
		var tex_size = (Vector2)intTexSize;

		var rect_size = Size;
		var scale_mult = rect_size.Y / tex_size.Y;
		var scaled_tex_size = tex_size * scale_mult;
		var offset = (rect_size - scaled_tex_size) / 2f;
		// var global_pos = GlobalPosition + offset;

		_Rect = new Rect2(offset, scaled_tex_size);

		GD.Print($"---Size Update---");
		GD.Print($"Rect GlobPos: {_Rect.Position}");
		GD.Print($"Rect Size: {_Rect.Size}");
		GD.Print($"Texture Size: {intTexSize}");
	}

	static bool debug = false;
	public void OnTextureCapture(ImageTexture texture)
	{
		Texture = texture;
		if (!debug)
		{
			Draws = _Mining.DebugProcess(texture, (Rect2I)_Rect);
			debug = true;
		}
	}

    public override void _Draw()
    {
		// DrawRect(_Rect, Color.Color8(25, 255, 25, 255), false);
		foreach (var c in Draws)
		{
			DrawRect(c.Rect,  c.DrawColor,  c.DrawFill);
			for (int idx = 0; idx < c.Label.Length; ++idx) 
			{
				DrawString(Roboto, c.Rect.Position + new Vector2(2f, 15f * (1f+idx)), c.Label[idx]);
			}
		}

        base._Draw();
    }
}
