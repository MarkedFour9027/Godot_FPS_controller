using Godot;
using System;
using System.Numerics;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed = 6.0f;
    [Export] public float Sensitivity = 0.01f; // Sensitivity for rotation
    [Export] public float VerticalSensitivity = 0.01f; // Sensitivity for vertical rotation
    [Export] public float JumpForce = 5.0f;
    [Export] public float Gravity = -24.8f;
    [Export] public float WalkAmplifier = 40.0f;
    [Export] public float WalkFrequency = 80.0f;
    [Export] public float WalkRotAmplifier = 40.0f;
    [Export] public float WalkRotFrequency = 80.0f;

    [Export] public float BobbingAmplitude = 0.1f;
    [Export] public float BobbingFrequency = 0.18f;

    private Godot.Vector3 _velocity = Godot.Vector3.Zero;
    private float _cameraTilt = 0.0f; // Used for up-and-down camera movement
    private float _weaponBob = 0.0f; // Used for weapon bobbing
    private float _weaponRotBob;
    private float _cameraBob;
    private Camera3D _camera;
    private Node3D _weapons;
    private Node3D _weaponsRotH;
    private TextEdit _textDebug;
    private Godot.Vector3 wpnPos;
    private Godot.Vector3 wpnRot;

    public override void _Ready()
    {
        // Capture the mouse
        Input.MouseMode = Input.MouseModeEnum.Captured;

        // Get the camera node
        _camera = GetNode<Camera3D>("RecoilHandler/Camera3D");
        _weapons = GetNode<Node3D>("RecoilHandler/Camera3D/RayCast3D/Holster");
        _weaponsRotH = GetNode<Node3D>("RecoilHandler/Camera3D/RayCast3D/Holster/Rot");
        _textDebug = GetNode<TextEdit>("RecoilHandler/Camera3D/TextDebug");

        wpnPos = _weapons.Position;
        wpnRot = _weapons.Rotation;

    }

    public override void _Input(InputEvent @event)
    {
        // Handle mouse input for looking around
        if (@event is InputEventMouseMotion mouseEvent)
        {
            // Horizontal rotation (left/right) with Sensitivity
            RotateY(-mouseEvent.Relative.X * Sensitivity);

            // Vertical rotation (up/down) with VerticalSensitivity
            _cameraTilt -= mouseEvent.Relative.Y * VerticalSensitivity;

            // Clamp the camera tilt to prevent flipping
            _cameraTilt = Mathf.Clamp(_cameraTilt, -90.0f, 90.0f);

            // Apply the tilt to the camera
            _camera.RotationDegrees = new Godot.Vector3(_cameraTilt, _camera.RotationDegrees.Y, _camera.RotationDegrees.Z);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        // Handle movement input
        Godot.Vector3 direction = Godot.Vector3.Zero;

        if (Input.IsActionPressed("move_forward"))
            direction -= Transform.Basis.Z;
        if (Input.IsActionPressed("move_backward"))
            direction += Transform.Basis.Z;
        if (Input.IsActionPressed("move_left"))
            direction -= Transform.Basis.X;
        if (Input.IsActionPressed("move_right"))
            direction += Transform.Basis.X;

        direction = direction.Normalized();

        // Apply movement velocity
        _velocity.X = direction.X * Speed;
        _velocity.Z = direction.Z * Speed;

        // Apply gravity
        if (!IsOnFloor())
        {
            _velocity.Y += Gravity * (float)delta;
        }
        else if (Input.IsActionJustPressed("jump"))
        {
            _velocity.Y = JumpForce;
        }

        // Move the character
        Velocity = _velocity;
        MoveAndSlide();
        //GD.Print(_weapons.Rotation);
        _textDebug.Text = Engine.GetFramesPerSecond().ToString();
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("move_forward"))
        {
            _cameraBob += BobbingFrequency * (float)delta;
            float cameraBobOffset = Mathf.Sin(_cameraBob) * BobbingAmplitude;

            Godot.Vector3 camPos = _camera.Position;
            camPos.Y = Mathf.Lerp(camPos.Y, cameraBobOffset, 0.1f);
            _camera.Position = camPos;

            _weaponBob += WalkFrequency * (float)delta;
            _weaponRotBob += WalkRotFrequency * (float)delta;
            float weaponBobOffset = Mathf.Sin(_weaponBob) * WalkAmplifier;
            float weaponRotOffset = Mathf.Cos(_weaponRotBob) * WalkRotAmplifier;

            wpnPos.Z = Mathf.Lerp(wpnPos.Z, weaponBobOffset, 0.1f);
            wpnPos.Y = Mathf.Lerp(wpnPos.Y, -.01f, 0.1f);
            wpnRot.Z = Mathf.Lerp(wpnRot.Z, weaponRotOffset, 0.1f);
            //wpnPos.Z = weaponBobOffset;
            _weapons.Position = wpnPos;
            _weapons.Rotation = wpnRot;
            //GD.Print("Rotation " + wpnRot.Z + " And Bob Value " + weaponRotOffset);
        }
        else if(Input.IsActionPressed("move_backward"))
        {
            _weaponBob += WalkFrequency * (float)delta;
            float weaponBobOffset = Mathf.Sin(_weaponBob) * -WalkAmplifier;
            float weaponRotOffset = Mathf.Cos(_weaponRotBob) * -WalkRotAmplifier;
            wpnPos.Z = Mathf.Lerp(wpnPos.Z, weaponBobOffset, 0.1f);
            wpnPos.Y = Mathf.Lerp(wpnPos.Y, .01f, 0.1f);
            wpnRot.Z = Mathf.Lerp(wpnRot.Z, weaponRotOffset, 0.1f);
            //wpnPos.Z = weaponBobOffset;
            _weapons.Position = wpnPos;
            _weapons.Rotation = wpnRot;
        }
        else
        {
            _cameraBob = 0.0f;
            Godot.Vector3 camPos = _camera.Position;
            camPos.Y = Mathf.Lerp(camPos.Y, 0.0f, 0.1f);
            _camera.Position = camPos;

            _weaponBob = 0.0f;
            _weaponRotBob = 0.0f;
            Godot.Vector3 lWpnRot = _weaponsRotH.Rotation;
            wpnPos.Z = Mathf.Lerp(wpnPos.Z, 0.0f, 0.1f);
            wpnPos.Y = Mathf.Lerp(wpnPos.Y, 0f, 0.1f);
            wpnRot.Z = Mathf.Lerp(wpnRot.Z, 0.0f, 0.1f);
            _weapons.Position = wpnPos;
            _weapons.Rotation = wpnRot;
            
        }
        if(Input.IsActionPressed("move_left"))
        {
            //_weaponBob += WalkFrequency * (float)delta;
            //float weaponBobOffset = Mathf.Sin(_weaponBob) * WalkAmplifier;

            Godot.Vector3 lWpnRot = _weaponsRotH.Rotation;
            //wpnPos.Z = Mathf.Lerp(wpnPos.Z, weaponBobOffset, 0.1f);
            lWpnRot.Y = Mathf.Lerp(lWpnRot.Y, .05f, 0.1f);
            //_weapons.Position = wpnPos;
            _weaponsRotH.Rotation = lWpnRot;
        }
        else if(Input.IsActionPressed("move_right"))
        {
            //_weaponBob += WalkFrequency * (float)delta;
            //float weaponBobOffset = Mathf.Sin(_weaponBob) * WalkAmplifier;

            Godot.Vector3 lWpnRot = _weaponsRotH.Rotation;
            //wpnPos.Z = Mathf.Lerp(wpnPos.Z, weaponBobOffset, 0.1f);
            lWpnRot.Y = Mathf.Lerp(lWpnRot.Y, -.05f, 0.1f);
            //_weapons.Position = wpnPos;
            _weaponsRotH.Rotation = lWpnRot;
        }
        else
        {
            Godot.Vector3 lWpnRot = _weaponsRotH.Rotation;
            lWpnRot.Y = Mathf.Lerp(lWpnRot.Y, 0.0f, 0.1f);
            _weaponsRotH.Rotation = lWpnRot;
        }
    }
}
