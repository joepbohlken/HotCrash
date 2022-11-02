using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
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

    [Header("Targeting")]
    public Image topLeftBorder;
    public Image bottomLeftBorder;
    public Image topRightBorder;
    public Image bottomRightBorder;
    public MeshCollider targetCollider;
    public Camera carCamera;

    [HideInInspector]
    public CarController car;

    private void Start()
    {
        abilityName.enabled = false;
        abilityIcon.enabled = false;
        timerProgress.fillAmount = 0;
    }

    private void Update()
    {
        UpdateKillCount();
        UpdateOpponentCount();
        TargetingReticle();
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
        //killsText.text = car.killCount.ToString();
    }

    private void TargetingReticle()
    {
        if (targetCollider != null && carCamera != null)
        {
            Vector3 boundPoint1 = targetCollider.bounds.min;
            Vector3 boundPoint2 = targetCollider.bounds.max;
            Vector3 boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
            Vector3 boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
            Vector3 boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
            Vector3 boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
            Vector3 boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
            Vector3 boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);

            Vector2[] screenPoints = new Vector2[8];
            screenPoints[0] = carCamera.WorldToScreenPoint(boundPoint1);
            screenPoints[1] = carCamera.WorldToScreenPoint(boundPoint2);
            screenPoints[2] = carCamera.WorldToScreenPoint(boundPoint3);
            screenPoints[3] = carCamera.WorldToScreenPoint(boundPoint4);
            screenPoints[4] = carCamera.WorldToScreenPoint(boundPoint5);
            screenPoints[5] = carCamera.WorldToScreenPoint(boundPoint6);
            screenPoints[6] = carCamera.WorldToScreenPoint(boundPoint7);
            screenPoints[7] = carCamera.WorldToScreenPoint(boundPoint8);

            Vector2 topLeftPosition = Vector2.zero;
            Vector2 topRightPosition = Vector2.zero;
            Vector2 bottomLeftPosition = Vector2.zero;
            Vector2 bottomRightPosition = Vector2.zero;

            for (int a = 0; a < screenPoints.Length; a++)
            {
                //Top Left
                if (topLeftPosition.x == 0 || topLeftPosition.x > screenPoints[a].x)
                {
                    topLeftPosition.x = screenPoints[a].x;
                }
                if (topLeftPosition.y == 0 || topLeftPosition.y > Screen.height - screenPoints[a].y)
                {
                    topLeftPosition.y = Screen.height - screenPoints[a].y;
                }
                //Top Right
                if (topRightPosition.x == 0 || topRightPosition.x < screenPoints[a].x)
                {
                    topRightPosition.x = screenPoints[a].x;
                }
                if (topRightPosition.y == 0 || topRightPosition.y > Screen.height - screenPoints[a].y)
                {
                    topRightPosition.y = Screen.height - screenPoints[a].y;
                }
                //Bottom Left
                if (bottomLeftPosition.x == 0 || bottomLeftPosition.x > screenPoints[a].x)
                {
                    bottomLeftPosition.x = screenPoints[a].x;
                }
                if (bottomLeftPosition.y == 0 || bottomLeftPosition.y < Screen.height - screenPoints[a].y)
                {
                    bottomLeftPosition.y = Screen.height - screenPoints[a].y;
                }
                //Bottom Right
                if (bottomRightPosition.x == 0 || bottomRightPosition.x < screenPoints[a].x)
                {
                    bottomRightPosition.x = screenPoints[a].x;
                }
                if (bottomRightPosition.y == 0 || bottomRightPosition.y < Screen.height - screenPoints[a].y)
                {
                    bottomRightPosition.y = Screen.height - screenPoints[a].y;
                }
            }

            topLeftBorder.transform.localPosition = new Vector2(topLeftPosition.x, topLeftPosition.y);
            topRightBorder.transform.localPosition = new Vector2(topRightPosition.x, topRightPosition.y);
            bottomLeftBorder.transform.localPosition = new Vector2(bottomLeftPosition.x, bottomLeftPosition.y);
            bottomRightBorder.transform.localPosition = new Vector2(bottomRightPosition.x, bottomRightPosition.y);

            topLeftBorder.enabled = true;
            topRightBorder.enabled = true;
            bottomLeftBorder.enabled = true;
            bottomRightBorder.enabled = true;
        }
        else
        {
            topLeftBorder.enabled = false;
            topRightBorder.enabled = false;
            bottomLeftBorder.enabled = false;
            bottomRightBorder.enabled = false;
        }
    }
}
