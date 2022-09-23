using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineExplode : MonoBehaviour
{
    private float armingTime = 3;
    private float mineRadius = 10;
    [SerializeField]
    private float mineStrength = 10000;
    [SerializeField]
    private ParticleSystem explosion;

    private void OnTriggerEnter(Collider collider)
    {
        if (armingTime <= 0)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, mineRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody rb = colliders[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.AddExplosionForce(mineStrength, transform.position, mineRadius, 1f, ForceMode.Impulse);
                    Instantiate(explosion, transform.position, transform.rotation);
                    Destroy(gameObject);
                }
            }
        }
    }

    private void Update()
    {
        armingTime -= Time.deltaTime;
    }
}
