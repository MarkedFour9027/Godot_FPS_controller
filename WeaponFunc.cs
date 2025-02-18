using Godot;
using System;

public partial class WeaponFunc : RayCast3D
{
	[Export] private float MagLimit = 30f;
	[Export] private float AmmoCap = 180f;
	[Export] private float CurrentAmmo;
	[Export] private float usedBullet;
	private float currentMag;
	private float firingTime = 0f;

	[Export] private bool oneChamber = true;
	[Export] private bool Reload = false;
	[Export] private float rateOfFire = 10f;
	[Export] private float timeToReload = 1f;
	[Export] private float timeToLoadPartial = 1f;
	private float _animBlend;

	[Export]private AnimationPlayer _playerWpn;
	private AnimationTree _playerWpnAnimTree;
	private AnimationNodeStateMachinePlayback _stateMachine;
	[Export] public Node3D recoilHandler;
	private TextEdit ammoCounter;
	//[Export] private Script recoilScript;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_playerWpn = GetNode<AnimationPlayer>("Holster/Rot/MP5/AnimationPlayer");
		_playerWpnAnimTree = GetNode<AnimationTree>("Holster/Rot/MP5/AnimationTree");
		_stateMachine = (AnimationNodeStateMachinePlayback)_playerWpnAnimTree.Get("parameters/playback");
		ammoCounter = GetNode<TextEdit>("Ammo");
		currentMag = MagLimit;
		CurrentAmmo = AmmoCap;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		ammoCounter.Text = currentMag.ToString() + " / " + CurrentAmmo.ToString() + " Debug " + usedBullet.ToString();
		
		if(Reload)
		{
			return;
		}

		if(Input.IsActionPressed("attack") && Time.GetTicksMsec() >= firingTime)
		{
			Fire();
			GD.Print(currentMag + " Bullet(s) left, then " + usedBullet + " being used");
		}
		if(Input.IsActionJustPressed("reload") && CurrentAmmo > 0 && currentMag < MagLimit + 1 )
		{
			if(oneChamber)
			{
				float insideChamber;
				insideChamber = usedBullet + 1;
				usedBullet = insideChamber;
				insideChamber = 0;
			}
			DoMidReload();
		}
	}

	void Fire()
	{
		if(currentMag > 0)
		{
			firingTime = Time.GetTicksMsec() + 1000f / rateOfFire;
			currentMag--;
			usedBullet++;
			_playerWpnAnimTree.Set("parameters/Firing/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
			recoilHandler.Call("MakeRecoil");
			
		}
		else if(CurrentAmmo > 0)
		{
			DoReload();
		}
	}

	private async void DoReload()
	{
		if(usedBullet >= 31)
		{
			usedBullet = 30;
		}
		Reload = true;
		await ToSignal(GetTree().CreateTimer(.25f), "timeout");
		_playerWpnAnimTree.Set("parameters/Empty Reload/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		_playerWpnAnimTree.Set("parameters/Fire/request", (int)AnimationNodeOneShot.OneShotRequest.Abort);
		await ToSignal(GetTree().CreateTimer(timeToReload), "timeout");
		float AmmoToUse;
		AmmoToUse = Math.Min(usedBullet, CurrentAmmo);
		CurrentAmmo -= AmmoToUse;
		currentMag += AmmoToUse;
		Reload = false;
		usedBullet = 0;
	}
	private async void DoMidReload()
	{
		Reload = true;
		await ToSignal(GetTree().CreateTimer(.25f), "timeout");
		_playerWpnAnimTree.Set("parameters/Partial Reload/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		_playerWpnAnimTree.Set("parameters/Fire/request", (int)AnimationNodeOneShot.OneShotRequest.Fire);
		await ToSignal(GetTree().CreateTimer(timeToLoadPartial), "timeout");
		float AmmoToUse;
		AmmoToUse = Math.Min(usedBullet, CurrentAmmo);
		CurrentAmmo -= AmmoToUse;
		currentMag += AmmoToUse;
		Reload = false;
		usedBullet = -1;
	}
}
