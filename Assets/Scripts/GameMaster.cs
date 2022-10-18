using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameMaster : MonoBehaviour
{
    public Canvas overlay;
    public GameObject cameraPrefab;
    public GameObject carCanvasPrefab;
    public GameObject playerHudPrefab;
    public GameObject carPrefab;

    public Transform carParentTransform;
    public Transform cameraParentTransform;
    public Transform hudParentTransform;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void InitializeCamera(int playerCount = 0)
    {
        if (playerCount <= 0)
        {
            return;
        }

        Destroy(overlay.gameObject);

        for (int i = 0; i < playerCount; i++)
        {
            // Add car
            ArcadeCar car = Instantiate(carPrefab, (Vector3.up * 1.5f) + (Vector3.right * i * 4f), new Quaternion(0, 0, 0, 0), carParentTransform).GetComponent<ArcadeCar>();
            car.controllable = i == 0;

            // Add camera
            CameraFollow cameraFollow = Instantiate(cameraPrefab, cameraParentTransform).GetComponent<CameraFollow>();
            cameraFollow.gameObject.name = "Camera Player " + (i + 1);
            cameraFollow.target = car.transform.Find("Body");

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

            camera.rect = rect;
            cameraUI.rect = rect;

            // Set camera layer mask
            string[] layersToIgnore = Enumerable.Range(0, playerCount).ToArray().Select(num => {
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
            
            if (playerCount > 3)
            {
                carHUD.scaleFactor = .75f;
            }

            // Set car health HUD images
            CarHealth carHealth = car.GetComponent<CarHealth>();
            HUD hud = carHUD.GetComponent<HUD>();
            carHealth.healthBar = hud.Bars;
            carHealth.healthText = hud.hpText;

            foreach (Vitals vital in carHealth.vitals) 
            {
                switch (vital.vitalType)
                {
                    case HitLocation.FRONT:
                        vital.image = hud.Front;
                        break;
                    case HitLocation.LEFT:
                        vital.image = hud.Left;
                        break;
                    case HitLocation.RIGHT:
                        vital.image = hud.Right;
                        break;
                    case HitLocation.BACK:
                        vital.image = hud.Back;
                        break;
                }
            }

            // Add car canvases
            for (int j = 0; j < playerCount; j++)
            {
                if(j != i)
                {
                    GameObject carCanvas = Instantiate(carCanvasPrefab, car.transform);
                    carCanvas.layer = LayerMask.NameToLayer("Player " + (j + 1));

                    carCanvas.GetComponentInChildren<CameraFollowUI>().cameraToFollow = camera;
                }
            }
        }

        // Set spectator camera

    }
}
