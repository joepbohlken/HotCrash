using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car1 : MonoBehaviour

{
    [SerializeField]
    private GameObject explosion;
    [SerializeField]
    private GameObject title;

    public void playExplosion()
    {
        explosion.SetActive(true);
        title.SetActive(true);
    }
}
