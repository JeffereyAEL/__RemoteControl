using Godot;

public partial class connection_manager : PanelContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		switch (OS.GetName()) {
			case "Windows":
				break;
			case "Android":
				break;
			default:
				break;

		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		
	}
}
