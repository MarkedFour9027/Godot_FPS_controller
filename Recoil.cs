using Godot;
using System;

public partial class Recoil : Node3D
{

    private Vector3 _currentRot = Vector3.Zero;
    private Vector3 _targetRot = Vector3.Zero;

    [Export] private float _recoilX;
    [Export] private float _recoilY;
    [Export] private float _recoilZ;

    [Export] private float _snap;
    [Export] private float _returnSpeed;

	public TextEdit _textDebug;

	public override void _Ready()
	{
		_textDebug = GetNode<TextEdit>("Camera3D/TextDebug");
	}

    public override void _Process(double delta)
    {

        _targetRot.X = Mathf.Lerp(_targetRot.X, 0, _returnSpeed * (float)delta);
        _targetRot.Y = Mathf.Lerp(_targetRot.Y, 0, _returnSpeed * (float)delta);
        _targetRot.Z = Mathf.Lerp(_targetRot.Z, 0, _returnSpeed * (float)delta);

        _currentRot.X = Mathf.Lerp(_currentRot.X, _targetRot.X, _snap * (float)delta);
        _currentRot.Y = Mathf.Lerp(_currentRot.Y, _targetRot.Y, _snap * (float)delta);
        _currentRot.Z = Mathf.Lerp(_currentRot.Z, _targetRot.Z, _snap * (float)delta);

        RotationDegrees = _currentRot;
        //_textDebug.Text = _currentRot.ToString();
    }

    public void MakeRecoil()
    {
        _targetRot += new Vector3((float)GD.RandRange(0, _recoilX), (float)GD.RandRange(-_recoilY, _recoilY),(float)GD.RandRange(-_recoilZ, _recoilZ));
    }
}
