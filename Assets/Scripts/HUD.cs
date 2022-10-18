using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Damage model")]
    public Image Front;
    public Image Left;
    public Image Right;
    public Image Back;

    [Header("HP bar")]
    public GameObject Bars;
    public TextMeshProUGUI hpText;

    [Header("Kill Count")]
    public TextMeshProUGUI killsText;

    [Header("Oponnents Count")]
    public TextMeshProUGUI opponentsText;
}
