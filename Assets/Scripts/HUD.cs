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
}
