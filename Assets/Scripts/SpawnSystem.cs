using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> spawnPoints;
    [SerializeField]
    private List<LayerMask> playerLayers;
    [SerializeField]
    private int botAmount;
    [SerializeField]
    private int playerAmount;

    public GameObject[] CarPrefabs;
    public GameObject cameraBrain;
    public GameObject cameraCar;

    private CameraSetup setup = new CameraSetup();

    void Start()
    {
        setup.SetCamera();
        if(LoadScene.playerAmount != 0)
        {
            playerAmount = LoadScene.playerAmount;
        }
        int playersSpawned = 0;
        int botsSpawned = 0;
        foreach (GameObject spawnPoint in spawnPoints)
        {
            GameObject carToSpawn = CarPrefabs[Random.Range(0, CarPrefabs.Length)];
            if (playersSpawned < playerAmount)
            {
                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                playersSpawned++;

                GameObject camBrain = Instantiate(cameraBrain, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                camBrain.transform.parent = car.transform;

                GameObject camCar = Instantiate(cameraCar, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                camCar.transform.parent = car.transform;

                CinemachineVirtualCamera cine = camCar.GetComponent<CinemachineVirtualCamera>();
                Camera cam = camBrain.GetComponent<Camera>();
                car.GetComponent<ArcadeCar>().controllable = true;
                cine.LookAt = car.transform;
                cine.Follow = car.transform;
                cam.rect = setup.GetRect(playersSpawned - 1);
                LayerConfig(playersSpawned - 1, cine, cam);
                continue;
            }
            if(botsSpawned < botAmount)
            {

                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                car.GetComponent<ArcadeCar>().controllable = false;
                botsSpawned++;
            }
            if(playersSpawned == playerAmount && botsSpawned == botAmount)
            {
                return;
            }
        }
    }

    void LayerConfig(int player, CinemachineVirtualCamera cine, Camera cam)
    {
        int layerToAdd = (int)Mathf.Log(playerLayers[player].value, 2);

        cine.gameObject.layer = layerToAdd;

        cam.cullingMask |= 1 << layerToAdd;
    }
}
