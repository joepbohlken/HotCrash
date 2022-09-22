using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Motor))]
[RequireComponent(typeof(Steering))]
[RequireComponent(typeof(Brakes))]
public class Vehicle : MonoBehaviour
{
	[SerializeField] Rigidbody m_Rigidbody;
	[SerializeField] float m_MaxReverseInput = -.5f;

	[Tooltip("The maximum motor torque available based on the speed (KMPH)")]
	[SerializeField] AnimationCurve m_MotorTorqueVsSpeed = AnimationCurve.Linear(0, 10000, 250, 0);

	[Tooltip("The steering angle based on the speed (KMPH)")]
	[SerializeField] AnimationCurve m_MaxSteeringAngleVsSpeed = AnimationCurve.Linear(0, 35, 250, 5);

	[Tooltip("The down force based on the speed (KMPH)")]
	[SerializeField] AnimationCurve m_DownForceVsSpeed = AnimationCurve.Linear(0, 0, 250, 2500);

	Steering m_Steering;
	Motor m_Motor;
	Brakes m_Brake;
	Aerodynamics m_Aerodynamics;
	Player m_Player;

	void Start()
	{
		m_Steering = GetComponent<Steering>();
		m_Motor = GetComponent<Motor>();
		m_Aerodynamics = GetComponent<Aerodynamics>();
		m_Brake = GetComponent<Brakes>();

		SetPlayer(new KeyboardPlayer());
	}

	public void SetPlayer(Player player)
	{
		m_Player = player;
	}

	void Update()
	{
		var speed = Vector3.Dot(m_Rigidbody.velocity, transform.forward) * 3.6F;

		m_Steering.range = m_MaxSteeringAngleVsSpeed.Evaluate(speed);
		m_Motor.maxTorque = m_MotorTorqueVsSpeed.Evaluate(speed);
		m_Aerodynamics.downForce = m_DownForceVsSpeed.Evaluate(speed);

		var input = m_Player.GetInput();

		if (input != null)
		{
			m_Steering.value = input.steering;
			m_Motor.value = Mathf.Clamp(input.acceleration, m_MaxReverseInput, 1);
			m_Aerodynamics.midAirSteerInput = input.steering;
			m_Brake.value = input.brake;
		}
	}
}
