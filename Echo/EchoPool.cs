using Godot;
using System;
using System.Collections.Generic;


public partial class EchoPool : Node2D
{
    [Export] public int PoolSize = 50; // Set this to a reasonable limit
    [Export] public PackedScene StickingEchoScene; // Set in editor
    [Export] public PackedScene VisualEchoScene;   // Set in editor

    private readonly Queue<StickingEcho> _availableEchoes = new();

    public override void _Ready()
    {
        // 1. Pre-instantiate and pool the sticking particles
        for (int i = 0; i < PoolSize; i++)
        {
            StickingEcho echo = StickingEchoScene.Instantiate<StickingEcho>();
            AddChild(echo);
            echo.RecycleEcho = ReturnToPool; // Set the callback action

            // Initialize as dormant
            echo.Monitoring = false;
            echo.Visible = false;
            _availableEchoes.Enqueue(echo);
        }

        // 2. Connect the player's jump signal
        // Assume the player is a child named "Player"
        var player = GetNode<Player>("/root/Level/Player");
        player.JumpSignal += OnPlayerJumped;
    }

    private void OnPlayerJumped(Vector2 position)
    {
        // A. Trigger Visual Echo (GPU, fast)
        var visualEcho = VisualEchoScene.Instantiate<GPUParticles2D>();
        visualEcho.GlobalPosition = position;
        AddChild(visualEcho);
        visualEcho.Emitting = true;
        // Free the visual effect once it's finished
        visualEcho.Finished += visualEcho.QueueFree;

        // B. Dispatch Sticking Particles (CPU, pooled)
        DispatchStickingParticles(position);
    }

    private void DispatchStickingParticles(Vector2 startPos)
    {
        // Simple approach: Dispatch N particles in a circle
        int particlesToDispatch = 5; // A small number for each jump

        for (int i = 0; i < particlesToDispatch; i++)
        {
            if (_availableEchoes.TryDequeue(out StickingEcho echo))
            {
                // 1. Activate the particle
                echo.GlobalPosition = startPos;
                echo.Visible = true;
                echo.Monitoring = true;

                // 2. Set its direction and velocity
                float angle = (float)GD.RandRange(0, 360);
                Vector2 direction = Vector2.Right.Rotated(Mathf.DegToRad(angle));
                echo.LinearVelocity = direction * 500; // Give it a push outwards
            }
        }
    }

    private void ReturnToPool(StickingEcho echo)
    {
        // Reset and disable the particle
        echo.Monitoring = false;
        echo.Visible = false;
        // Detach from current parent (e.g., a platform) and put back under the manager
        if (echo.GetParent() != this)
        {
            echo.GetParent().RemoveChild(echo);
            AddChild(echo);
        }
        // Put it back in the queue
        _availableEchoes.Enqueue(echo);
    }
}