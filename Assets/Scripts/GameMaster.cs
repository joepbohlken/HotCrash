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

    private List<Transform> spawnPoints = new List<Transform>();
    private int playerCount;

    private void Awake()
    {
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

    public void InitializeScene(int playerCount = 0)
    {
        this.playerCount = playerCount;
        if (playerCount <= 0)
        {
            return;
        }

        Destroy(overlay.gameObject);

        for (int i = 0; i < totalPlayers; i++)
        {
            bool isPlayer = i < playerCount;
            SpawnCar(isPlayer, i);
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

    private void SpawnCar(bool isPlayer, int i)
    {
        // Get random spawn position
        // Stop when there no more spawn positions
        if (spawnPoints.Count == 0)
        {
            Debug.Log("No spawn points left. Unable to spawn");
            return;
        }

        int spawnIndex = Random.Range(0, spawnPoints.Count - 1);
        Vector3 spawnPos = spawnPoints[spawnIndex].position;

        // Remove the used spawn point
        spawnPoints.RemoveAt(spawnIndex);

        // Add car
        ArcadeCar car = Instantiate(carPrefab, spawnPos, Quaternion.LookRotation((Vector3.zero - spawnPos), transform.up), carParentTransform).GetComponent<ArcadeCar>();
        car.gameObject.name = isPlayer ? "Player " + (i + 1) : "Bot " + (i + 1);
        car.controllable = i == 0;

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

            Debug.Log(layersToIgnore.LastOrDefault());
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

            car.GetComponent<AbilityController>().hud = hud;

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
        }

        // Add car canvases
        for (int j = 0; j < playerCount; j++)
        {
            if (j != i)
            {
                CarCanvas carCanvas = Instantiate(carCanvasPrefab, car.transform.Find("UI")).GetComponentInChildren<CarCanvas>();
                carCanvas.transform.parent.gameObject.layer = LayerMask.NameToLayer("Player " + (j + 1));

                carHealth.healthBars.Add(carCanvas.bars);
                carHealth.healthTexts.Add(carCanvas.hpText);
            }
        }
    }
}
