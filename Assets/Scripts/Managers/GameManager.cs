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

    public int placement = 0;
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
    public Canvas canvas;
    public Leaderboard leaderboard;

    [HideInInspector]
    public List<PlayerVehicleSelection> playerSelections = new List<PlayerVehicleSelection>();
    [HideInInspector]
    public int playersCount = 0;
    public List<CarScore> scoreboard = new List<CarScore>();
    [HideInInspector]
    public int carsLeftAlive;

    private Animator animator;
    private float currentGameTime = 0;
    private bool loadingScene = false;

    public bool initialLoad { get; private set; } = false;
    public bool gameStarted { get; set; } = false;
    public int playersLeft { get; private set; }
    public bool leaderboardOpen { get; private set; } = false;

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
        else main = this;

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

            for (int i = 0; i < scoreboard.Count; i++)
            {
                CarController car = scoreboard[i].car.GetComponent<CarController>();
                if (!car.isDestroyed)
                {
                    scoreboard[i].timeSurvived = currentGameTime;
                }
            }
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

        loadingScene = false;
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
        gameStarted = true;
        playersLeft = playersCount;

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

        if(!loadingScene)
        {
            loadingScene = true;
            StartCoroutine(LoadLevel(1));
        }
    }

    public void OnGameEnd()
    {
        // Fade out to end camera
        StartCoroutine(TransitionToLeaderboard());

        leaderboardOpen = true;
    }

    public void CleanUpLevelScene()
    {
        Transform cameraParent = FindObjectOfType<LevelManager>().cameraParentTransform;
        Transform hudParent = FindObjectOfType<LevelManager>().hudParentTransform;

        // Delete player cameras
        for (int i = 0; i < playersCount; i++)
        {
            GameObject camera = cameraParent.GetChild(i).gameObject;
            GameObject hud = hudParent.GetChild(i).gameObject;

            if (camera != null)
                Destroy(camera);

            if (hud != null)
                Destroy(hud);
        }

        // Todo instatiate spectator camera
    }

    private IEnumerator TransitionToLeaderboard()
    {
        yield return new WaitForSeconds(3f);

        leaderboard.gameObject.SetActive(true);
        AudioController.main.StopAllSounds();
    }

    public void OnUpdateScore(GameObject car, float damage = 0, bool taken = false)
    {
        CarScore carScore = scoreboard.FirstOrDefault(score => score.car == car);

        if (taken)
            carScore.damageTaken += damage;
        else
            carScore.damageDealt += damage;


        // Order by longest alive, then highest kills, highest dmg done and finally dmg taken
        scoreboard = scoreboard.OrderByDescending(a => a.timeSurvived).ThenByDescending(a => a.killCount).ThenByDescending(a => a.damageDealt).ThenByDescending(a => a.damageTaken).ToList();

        // Update the placement
        for (int i = 0; i < scoreboard.Count; i++)
        {
            scoreboard[i].placement = (i + 1);
        }
    }

    public void ReturnToMainMenu()
    {
        gameStarted = false;
        leaderboardOpen = false;

        scoreboard.Clear();

        if (!loadingScene)
        {
            loadingScene = true;
            StartCoroutine(LoadLevel(0));
        }
    }

    public void OnCarDeath(GameObject car, GameObject carDestroyer)
    {
        bool carDestroyNotNull = carDestroyer != null;

        CarScore destroyedCarScore = scoreboard.FirstOrDefault(score => score.car == car);
        CarScore carDestroyerScore = scoreboard.FirstOrDefault(score => score.car == carDestroyer);

        // Update the car destroyers score
        if (carDestroyNotNull)
            carDestroyerScore.killCount++;

        carsLeftAlive--;

        if (car.GetComponent<CarController>().player != null)
        {
            playersLeft--;
        }

        // Order by longest alive, then highest kills, highest dmg done and finally dmg taken
        scoreboard = scoreboard.OrderByDescending(a => a.timeSurvived).ThenByDescending(a => a.killCount).ThenByDescending(a => a.damageDealt).ThenByDescending(a => a.damageTaken).ToList();

        // End game if 1 player left or no players left
        if (carsLeftAlive < 2 || playersLeft < 1)
        {
            if (carDestroyNotNull)
                carDestroyerScore.timeSurvived = currentGameTime + 1;

            OnGameEnd();
        }
    }

    private void ResetCanvas()
    {
        countDown.SetActive(false);
        leaderboard.gameObject.SetActive(false);
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        yield return new WaitForSeconds(0.5f);

        animator.Play("Crossfade_Start");

        yield return new WaitForSeconds(sceneTransitionTime);

        ResetCanvas();

        SceneManager.LoadScene(levelIndex);
    }
}
