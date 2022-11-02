using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private GameObject carSoundObjectPrefab;
    [SerializeField] private GameObject oneShotObjectPrefab;
    [SerializeField] private Transform carContainer;

    private LevelManager levelManager;

    ///<summary>Item1: car, Item2: sound</summary>
    private List<Tuple<GameObject, CarSound>> players = new List<Tuple<GameObject, CarSound>>();
    ///<summary>Item1: car, Item2: sound</summary>
    private List<Tuple<GameObject, CarSound>> bots = new List<Tuple<GameObject, CarSound>>();
    private List<(GameObject, Vector3)> oneShots = new List<(GameObject, Vector3)>();

    private bool isInitialized = false;
    public static AudioController main { get; private set; }

    private void Awake()
    {
        main = this;

        levelManager = FindObjectOfType<LevelManager>();
        levelManager.onCarsInitialized.AddListener(InitializeSoundObjects);
    }

    private void InitializeSoundObjects()
    {
        levelManager.onCarsInitialized.RemoveListener(InitializeSoundObjects);

        for (int i = 0; i < carContainer.childCount; i++)
        {
            CarController car = carContainer.GetChild(i).GetComponent<CarController>();
            CarEffects effects = car.GetComponent<CarEffects>();

            CarSound soundObject = Instantiate(carSoundObjectPrefab, transform).GetComponent<CarSound>();
            soundObject.SetUpSources(car, car.GetComponent<CarDeformation>());
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
        UpdateOneShotPositions();
    }

    private void UpdateBotPositions()
    {
        foreach (Tuple<GameObject, CarSound> botData in bots)
        {
            botData.Item2.transform.localPosition = GetPosition(botData.Item1.transform.position);
        }
    }

    private void UpdateOneShotPositions()
    {
        foreach((GameObject, Vector3) oneShotData in oneShots)
        {
            oneShotData.Item1.transform.localPosition = GetPosition(oneShotData.Item2);
        }
    }

    private Vector3 GetPosition(Vector3 soundPosition)
    {
        float closestDistance = 999f;
        Vector3 direction = Vector3.zero;

        foreach (Tuple<GameObject, CarSound> playerData in players)
        {
            float distance = (playerData.Item1.transform.position - soundPosition).magnitude;
            if (distance < closestDistance)
            {
                closestDistance = distance;
                direction = (soundPosition - playerData.Item1.transform.position).normalized;
            }
        }

        return direction * closestDistance;
    }

    public void PlayOneShot(Vector3 soundPosition, AudioClip clip, float pitch, float volume)
    {
        AudioSource oneShotObject = Instantiate(oneShotObjectPrefab, transform).GetComponent<AudioSource>();
        oneShotObject.transform.localPosition = GetPosition(soundPosition);

        oneShotObject.clip = clip;
        oneShotObject.pitch = pitch;
        oneShotObject.volume = volume;
        oneShotObject.Play();

        oneShots.Add((oneShotObject.gameObject, soundPosition));

        StartCoroutine(OneShotTimer(oneShotObject.gameObject, soundPosition, clip.length));
    }

    private IEnumerator OneShotTimer(GameObject oneShotObject, Vector3 soundPosition, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        oneShots.Remove((oneShotObject, soundPosition));
        Destroy(oneShotObject);
    }
}
