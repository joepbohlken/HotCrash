using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityDisplay : MonoBehaviour
{
    [SerializeField]
    private Image AbilityImage;
    [SerializeField]
    private Image TimerImage;
    [SerializeField]
    private Text AbilityName;
    [SerializeField]
    private TextMeshProUGUI TimerText;

    private void Start()
    {

        TimerImage.enabled = false;
        TimerText.enabled = false;
    }

    public void SetInfo(Ability ability)
    {
        AbilityImage.sprite = ability.AbilityImage;
        AbilityName.text = ability.a_Name;
    }

    public void StartCountdown(float cooldown)
    {
        TimerImage.fillAmount = 0;
        AbilityImage.enabled = false;
        AbilityName.enabled = false;
        TimerImage.enabled = true;
        TimerText.enabled = true;
        StartCoroutine(UpdateTimerFillAmount(cooldown));
    }

    private IEnumerator UpdateTimerFillAmount(float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            TimerImage.fillAmount = Mathf.Lerp(0, 1, t / duration);
            TimerText.text = (duration - Mathf.FloorToInt(t)).ToString();
            yield return null;
        }

        TimerImage.fillAmount = 1;
        TimerImage.enabled = false;
        TimerText.enabled = false;
        AbilityImage.enabled = true;
        AbilityName.enabled = true;
    }
}
