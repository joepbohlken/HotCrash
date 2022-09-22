using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CenterOfMassAssigner : MonoBehaviour
{
	public Transform m_Point;

	Rigidbody m_Rigidbody;

	void Start()
	{
		m_Rigidbody = GetComponent<Rigidbody>();
		if (m_Rigidbody == null) return;
		m_Rigidbody.centerOfMass = m_Point.localPosition;
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;

		if (m_Point != null)
			Gizmos.DrawSphere(m_Point.position, .1f);
	}
#endif
}
