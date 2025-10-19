using Godot;
using System;

public partial class StickingEcho : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body.IsInGroup("StickySurfaces"))
		{

			Monitoring = false; // Stop detecting further collisions
			GetParent().RemoveChild(this);
			body.AddChild(this);

			// stop movement
			GlobalPosition = body.ToLocal(GlobalPosition);

			// Optionally, you can stop any movement or effects here
			GetNode<GpuParticles2D>("EchoParticles").Emitting = false;
		}
	}

	public Action<StickingEcho> RecycleEcho { get; set; }

	public void StartRecycleTimer(float delay)
	{
		var timer = new Timer();
		timer.WaitTime = delay;
		timer.OneShot = true;
		AddChild(timer);
		timer.Start();
		timer.Timeout += () =>
		{
			RecycleEcho?.Invoke(this);
			timer.QueueFree();
		};
	}
}
