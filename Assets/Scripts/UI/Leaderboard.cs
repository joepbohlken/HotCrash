using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public GameObject rowsWrapper;
    public GameObject rowPrefab;

    public void InitializeBoard()
    {
        /*
        foreach (CarScore score in GameManager.main.scoreboard)
        {
            GameObject row = Instantiate(rowPrefab);
            row.transform.SetParent(rowsWrapper.transform, false);
        }
        */

        for (int i = 0; i < 1; i++)
        {
            GameObject row = Instantiate(rowPrefab);
            row.transform.SetParent(rowsWrapper.transform, false);
        }
    }
}
