using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct Wheel
{
	public WheelCollider wheelCollider;
	public Transform wheelView;

	public bool isGrounded { get; private set; }

	public void UpdateVisual()
	{
		isGrounded = wheelCollider.isGrounded;

		UpdateTransform();
	}

	public void UpdateTransform()
	{
		Vector3 pos;
		Quaternion quat;
		wheelCollider.GetWorldPose(out pos, out quat);
		wheelView.position = pos;
		wheelView.rotation = quat;
	}
}