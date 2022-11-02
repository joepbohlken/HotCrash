using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leaderboard : MonoBehaviour
{
    public GameObject rowsWrapper;
    public GameObject rowPrefab;

    public void InitializeBoard(CarScore[] scoreboard)
    {
        // Order by longest alive, then highest kills, highest dmg done and finally dmg taken

        for (int i = 0; i < scoreboard.Length; i++)
        {
            ScoreRow row = Instantiate(rowPrefab).GetComponent<ScoreRow>();
            row.transform.SetParent(rowsWrapper.transform, false);

            // Set positioning
            RectTransform rect = row.GetComponent<RectTransform>();
            Vector2 pos = rect.anchoredPosition;
            float offset = 2f;

            pos.y -= (i * rect.sizeDelta.y + (i != 0 ? offset * i : 0));
            rect.anchoredPosition = pos;

            // Update score texts
            row.placement.text = (i + 1).ToString();
            row.name.text = scoreboard[i].car.gameObject.name;
            row.killCount.text = scoreboard[i].killCount.ToString();
            row.damageDealt.text = scoreboard[i].damageDealt.ToString();
            row.damageTaken.text = scoreboard[i].damageTaken.ToString();
        }
    }
}
