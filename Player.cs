using Godot;
using System;
//using System.Numerics;

public partial class Player : CharacterBody3D
{
	[ExportGroup("Player Settings")]
	[Export] public float Speed = 6.0f;
	[Export] public float SprintMultiplier = 1.5f;
	[Export] public float Sensitivity = 0.01f; // Sensitivity for rotation
	[Export] public float VerticalSensitivity = 0.01f; // Sensitivity for vertical rotation
	[Export] public float JumpForce = 5.0f;
	[Export] public float Gravity = -24.8f;
	[ExportGroup("Walk Bobbing Settings")]
	[Export] public float WalkAmplifier = 40.0f;
	[Export] public float WalkFrequency = 80.0f;
	[Export] public float WalkRotAmplifier = 40.0f;
	[Export] public float WalkRotFrequency = 80.0f;
	[ExportGroup("Sprint Bobbing Settings")]
    [Export] public float SprintAmplifier = 60.0f;
    [Export] public float SprintFrequency = 100.0f;
	[Export] public float SprintRotAmplifier = 20.0f;
	[Export] public float SprintRotFrequency = 100.0f;
	[Export] public float cameraSprintAmp = 5f;
	[Export] public float cameraSprintFreq = 10f;
	[ExportGroup("Z Position Multipliers")]
	[Export] public float WalkPosZMultiplier = 2f;
	[Export] public float SprintPosZMultiplier = 1.7f;

	[ExportGroup("Sprint Weapon Position")]
	[Export] public float SprintPos_X;
	[Export] public float SprintPos_Y;
	[Export] public float SprintPos_Z;

	[Export] public float SprintRot_X = -10f;
    [Export] public float SprintRot_Y = 0f;
    [Export] public float SprintRot_Z = 5f;
	[ExportGroup("Idle Bobbing Settings")]
	[Export] public float BobbingAmplitude = 0.1f;
	[Export] public float BobbingFrequency = 0.18f;

	private Godot.Vector3 _velocity = Godot.Vector3.Zero;
	private float _cameraTilt = 0.0f; // Used for up-and-down camera movement
	private float _weaponBob = 0.0f; // Used for weapon bobbing
	private float _weaponRotBob;
	private float _cameraBob;
	private float _cameraRotBob;
	private float _currentAmplitude;
	private float _currentFrequency;
	private float _currentRotAmplitude;
	private float _currentRotFrequency;
	private float _posZMultiplier;
	private float _camSprintAmp;
	private Camera3D _camera;
	private Node3D _weapons;
	private Node3D _weaponsRotH;
	private TextEdit _textDebug;
	private Godot.Vector3 wpnPos;
	private Godot.Vector3 wpnRot;

