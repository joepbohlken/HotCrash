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
    private CinemachineVirtualCamera cameraBrainPrefab;
    [SerializeField]
    private Transform carContainer;

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
                CinemachineVirtualCamera cameraBrain = Instantiate(cameraBrainPrefab);
                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation, carContainer);
                car.GetComponent<ArcadeCar>().controllable = true;
                playersSpawned++;
                cameraBrain.LookAt = car.transform;
                cameraBrain.Follow = car.transform;

                HUD hud = FindObjectOfType<HUD>();
                CarHealth carHealth = car.GetComponent<CarHealth>();
                carHealth.healthBars.Add(hud.bars);
                carHealth.healthTexts.Add(hud.hpText);

                car.GetComponent<AbilityController>().hud = hud;

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

                continue;
            }
            if(botsSpawned < botAmount)
            {

                GameObject car = Instantiate(carToSpawn, spawnPoint.transform.position + new Vector3(0, 1, 0), spawnPoint.transform.rotation, carContainer);
                car.GetComponent<ArcadeCar>().controllable = false;

                CarAI carAI = car.AddComponent<CarAI>();
                carAI.boxSize = new Vector3(2, 0.4f, 5);
                carAI.InitializeAI();
                
                botsSpawned++;
            }
            if(playersSpawned == playerAmount && botsSpawned == botAmount)
            {
                return;
            }
        }
    }
}
