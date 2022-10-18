using Cinemachine;
using System.Collections.Generic;
using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> spawnPoints;
    [SerializeField]
    private int botAmount;
    [SerializeField]
    private int playerAmount;
    [SerializeField]
    private CinemachineVirtualCamera cameraBrain;

    public GameObject[] CarPrefabs;

    void Start()
    {
        int playersSpawned = 0;
        int botsSpawned = 0;
        foreach (GameObject spawnPoint in spawnPoints)
        {
            GameObject carToSpawn = CarPrefabs[Random.Range(0, CarPrefabs.Length)];
            if (playersSpawned < playerAmount)
            {
                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation);
                car.GetComponent<ArcadeCar>().controllable = true;
                playersSpawned++;
                cameraBrain.LookAt = car.transform;
                cameraBrain.Follow = car.transform;
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
}
