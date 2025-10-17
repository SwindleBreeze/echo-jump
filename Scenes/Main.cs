using Godot;
using System;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        var player = GetNode<PlayerBody>("Player/PlayerBody");
        player.JumpSignal += OnPlayerJump;
    }

    private void OnPlayerJump(Vector2 position)
    {
        EmitEcho(position);
    }

    async private void EmitEcho(Vector2 position)
    {
        PackedScene echoScene = GD.Load<PackedScene>("res://Echo/Echo.tscn");
        var echoInstance = echoScene.Instantiate<Node2D>();
        echoInstance.GlobalPosition = position;
        AddChild(echoInstance);
        // Emit particle effect from echoInstance
        echoInstance.GetNode<GpuParticles2D>("EchoParticles").Emitting = true;
        // await for the particles to finish before removing the echoInstance
        await ToSignal(GetTree().CreateTimer(echoInstance.GetNode<GpuParticles2D>("EchoParticles").Lifetime), "timeout");
        echoInstance.QueueFree();
    }
}
