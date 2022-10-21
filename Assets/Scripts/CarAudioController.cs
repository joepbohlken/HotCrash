using System;
using System.Collections.Generic;
using UnityEngine;

public class CarAudioController : MonoBehaviour
{
    [SerializeField] private GameObject soundObjectPrefab;
    [SerializeField] private Transform carContainer;

    private GameMaster gameMaster;

    ///<summary>Item1: car, Item2: sound</summary>
    private List<Tuple<GameObject, CarSound>> players = new List<Tuple<GameObject, CarSound>>();
    ///<summary>Item1: car, Item2: sound</summary>
    private List<Tuple<GameObject, CarSound>> bots = new List<Tuple<GameObject, CarSound>>();

    private bool isInitialized = false;

    private void Awake()
    {
        gameMaster = FindObjectOfType<GameMaster>();
        gameMaster.onCarsInitialized.AddListener(InitializeSoundObjects);
    }

    private void InitializeSoundObjects()
    {
        gameMaster.onCarsInitialized.RemoveListener(InitializeSoundObjects);

        for (int i = 0; i < carContainer.childCount; i++)
        {
            ArcadeCar car = carContainer.GetChild(i).GetComponent<ArcadeCar>();
            CarEffects effects = car.GetComponent<CarEffects>();

            CarSound soundObject = Instantiate(soundObjectPrefab, transform).GetComponent<CarSound>();
            soundObject.SetUpSources(car);
            effects.SetSound(soundObject);

            if (car.isBot) bots.Add(Tuple.Create(car.gameObject, soundObject));
            else
            {
                players.Add(Tuple.Create(car.gameObject, soundObject));
                soundObject.transform.localPosition = Vector3.zero;
            }
        }

        UpdateBotPositions();

        foreach (Tuple<GameObject, CarSound> data in players)
        {
            data.Item2.StartSounds();
        }
        foreach (Tuple<GameObject, CarSound> data in bots)
        {
            data.Item2.StartSounds();
        }

        isInitialized = true;
    }

    private void LateUpdate()
    {
        if (!isInitialized) return;
        UpdateBotPositions();
    }

    private void UpdateBotPositions()
    {
        foreach (Tuple<GameObject, CarSound> botData in bots)
        {
            float closestDistance = 999f;

            foreach (Tuple<GameObject, CarSound> playerData in players)
            {
                float distance = (playerData.Item1.transform.position - botData.Item1.transform.position).magnitude;
                if (distance < closestDistance) closestDistance = distance;
            }

            botData.Item2.transform.localPosition = new Vector3(closestDistance, 0, 0);
        }
    }
}
