using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiRoll : MonoBehaviour
{
	[Serializable]
	public class Axle
	{
		public Wheel left;
		public Wheel right;
		public float force;
	}

	public new Rigidbody rigidbody;
	public List<Axle> axles;

	void FixedUpdate()
	{
		foreach (var axle in axles)
		{
			var wsDown = transform.TransformDirection(Vector3.down);
			wsDown.Normalize();

			float travelL = Mathf.Clamp01(axle.left.compressionRatio);
			float travelR = Mathf.Clamp01(axle.right.compressionRatio);
			float antiRollForce = (travelL - travelR) * axle.force;

			WheelHit hit;
			if (axle.left.isGrounded)
				rigidbody.AddForceAtPosition(wsDown * -antiRollForce, axle.left.hit.point);


			if (axle.right.isGrounded)
				rigidbody.AddForceAtPosition(wsDown * antiRollForce, axle.right.hit.point);
		}
	}
}
