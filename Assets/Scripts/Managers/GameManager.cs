using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    MAINMENU,
    ASSIGNINGPLAYERS,
    PAUSE,
    GAMESTART,
    GAMESTOP
}

public class PlayerVehicleSelection
{
    public PlayerSlot player;

    public GameObject carPrefab;
    public Color carColor;
}

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
