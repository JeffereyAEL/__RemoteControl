using Godot;
using System;

public partial class InputFetcher : TextureRect
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public override void _Input(InputEvent e)
    {
        // TODO: read mouse input events, convert them to source-relative space and simulate-input into LDPlayer via Windows
    }
}
