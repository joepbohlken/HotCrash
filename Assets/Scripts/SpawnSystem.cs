using Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> spawnPoints;
    [SerializeField]
    private int botAmount;
    [SerializeField]
    private int playerAmount;
    [SerializeField]
    private CinemachineVirtualCamera cameraBrainPrefab;
    [SerializeField]
    private Transform carContainer;

    public GameObject[] CarPrefabs;

    void Start()
    {
        int botsSpawned = 0;
        foreach (GameObject spawnPoint in spawnPoints)
        {
            GameObject carToSpawn = CarPrefabs[Random.Range(0, CarPrefabs.Length)];
            /*
            if (playersSpawned < playerAmount)
            {
                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                PlayerInput input = car.GetComponent<PlayerInput>();
                input.enabled = true;
                if(playersSpawned == 0)
                {
                    input.defaultControlScheme = "Keyboard";
                }
                else
                {
                    input.defaultControlScheme = "Controller";
                }

                GameObject camBrain = Instantiate(cameraBrain, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                camBrain.transform.parent = car.transform;

                GameObject camCar = Instantiate(cameraCar, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                camCar.transform.parent = car.transform;

                CinemachineVirtualCamera cine = camCar.GetComponent<CinemachineVirtualCamera>();
                Camera cam = camBrain.GetComponent<Camera>();
                CinemachineVirtualCamera cameraBrain = Instantiate(cameraBrainPrefab);
                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation, carContainer);
                car.GetComponent<ArcadeCar>().controllable = true;
                cine.LookAt = car.transform;
                cine.Follow = car.transform;
                cam.rect = setup.GetRect(playersSpawned);
                LayerConfig(playersSpawned, cine, cam);
                playersSpawned++;
                continue;
            }
            */
            if(botsSpawned < botAmount)
            {

                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation, carContainer);
                car.GetComponent<ArcadeCar>().controllable = false;

                CarAI carAI = car.AddComponent<CarAI>();
                carAI.boxSize = new Vector3(2, 0.4f, 5);
                carAI.InitializeAI();
                
                botsSpawned++;
            }
            if(botsSpawned == botAmount)
            {
                return;
            }
        }
    }
}
