using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineExplode : MonoBehaviour
{
    [SerializeField]
    private float armingTime = 3;
    [SerializeField]
    private float mineRadius = 10;
    [SerializeField]
    private float mineStrength = 10000;
    [SerializeField]
    private ParticleSystem explosion;
    [SerializeField]
    private float explosionDelay = 5;

    private void OnTriggerEnter(Collider collider)
    {
        if (armingTime <= 0)
        {
            Collider[] colliders = Physics.OverlapSphere(transform.position, mineRadius);
            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody rb = collider.GetComponentInParent<Rigidbody>();
                if (rb != null)
                {
                    StartCoroutine(explode(explosionDelay, rb));
                    Debug.Log(i);
                    
                    Debug.Log(rb);
                }
            }
        }
    }
    private IEnumerator explode(float time, Rigidbody rb)
    {
        yield return new WaitForSeconds(time);
        rb.AddExplosionForce(mineStrength, transform.position, mineRadius, 1f, ForceMode.Impulse);
        Instantiate(explosion, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    private void Update()
    {
        armingTime -= Time.deltaTime;
    }
}
