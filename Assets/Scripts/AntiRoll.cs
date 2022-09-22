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

	private bool activated = false;

    private void Start()
    {
		StartCoroutine(StartActivation());
    }

    void FixedUpdate()
	{
		if (!activated) return;

		foreach (var axle in axles)
		{
			var wsDown = transform.TransformDirection(Vector3.down);
			wsDown.Normalize();

			float travelL = Mathf.Clamp01(axle.left.CompressionRatio);
			float travelR = Mathf.Clamp01(axle.right.CompressionRatio);
			float antiRollForce = (travelL - travelR) * axle.force;

			WheelHit hit;
			if (axle.left.sharedData.isGrounded)
				rigidbody.AddForceAtPosition(wsDown * -antiRollForce, axle.left.active_hit_data.point);


			if (axle.right.sharedData.isGrounded)
				rigidbody.AddForceAtPosition(wsDown * antiRollForce, axle.right.active_hit_data.point);
		}
	}

	IEnumerator StartActivation()
    {
		yield return new WaitForSeconds(2f);
		activated = true;
    }
}