	public override void _Ready()
	{
		_currentAmplitude = WalkAmplifier;
		_currentFrequency = WalkFrequency;
		_posZMultiplier = WalkPosZMultiplier;
		_currentRotAmplitude = WalkRotAmplifier;
		_currentRotFrequency = WalkRotFrequency;
		_camSprintAmp = 0f;
		// Capture the mouse
		Input.MouseMode = Input.MouseModeEnum.Captured;

		// Get the camera node
		_camera = GetNode<Camera3D>("RecoilHandler/Camera3D");
		_weapons = GetNode<Node3D>("RecoilHandler/Camera3D/RayCast3D/Holster");
		_weaponsRotH = GetNode<Node3D>("RecoilHandler/Camera3D/RayCast3D/Holster/Rot");
		_textDebug = GetNode<TextEdit>("RecoilHandler/Camera3D/TextDebug");

		wpnPos = _weapons.Position;
		wpnRot = _weaponsRotH.RotationDegrees;

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
		BobSprint(delta);
		if (Input.IsActionPressed("move_forward"))
		{
			
			_cameraBob += BobbingFrequency * (float)delta;
			_cameraRotBob += cameraSprintFreq * (float)delta;
			float cameraBobOffset = Mathf.Sin(_cameraBob) * BobbingAmplitude;
			float cameraRotOffset = Mathf.Cos(_cameraRotBob) * _camSprintAmp;

			Godot.Vector3 camPos = _camera.Position;
			Godot.Vector3 camRot = _camera.RotationDegrees;
			camPos.Y = Mathf.Lerp(camPos.Y, cameraBobOffset, 0.1f);
			camRot.Z = Mathf.Lerp(camRot.Z, cameraRotOffset, 0.1f);
			_camera.RotationDegrees = camRot;
			_camera.Position = camPos;

			_weaponBob += _currentFrequency * (float)delta;
			_weaponRotBob += _currentRotFrequency * (float)delta;
			float weaponBobOffset = Mathf.Sin(_weaponBob) * _currentAmplitude;
			float weaponZBobOffset = Mathf.Cos(_weaponBob * _posZMultiplier) * _currentAmplitude;
			float weaponRotOffset = Mathf.Cos(_weaponRotBob) * _currentRotAmplitude;

			wpnPos.X = Mathf.Lerp(wpnPos.X, weaponBobOffset, 0.1f);
			wpnPos.Z = Mathf.Lerp(wpnPos.Z, -weaponZBobOffset , 0.1f);
			wpnPos.Y = Mathf.Lerp(wpnPos.Y, -.01f, 0.1f);
			wpnRot.Z = Mathf.Lerp(wpnRot.Z, weaponRotOffset, 0.1f);
			//wpnPos.Z = weaponBobOffset;
			_weapons.Position = wpnPos;
			_weapons.Rotation = wpnRot;
			//GD.Print("Rotation " + wpnRot.Z + " And Bob Value " + weaponRotOffset);
		}
		else if(Input.IsActionPressed("move_backward"))
		{
			_weaponBob += _currentFrequency * (float)delta;
			float weaponBobOffset = Mathf.Sin(_weaponBob) * -_currentAmplitude;
			float weaponZBobOffset = Mathf.Cos(_weaponBob * _posZMultiplier) * _currentAmplitude;
			float weaponRotOffset = Mathf.Cos(_weaponRotBob) * -_currentRotAmplitude;
			wpnPos.X = Mathf.Lerp(wpnPos.X, weaponBobOffset, 0.1f);
			wpnPos.Z = Mathf.Lerp(wpnPos.Z, -weaponZBobOffset * 1.5f, 0.1f);
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
			wpnPos.X = Mathf.Lerp(wpnPos.X, 0.0f, 0.1f);
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
	private void BobSprint(double delta)
	{
		{
			
			Vector3 rot = _weaponsRotH.RotationDegrees;

        	Vector3 sprintTarget; Vector3 sprintPosTarget;
        	if (Input.IsActionPressed("sprint"))
        	{
        	    sprintTarget = new Vector3(SprintRot_X, SprintRot_Y, SprintRot_Z);
				sprintPosTarget = new Vector3(SprintPos_X, SprintPos_Y, SprintPos_Z);
				_currentAmplitude = SprintAmplifier;
				_currentFrequency = SprintFrequency;
				_currentRotAmplitude = SprintRotAmplifier;
				_currentRotFrequency = SprintRotFrequency;
				_posZMultiplier = SprintPosZMultiplier;
				_camSprintAmp = cameraSprintAmp;
        	}
        	else
        	{
        	    sprintTarget = Vector3.Zero;
				sprintPosTarget = Vector3.Zero;
				_currentAmplitude = WalkAmplifier;
				_currentFrequency = WalkFrequency;
				_currentRotAmplitude = WalkRotAmplifier;
				_currentRotFrequency = WalkRotFrequency;
				_posZMultiplier = WalkPosZMultiplier;
				_camSprintAmp = 0f;
        	}

        	float strafe = 0f;
        	if (Input.IsActionPressed("move_left")) strafe = 3f;
        	if (Input.IsActionPressed("move_right")) strafe = -3f;

        	sprintTarget.Z += strafe;
        	sprintTarget.Z += Mathf.Cos(_weaponRotBob) * SprintRotAmplifier;

        	rot = rot.Lerp(sprintTarget, 0.1f);
        	_weaponsRotH.RotationDegrees = rot;
			_weaponsRotH.Position = _weaponsRotH.Position.Lerp(sprintPosTarget, 0.1f);
		}
	}
}