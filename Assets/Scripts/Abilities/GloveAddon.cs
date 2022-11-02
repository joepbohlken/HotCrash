using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GloveAddon : MonoBehaviour
{
    public int knockback;
    public float armingTime = 0.1f;
    public Transform target;
    public Collider col;

    private void Start()
    {
    }

    private void Update()
    {
        //Time
        armingTime -= Time.deltaTime;

        //Correct position
        Vector3 correctedPos = target.position;
        correctedPos.y += 0.35f;

        Vector3 targetDirection = (target.position - transform.position);
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, 360, 0.0f);
        newDirection.z += 90;
        transform.rotation = Quaternion.LookRotation(newDirection);
        transform.position = Vector3.MoveTowards(transform.position, correctedPos, Time.deltaTime * 60);
    }

    private void OnTriggerEnter(Collider collider)
    {
        col = collider;
        if (armingTime <= 0)
        {
            Rigidbody rb = collider.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {              
                Vector3 forceDirection = (collider.GetComponent<Collider>().transform.position - transform.position).normalized;

                rb.AddForceAtPosition(forceDirection * knockback + Vector3.up * knockback, transform.position + new Vector3(0, -0.5f, 0), ForceMode.Impulse);

                Destroy(gameObject);
            }
        }
    }
}
