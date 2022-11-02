using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreRow : MonoBehaviour
{
    public TextMeshProUGUI placement;
    public TextMeshProUGUI name;
    public TextMeshProUGUI killCount;
    public TextMeshProUGUI damageDealt;
    public TextMeshProUGUI damageTaken;

    public CarScore score;

    private void Start()
    {
        UpdateScore();
    }

    public void UpdateScore()
    {
        placement.text = score.placement.ToString();
        name.text = score.car.gameObject.name;
        killCount.text = score.killCount.ToString();
        damageDealt.text = Mathf.RoundToInt(score.damageDealt).ToString();
        damageTaken.text = Mathf.RoundToInt(score.damageTaken).ToString();
    }
}
