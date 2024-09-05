using Godot;
using System;

// TODO: impliment hardware acceleration hooking for more flexible graphics capture
public partial class HardwareAccCapture : GraphicsCapture
{
    public HardwareAccCapture()
    {
        // TODO: Initialize childclass properties here
    }
    protected override void _Factory()
    {
        throw new NotImplementedException();
    }
	public static HardwareAccCapture Factory(string winTitle)
	{
		return Factory<HardwareAccCapture>(winTitle);
	}
    public override ImageTexture CaptureWindow()
    {
        throw new NotImplementedException();
    }

    public override void OnWindowSizeChanged()
    {
        throw new NotImplementedException();
    }

}
