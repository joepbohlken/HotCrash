using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class LevelManager : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent onCarsInitialized;

    [Header("Global settings")]
    public int totalPlayers = 12;
    public bool spectatorCamera = false;

    [Header("Prefabs")]
    public GameObject cameraPrefab;
    public GameObject carCanvasPrefab;
    public GameObject playerHudPrefab;
    public GameObject[] carPrefabs;
    public Material[] carColors;
    public GameObject playerManagerPrefab;

    [Header("Containers")]
    public Transform carParentTransform;
    public Transform cameraParentTransform;
    public Transform hudParentTransform;
    public Transform spawnPointsTransform;
    public Transform wheelContainer;

    [Header("Debugging")]
    public bool initializePlayerManager = false;

    private List<Transform> spawnPoints = new List<Transform>();

    private void OnEnable()
    {
        if (initializePlayerManager && PlayerManager.main != null)
        {
            initializePlayerManager = false;
        }

        if (PlayerManager.main == null && initializePlayerManager)
        {
            PlayerManager pm = Instantiate(playerManagerPrefab).GetComponent<PlayerManager>();
            pm.onReady.AddListener(OnPlayersReady);

            pm.canCancel = false;
        }
    }

    private void OnDisable()
    {
        if (PlayerManager.main != null && initializePlayerManager)
            PlayerManager.main.onReady.RemoveListener(OnPlayersReady);
    }


    private void Awake()
    {
        if (spawnPointsTransform != null)
        {
            for (int i = 0; i < spawnPointsTransform.childCount; i++)
            {
                spawnPoints.Add(spawnPointsTransform.GetChild(i).GetComponent<Transform>());
            }
        }
    }

    private void Start()
    {
        if (!initializePlayerManager)
        {
            InitializeGame();

            GameManager.main.OnStartGame();
        }
        else
        {
            PlayerManager.main.ShowMenu(true);
        }

    }

    private void OnPlayersReady()
    {
        List<PlayerVehicleSelection> selections = new List<PlayerVehicleSelection>();

        foreach (PlayerController player in PlayerManager.main.players)
        {
            PlayerVehicleSelection selection = new PlayerVehicleSelection()
            {
                player = player,
            };

            selections.Add(selection);
        }

        InitializeGame(selections);
    }

    public void InitializeGame(List<PlayerVehicleSelection> playerSelections = null)
    {
        bool hasGameManager = GameManager.main != null;

        int playerCount = 0;
        if (PlayerManager.main != null)
        {
            playerCount = PlayerManager.main.playerCount;
        }

        List<PlayerVehicleSelection> selections = hasGameManager ? GameManager.main.playerSelections : playerSelections;

        // Spawn players
        if (playerCount > 0 && selections != null)
        {
            foreach (var (selection, i) in selections.Select((value, i) => (value, i)))
            {
                SpawnCar(i, selection);
            }
        }

        // Spawn bots
        for (int i = 0; i < totalPlayers - playerCount; i++)
        {
            SpawnCar(i);
        }

        if (hasGameManager && GameManager.main.scoreboard.Count == totalPlayers)
        {
            onCarsInitialized.Invoke();
            GameManager.main.carsLeftAlive = totalPlayers;
            GameManager.main.OnStartGame();
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
            CameraController cameraFollow = Instantiate(cameraPrefab, cameraParentTransform).GetComponent<CameraController>();
            cameraFollow.gameObject.name = "Spectator Camera";
            cameraFollow.target = carParentTransform;

            float dividerOffset = 0f;

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

    private void SpawnCar(int i, PlayerVehicleSelection selection = null)
    {
        // Get random spawn position
        // Stop when there no more spawn positions
        if (spawnPoints.Count == 0)
        {
            Debug.Log("No spawn points left. Unable to spawn");
            return;
        }

        int playerCount = 0;

        if (PlayerManager.main != null)
        {
            playerCount = PlayerManager.main.playerCount;
        }

        bool isPlayer = selection != null;

        int spawnIndex = Random.Range(0, spawnPoints.Count);
        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        // Remove the used spawn point
        spawnPoints.RemoveAt(spawnIndex);

        // Add car
        // Get random car
        int carIndex = Random.Range(0, carPrefabs.Length);
        GameObject selectedCar = carPrefabs[carIndex];

        // Overwrite car choice when player and contains valid choice
        if (selection != null)
        {
            GameObject carSelection = carPrefabs.FirstOrDefault(c => c.name == selection.carName);

            if (carSelection != null)
            {
                selectedCar = carSelection;
            }
        }

        CarController car = Instantiate(selectedCar, spawnPos, Quaternion.LookRotation((Vector3.zero - spawnPos), transform.up), carParentTransform).GetComponent<CarController>();
        car.gameObject.name = isPlayer ? "Player " + (i + 1) : "Bot " + (i + 1);

        // Set random color
        int colorIndex = Random.Range(0, carColors.Length);
        Material selectedColor = carColors[colorIndex];

        // Overwrite car choice when player and contains valid choice
        if (selection != null)
        {
            Material colorSelection = carColors.FirstOrDefault(c => c == selection.carColor);

            if (colorSelection != null)
            {
                selectedColor = colorSelection;
            }
        }

        car.transform.Find("Body").gameObject.GetComponent<Renderer>().material = selectedColor;
        car.isBot = !isPlayer;

        if (GameManager.main != null)
        {
            car.driveable = false;
        }


        if (GameManager.main != null)
        {
            CarScore score = new CarScore()
            {
                car = car.gameObject
            };

            GameManager.main.scoreboard.Add(score);
        }

        CarHealth carHealth = car.GetComponent<CarHealth>();

        if (isPlayer)
        {
            car.player = selection.player;

            // Add camera
            CameraController cameraFollow = Instantiate(cameraPrefab, cameraParentTransform).GetComponent<CameraController>();
            cameraFollow.gameObject.name = "Camera Player " + (i + 1);
            cameraFollow.target = car.transform.Find("Body");
            cameraFollow.SetCar(car);

            // Calculate camera size
            float dividerOffset = playerCount != 1 ? 0 : 0;
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
            carHUD.GetComponent<HUD>().player = selection.player;

            // Adjust hud scaling
            if (playerCount >= 3)
            {
                carHUD.scaleFactor = .75f;
            }

            // Set car health HUD images
            HUD hud = carHUD.GetComponent<HUD>();
            carHealth.healthBars.Add(hud.bars);
            carHealth.healthTexts.Add(hud.hpText);

            // Set ability controller references
            AbilityController abilityController = car.GetComponent<AbilityController>();
            abilityController.hud = hud;
            abilityController.playerCamera = cameraFollow.cameraObject.GetComponent<Camera>();
            // ---
            hud.carCamera = cameraUI;

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
            selection.player.car = car;
            selection.player.cameraFollow = cameraFollow;
            selection.player.abilityController = abilityController;
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

                car.carCanvasRefs.Add(carCanvas);
            }
        }
    }
}
