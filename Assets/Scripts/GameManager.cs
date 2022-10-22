using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    [Range(0, 4)]
    public int playersCount = 0;

    private void Awake()
    {
        if (main != null) Destroy(gameObject);
        main = this;

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        
    }
}
