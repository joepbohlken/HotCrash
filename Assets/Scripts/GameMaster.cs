using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    [HideInInspector]
    public static GameMaster main;

    [Header("Global settings")]
    public int totalPlayers = 12;

    [Header("Spawning References")]
    public Canvas overlay;
    public GameObject cameraPrefab;
    public GameObject carCanvasPrefab;
    public GameObject playerHudPrefab;
    public GameObject carPrefab;

    public Transform carParentTransform;
    public Transform cameraParentTransform;
    public Transform hudParentTransform;
    public Transform spawnPointsTransform;

    public bool spectatorCamera = true;

    [Header("Global References")]
    public Transform wheelContainer;

    private PlayerInputManager playerInputManager;
    private List<Transform> spawnPoints = new List<Transform>();
    private List<PlayerInput> players = new List<PlayerInput>();

    private float currentTime = 5;
    private bool isStarted = false;

    private void OnEnable()
    {
        playerInputManager.onPlayerJoined += OnPlayerJoin;
        playerInputManager.onPlayerLeft += OnPlayerLeave;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= OnPlayerJoin;
        playerInputManager.onPlayerLeft += OnPlayerLeave;
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        playerInputManager = GetComponent<PlayerInputManager>();
        //playerInputManager.DisableJoining();

        if (main != null) Destroy(this);
        main = this;

        if (spawnPointsTransform != null)
        {
            for (int i = 0; i < spawnPointsTransform.childCount; i++)
            {
                spawnPoints.Add(spawnPointsTransform.GetChild(i).GetComponent<Transform>());
            }
        }
    }

    private void Update()
    {
        currentTime -= Time.deltaTime;

        if (currentTime <= 0 && !isStarted)
        {
            isStarted = true;
            DisablePlayerJoining();
            InitializeGame();
        }
    }

    public void OnPlayerJoin(PlayerInput input)
    {
        players.Add(input);
    }

    public void OnPlayerLeave(PlayerInput input)
    {
        players.Remove(input);
        Destroy(input.gameObject);
    }

    public void EnablePlayerJoining()
    {
        playerInputManager.EnableJoining();
    }

    public void DisablePlayerJoining()
    {
        playerInputManager.DisableJoining();
    }

    public void InitializeGame()
    {
        if (players.Count <= 0)
        {
            return;
        }

        int playerCount = players.Count;
        Destroy(overlay.gameObject);

        // Spawn players
        foreach (var (input, i) in players.Select((value, i) => (value, i)))
        {
            SpawnCar(i, input);
        }

        // Spawn bots
        for (int i = 0; i < totalPlayers - playerCount; i++)
        {
            SpawnCar(i);
        }

        // Set camera to follow 
        // Loop through all the players
        for (int x = 0; x < totalPlayers; x++)
        {
            Transform carCanvases = carParentTransform.GetChild(x).Find("UI");

            // Loop through all of its canvases using playerCount
            for (int y = 0; y < playerCount; y++)
            {
                if (x != y)
                {
                    // Find canvas
                    int actualIndex = x < y ? y - 1 : y;

                    CarCanvas carCanvas = carCanvases.GetChild(actualIndex).GetComponentInChildren<CarCanvas>();
                    carCanvas.cameraToFollow = cameraParentTransform.GetChild(y).GetComponentInChildren<Camera>();
                }
            }
        }

        // Add spectator camera
        if (spectatorCamera && playerCount == 3)
        {
            CameraFollow cameraFollow = Instantiate(cameraPrefab, cameraParentTransform).GetComponent<CameraFollow>();
            cameraFollow.gameObject.name = "Spectator Camera";
            cameraFollow.target = carParentTransform;
            cameraFollow.mouseSensitivity = 0;

            float dividerOffset = 0.001f;

            // Set camera size and position
            Camera camera = cameraFollow.GetComponentInChildren<Camera>();
            Rect rect = camera.rect;

            rect.width = 0.5f - dividerOffset;
            rect.height = 0.5f - dividerOffset * 2;
            rect.x = 0.5f + dividerOffset;
            rect.y = 0;

            camera.rect = rect;
        }
    }

    private void SpawnCar(int i, PlayerInput input = null)
    {
        // Get random spawn position
        // Stop when there no more spawn positions
        if (spawnPoints.Count == 0)
        {
            Debug.Log("No spawn points left. Unable to spawn");
            return;
        }

        int playerCount = players.Count;
        bool isPlayer = input != null;

        int spawnIndex = Random.Range(0, spawnPoints.Count - 1);
        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        // Remove the used spawn point
        spawnPoints.RemoveAt(spawnIndex);

        // Add car
        ArcadeCar car = Instantiate(carPrefab, spawnPos, Quaternion.LookRotation((Vector3.zero - spawnPos), transform.up), carParentTransform).GetComponent<ArcadeCar>();
        car.gameObject.name = isPlayer ? "Player " + (i + 1) : "Bot";
        car.isBot = !isPlayer;

        CarHealth carHealth = car.GetComponent<CarHealth>();

        if (isPlayer)
        {
            // Add camera
            CameraFollow cameraFollow = Instantiate(cameraPrefab, cameraParentTransform).GetComponent<CameraFollow>();
            cameraFollow.gameObject.name = "Camera Player " + (i + 1);
            cameraFollow.target = car.transform.Find("Body");
            cameraFollow.mouseSensitivity = i != 0 ? 0 : 2;

            // Calculate camera size
            float dividerOffset = playerCount != 1 ? 0.001f : 0;
            float sizeX = Mathf.Clamp(1 / playerCount, 0.5f, 1);
            float sizeY = playerCount <= 2 ? 1 : 0.5f;
            float posX = (i % 2) * 0.5f;
            float posY = i >= 2 || playerCount <= 2 ? 0 : 0.5f;

            // Set camera size and position
            Camera camera = cameraFollow.GetComponentInChildren<Camera>();
            Camera cameraUI = camera.transform.GetChild(0).GetComponent<Camera>();
            Rect rect = camera.rect;

            rect.width = sizeX - dividerOffset;
            rect.height = sizeY - dividerOffset * 2;
            rect.x = posX + (posX > 0 ? dividerOffset : 0);
            rect.y = posY + (posY > 0 ? dividerOffset * 2 : 0);

            //Adjust screens on 3rd player
            if (!spectatorCamera)
            {
                if (i == 0 && playerCount == 3)
                {
                    rect.width = 1;
                }

                if ((i == 1 || i == 2) && playerCount == 3)
                {
                    rect.x = (i - 1) * 0.5f + (i - 1 != 0 ? dividerOffset : 0);
                    rect.y = 0;
                }
            }

            camera.rect = rect;
            cameraUI.rect = rect;

            // Set camera layer mask
            string[] layersToIgnore = Enumerable.Range(0, playerCount).ToArray().Select(num =>
            {
                if (num != i)
                {
                    return "Player " + (num + 1);
                }
                else
                {
                    return "UI";
                }
            }).ToArray();

            camera.cullingMask = ~LayerMask.GetMask(layersToIgnore);

            // Add player hud
            Canvas carHUD = Instantiate(playerHudPrefab, hudParentTransform).GetComponent<Canvas>();
            carHUD.worldCamera = cameraUI;
            carHUD.GetComponent<HUD>().car = car.gameObject;

            // Adjust hud scaling
            if (playerCount >= 3)
            {
                carHUD.scaleFactor = .75f;
            }

            // Set car health HUD images
            HUD hud = carHUD.GetComponent<HUD>();
            carHealth.healthBars.Add(hud.bars);
            carHealth.healthTexts.Add(hud.hpText);

            AbilityController abilityController = car.GetComponent<AbilityController>();
            abilityController.hud = hud;

            // Set correct image per vital
            foreach (Vitals vital in carHealth.vitals)
            {
                switch (vital.vitalType)
                {
                    case HitLocation.FRONT:
                        vital.image = hud.front;
                        break;
                    case HitLocation.LEFT:
                        vital.image = hud.left;
                        break;
                    case HitLocation.RIGHT:
                        vital.image = hud.right;
                        break;
                    case HitLocation.BACK:
                        vital.image = hud.back;
                        break;
                }
            }

            // Add corresponding scripts to the player controller
            input.GetComponent<PlayerController>().car = car;
            input.GetComponent<PlayerController>().cameraFollow = cameraFollow;
            input.GetComponent<PlayerController>().abilityController = abilityController;
        }

        // Add car canvases
        for (int j = 0; j < playerCount; j++)
        {
            if ((j != i && isPlayer) || !isPlayer)
            {
                CarCanvas carCanvas = Instantiate(carCanvasPrefab, car.transform.Find("UI")).GetComponentInChildren<CarCanvas>();
                carCanvas.transform.parent.gameObject.layer = LayerMask.NameToLayer("Player " + (j + 1));

                carHealth.healthBars.Add(carCanvas.bars);
                carHealth.healthTexts.Add(carCanvas.hpText);
            }
        }
    }
}
