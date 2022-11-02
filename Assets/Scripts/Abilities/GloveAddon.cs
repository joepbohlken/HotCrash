using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GloveAddon : MonoBehaviour
{
    public int knockback;
    public float speed;
    public float armingTime = 0.1f;
    public Transform target;
    public Transform location;

    private void Start()
    {
        location = transform.parent.transform;
    }

    private void Update()
    {
        //Time
        armingTime -= Time.deltaTime;

        //Correct position
        Vector3 correctedPos = target.position;
        correctedPos.y += 0.45f;

        Vector3 targetDirection = (target.position - location.position);
        Vector3 newDirection = Vector3.RotateTowards(location.forward, targetDirection, 360, 0.0f);
        location.rotation = Quaternion.LookRotation(newDirection);
        location.position = Vector3.MoveTowards(location.position, correctedPos, Time.deltaTime * speed);
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (armingTime <= 0)
        {
            Rigidbody rb = collider.GetComponentInParent<Rigidbody>();
            if (rb != null)
            {              
                Vector3 forceDirection = (collider.GetComponent<Collider>().transform.position - transform.position).normalized;

                rb.AddForceAtPosition(forceDirection * knockback + Vector3.up * knockback, transform.position + new Vector3(0, -0.5f, 0), ForceMode.Impulse);

                Destroy(gameObject.transform.parent.gameObject);
            }
        }
    }
}
