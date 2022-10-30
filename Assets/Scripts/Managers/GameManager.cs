using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerVehicleSelection
{
    public PlayerController player;

    public string carName;
    public Material carColor;
}

public class GameManager : MonoBehaviour
{
    public static GameManager main;

    public float sceneTransitionTime = 1f;

    [HideInInspector]
    public List<PlayerVehicleSelection> playerSelections = new List<PlayerVehicleSelection>();
    [HideInInspector]
    public int playersCount = 0;
    [HideInInspector]
    public List<GameObject> cars = new List<GameObject>();
    [HideInInspector]
    public int carsLeftAlive;

    private Animator animator;
    private bool initialLoad = false;

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

    public void StartGame(CarSelectionSlot[] slots)
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

        StartCoroutine(LoadLevel(2));
    }

    public void EndGame()
    {

    }

    public void OnCarDeath(GameObject car, GameObject killer)
    {
        cars.Remove(car);

        GameObject killerCar = cars.FirstOrDefault(car => car == killer);
    }

    private IEnumerator LoadLevel(int levelIndex)
    {
        animator.Play("Crossfade_Start");

        yield return new WaitForSeconds(sceneTransitionTime);

        SceneManager.LoadScene(levelIndex);
    }
}
