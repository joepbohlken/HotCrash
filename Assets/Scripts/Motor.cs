using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Ackermann))]
public class Motor : MonoBehaviour
{
	[Tooltip("The maximum torque that the motor generates")]
	public float maxTorque = 20000;

	[Tooltip("Multiplier to the maxTorque")]
	public float value;

	public Wheel m_FrontRight;
	public Wheel m_FrontLeft;
	public Wheel m_RearRight;
	public Wheel m_RearLeft;

	public Ackermann m_Ackermann { get; private set; }

	private void Awake()
	{
		m_Ackermann = GetComponent<Ackermann>();
	}

	private void Update()
	{
		ApplyMotorTorque();
	}

	void ApplyMotorTorque()
	{
		float fr, fl, rr, rl;

		// If we have Ackerman steering, we apply torque based on the steering radius of each wheel
		var radii = m_Ackermann.GetRadii();
		var total = radii[0, 0] + radii[1, 0] + radii[0, 1] + radii[1, 1];
		fl = radii[0, 0] / total;
		fr = radii[1, 0] / total;
		rl = radii[0, 1] / total;
		rr = radii[1, 1] / total;

		m_FrontLeft.sharedData.motorTorque = value * maxTorque * fl;
		m_FrontRight.sharedData.motorTorque = value * maxTorque * fr;

		m_RearLeft.sharedData.motorTorque = value * maxTorque * rl;
		m_RearRight.sharedData.motorTorque = value * maxTorque * rr;
	}
}
