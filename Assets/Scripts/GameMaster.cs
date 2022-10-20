using Cinemachine;
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

    [Header("Split-screen Testing")]
    public Canvas overlay;
    public GameObject cameraPrefab;
    public GameObject carCanvasPrefab;
    public GameObject playerHudPrefab;
    public GameObject carPrefab;

    public Transform playerParentTransform;
    public Transform cameraParentTransform;
    public Transform hudParentTransform;
    public TextMeshProUGUI text;

    public bool spectatorCamera = true;

    [Header("Global References")]
    public Transform wheelContainer;

    private List<PlayerInput> players = new List<PlayerInput>();
    private List<GameObject> playerPrefabs = new List<GameObject>();
    [SerializeField]
    private List<Transform> startingPoints;
    [SerializeField]
    private List<LayerMask> playerLayers;

    [SerializeField]
    private int playerAmount;

    [SerializeField]
    private PlayerInputManager playerInputManager;

    private void Awake()
    {
        if (main != null) Destroy(this);
        main = this;

        playerInputManager = FindObjectOfType<PlayerInputManager>();
        if (LoadScene.playerAmount != 0)
        {
            playerAmount = LoadScene.playerAmount;
        }
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    private void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }

    public void AddPlayer(PlayerInput player)
    {
        players.Add(player);

        Transform playerParent = player.transform.parent;
        playerParent.position = startingPoints[players.Count - 1].position + new Vector3(0, 1, 0);
        playerParent.rotation = startingPoints[players.Count - 1].rotation;

        //convert layer mask (bit) to an integer 
        int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);

        //set the layer
        playerParent.GetComponentInChildren<CinemachineVirtualCamera>().gameObject.layer = layerToAdd;
        //add the layer
        playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;

        playerPrefabs.Add(player.transform.parent.gameObject);

        if(players.Count >= playerAmount)
        {
            playerInputManager.DisableJoining();
            Time.timeScale = 1;
            InitializeScene(players.Count);
        }
    }

    private void Update()
    {
        text.text = "Players Currently Ready " + players.Count;
    }

    public void InitializeScene(int playerCount = 0)
    {
        if (playerCount <= 0)
        {
            return;
        }

        Destroy(overlay.gameObject);

        for (int i = 0; i < playerCount; i++)
        {
            // Add car
            playerPrefabs[i].name = "Player " + (i + 1);

            /*
            // Add camera
            CameraFollow cameraFollow = Instantiate(cameraPrefab, cameraParentTransform).GetComponent<CameraFollow>();
            cameraFollow.gameObject.name = "Camera Player " + (i + 1);
            cameraFollow.target = car.transform.Find("Body");
            cameraFollow.mouseSensitivity = i != 0 ? 0 : 2;
            */

            float dividerOffset = playerCount != 1 ? 0.001f : 0;
            float sizeX = Mathf.Clamp(1 / playerCount, 0.5f, 1);
            float sizeY = playerCount <= 2 ? 1 : 0.5f;
            float posX = (i % 2) * 0.5f;
            float posY = i >= 2 || playerCount <= 2 ? 0 : 0.5f;

            // Set camera size and position
            Camera camera = playerPrefabs[i].GetComponentInChildren<Camera>();
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

            camera.cullingMask =~ LayerMask.GetMask(layersToIgnore);
            

            // Add player hud
            Canvas carHUD = Instantiate(playerHudPrefab, hudParentTransform).GetComponent<Canvas>();
            carHUD.worldCamera = cameraUI;
            carHUD.GetComponent<HUD>().car = playerPrefabs[i].gameObject;

            if (playerCount >= 3)
            {
                carHUD.scaleFactor = .75f;
            }

            // Set car health HUD images
            CarHealth carHealth = playerPrefabs[i].GetComponentInChildren<CarHealth>();
            HUD hud = carHUD.GetComponent<HUD>();
            carHealth.healthBars.Add(hud.bars);
            carHealth.healthTexts.Add(hud.hpText);

            playerPrefabs[i].GetComponentInChildren<AbilityController>().hud = hud;

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

            // Add car canvases
            for (int j = 0; j < playerCount; j++)
            {
                if (j != i)
                {
                    CarCanvas carCanvas = Instantiate(carCanvasPrefab, playerPrefabs[i].transform.Find("UI")).GetComponentInChildren<CarCanvas>();
                    carCanvas.transform.parent.gameObject.layer = LayerMask.NameToLayer("Player " + (j + 1));

                    carHealth.healthBars.Add(carCanvas.bars);
                    carHealth.healthTexts.Add(carCanvas.hpText);
                }
            }
        }

        // Set camera to follow 
        // Loop through all the players
        for (int x = 0; x < playerCount; x++)
        {
            Transform carCanvases = playerParentTransform.GetChild(x).GetChild(0).Find("UI");

            // Loop through all of its canvases using playerCount
            for (int y = 0; y < playerCount; y++)
            {
                if (x != y)
                {
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
            cameraFollow.target = playerParentTransform;
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
}
