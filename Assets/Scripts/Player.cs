using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Player
{
	protected VehicleInput p_Input;
	public abstract VehicleInput GetInput();
}

public class KeyboardPlayer : Player
{
	public const string k_SteeringAxisName = "Horizontal";
	public const string k_AccelerateAxisName = "Vertical";
	public const string k_BrakeAxisName = "Jump";

	public KeyboardPlayer()
	{
		p_Input = new VehicleInput();
	}

	public override VehicleInput GetInput()
	{
		p_Input.acceleration = Input.GetAxis(k_AccelerateAxisName);
		p_Input.steering = Input.GetAxis(k_SteeringAxisName);
		p_Input.brake = Input.GetAxis(k_BrakeAxisName);
		return p_Input;
	}
}
