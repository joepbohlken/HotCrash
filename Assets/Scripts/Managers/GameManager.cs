using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerVehicleSelection
{
    public PlayerController player;

    public string carName;
    public Material carColor;
}

[Serializable]
public class CarScore
{
    public GameObject car;

    public int killCount = 0;
    public float damageDealt = 0;
    public float damageTaken = 0;
    public float timeSurvived = 0;
}

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    public float sceneTransitionTime = 1f;
    public float countDownTime = 3f;
    public GameObject countDown;
    public TextMeshProUGUI countDownText;

    [HideInInspector]
    public List<PlayerVehicleSelection> playerSelections = new List<PlayerVehicleSelection>();
    [HideInInspector]
    public int playersCount = 0;
    public List<CarScore> scoreboard = new List<CarScore>();
    [HideInInspector]
    public int carsLeftAlive;

    private Animator animator;
    private bool initialLoad = false;

    private bool gameStarted = false;
    private float currentGameTime = 0;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Awake()
    {
        initialLoad = true;

        if (main != null) Destroy(gameObject);
        main = this;

        DontDestroyOnLoad(gameObject);

        animator = GetComponentInChildren<Animator>();

        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (gameStarted)
        {
            currentGameTime += Time.deltaTime;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!initialLoad)
        {
            animator.Play("Crossfade_End");
        }
        else
        {
            initialLoad = false;
        }
    }

    public void OnStartGame()
    {
        StartCoroutine(StartCountDown());
    }

    private IEnumerator StartCountDown()
    {
        countDown.SetActive(true);

        float timeLeft = countDownTime;

        while (timeLeft > 0)
        {
            countDownText.text = timeLeft.ToString();

            yield return new WaitForSeconds(1f);

            timeLeft--;
        }

        countDownText.text = "GO!";

        foreach (CarScore score in scoreboard)
        {
            score.car.GetComponent<CarController>().driveable = true;
        }

        yield return new WaitForSeconds(.5f);

        countDown.SetActive(false);
    }

    public void OnStartGame(CarSelectionSlot[] slots)
    {
        // Clear previous selections
        playerSelections.Clear();

        // Add new ones
        foreach (CarSelectionSlot slot in slots)
        {
            if (slot.player == null)
            {
                continue;
            }

            PlayerVehicleSelection selection = new PlayerVehicleSelection()
            {
                player = slot.player,
                carName = slot.carName,
                carColor = slot.carColor
            };

            playerSelections.Add(selection);
        }

        StartCoroutine(LoadLevel(1));
    }

    public void OnGameEnd()
    {

    }

    public void OnUpdateScore(GameObject car, GameObject opponent, float damage = 0)
    {
        CarScore carScore = scoreboard.FirstOrDefault(score => score.car == car);
        CarScore opponentScore = scoreboard.FirstOrDefault(score => score.car == opponent);

        opponentScore.damageDealt += damage;
        carScore.damageTaken += damage;
    }

    public void OnCarDeath(GameObject car, GameObject carDestroyer)
    {
        CarScore destroyedCarScore = scoreboard.FirstOrDefault(score => score.car == car);
        CarScore carDestroyerScore = scoreboard.FirstOrDefault(score => score.car == carDestroyer);

        // Update the car destroyers score
        carDestroyerScore.killCount++;

        // Set time alive on car death
        destroyedCarScore.timeSurvived = currentGameTime;
        carsLeftAlive--;

        if (carsLeftAlive < 2)
        {
            carDestroyerScore.timeSurvived = currentGameTime;

            OnGameEnd();
        }
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        animator.Play("Crossfade_Start");

        yield return new WaitForSeconds(sceneTransitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}
