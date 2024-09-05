using Godot;

public abstract partial class GraphicsCapture : GodotObject
{
    protected string WinTitle;
    protected int WinWidth;
    protected int WinHeight;
    protected GraphicsCapture()
    {
        WinTitle = null;
        WinWidth = 0;
        WinHeight = 0;
    }
    protected abstract void _Factory();
    protected static T Factory<T>(string winTitle) where T : GraphicsCapture, new() 
    {
        T graphics_capture = new(){WinTitle = winTitle};
        graphics_capture._Factory();
        return graphics_capture;
    }
	public bool IsValid()
	{
		return WinTitle != null;
	}

    public abstract ImageTexture CaptureWindow();

    public abstract void OnWindowSizeChanged();
}