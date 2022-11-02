using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    public Image[] borders;

    [Header("Damage model")]
    public Image front;
    public Image left;
    public Image right;
    public Image back;

    [Header("HP bar")]
    public GameObject bars;
    public TextMeshProUGUI hpText;

    [Header("Kill Count")]
    public TextMeshProUGUI killsText;

    [Header("Oponnents Count")]
    public TextMeshProUGUI opponentsText;

    [Header("Abilities")]
    public Image abilityIcon;
    public Image timerProgress;
    public TextMeshProUGUI abilityName;
    public TextMeshProUGUI timerText;

    public PlayerController player { get; set; }

    private void Start()
    {
        abilityName.enabled = false;
        abilityIcon.enabled = false;
        timerProgress.fillAmount = 0;

        foreach (Image border in borders)
        {
            border.color = player.playerColor;
        }

        if (GameManager.main == null)
        {
            opponentsText.transform.parent.gameObject.SetActive(false);
            killsText.transform.parent.gameObject.SetActive(false);
        }
    }

    private void Update()
    {

        if (GameManager.main != null)
        {
            UpdateKillCount();
            UpdateOpponentCount();
        }
    }

    public void SetInfo(Ability ability)
    {
        abilityIcon.sprite = ability.abilityIcon;
        abilityName.text = ability.abilityName;
    }

    public void StartCountdown(float cooldown)
    {
        timerProgress.fillAmount = 0;
        abilityIcon.enabled = false;
        abilityName.enabled = false;
        timerProgress.enabled = true;
        timerText.enabled = true;
        StartCoroutine(UpdateTimerFillAmount(cooldown));
    }

    private IEnumerator UpdateTimerFillAmount(float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            timerProgress.fillAmount = Mathf.Lerp(0, 1, t / duration);
            timerText.text = (duration - Mathf.FloorToInt(t)).ToString();
            yield return null;
        }

        timerProgress.fillAmount = 1;
        timerProgress.enabled = false;
        timerText.enabled = false;
        abilityIcon.enabled = true;
        abilityName.enabled = true;
    }

    private void UpdateOpponentCount()
    {
        if (GameManager.main != null)
        {
            opponentsText.text = (GameManager.main.carsLeftAlive - 1).ToString();
        }
    }

    private void UpdateKillCount()
    {
        if (GameManager.main != null)
        {
            foreach (CarScore score in GameManager.main.scoreboard)
            {
                if (score.car.GetComponent<CarController>().player == player)
                {
                    killsText.text = score.killCount.ToString();
                }
            }
        }
    }
}
