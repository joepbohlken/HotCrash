using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    [Header("--- Audio Clips ---")]
    [SerializeField] private AudioClip carStartClip;
    [SerializeField] private AudioClip driftingClip;
    [SerializeField] private List<AudioClip> gearShiftSFX = new List<AudioClip>();
    [SerializeField] private List<AudioClip> crashingSFX = new List<AudioClip>();
    [SerializeField] private List<AudioClip> deathSFX = new List<AudioClip>();

    [Header("--- Gears ---")]
    [SerializeField] private List<Gear> gears = new List<Gear>();

    [Header("--- Volumes ---")]
    [Range(0, 1)]
    [SerializeField] private float engineStartVolume;
    [Range(0, 1)]
    [SerializeField] private float engineVolume;
    [Range(0, 1)]
    [SerializeField] private float gearShiftVolume;
    [Range(0, 1)]
    [SerializeField] private float driftVolume;
    [Range(0, 1)]
    [SerializeField] private float crashVolume;
    [Range(0, 1)]
    [SerializeField] private float deathVolume;
    [Range(0, 1)]
    [SerializeField] private float mineVolume;


    [Header("--- Pitch ---")]
    [Tooltip("Y - Pitch. X - Vehicle speed (km/h)")]
    [SerializeField] private AnimationCurve pitchCurve;

    private AudioSource engineAudioSource;
    private AudioSource driftAudioSource;
    private AudioSource crashAudioSource;
    private AudioSource deathAudioSource;
    private AudioSource mineAudioSource;

    private int currentGear = 1;
    private float pitchRate;
    private bool isDrifting = false;
    private bool isSetUp = false;
    private bool isBot = true;

    private CarController carController;

    [Serializable]
    public class Gear
    {
        public string name;
        public AudioClip audioClip;
        [Range(-100, 250)]
        public float minSpeed;
        [Range(-100, 250)]
        public float maxSpeed;
        [Range(0.1f, 2f)]
        public float minAudioPitch;
        [Range(0.1f, 2f)]
        public float maxAudioPitch;
    }

    public void SetUpSources(CarController car, CarDeformation carDeformation)
    {
        carController = car;
        isBot = car.isBot;

        // Engine source
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.clip = gears[1].audioClip;
        engineAudioSource.volume = engineVolume;
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.minDistance = 15;
        engineAudioSource.maxDistance = 50;

        // Drifting source
        driftAudioSource = gameObject.AddComponent<AudioSource>();
        driftAudioSource.clip = driftingClip;
        driftAudioSource.loop = true;
        driftAudioSource.spatialBlend = 1f;
        driftAudioSource.minDistance = 15;
        driftAudioSource.maxDistance = 50;

        // Crash sound
        crashAudioSource = gameObject.AddComponent<AudioSource>();
        crashAudioSource.playOnAwake = false;
        crashAudioSource.loop = false;
        crashAudioSource.volume = crashVolume;
        crashAudioSource.spatialBlend = 1f;
        crashAudioSource.minDistance = 15;
        crashAudioSource.maxDistance = 50;
        carDeformation.onCrash.AddListener(PlayCrashSound);

        // Death sound
        deathAudioSource = gameObject.AddComponent<AudioSource>();
        deathAudioSource.playOnAwake = false;
        deathAudioSource.loop = false;
        deathAudioSource.volume = deathVolume;
        deathAudioSource.spatialBlend = 1f;
        deathAudioSource.minDistance = 15;
        deathAudioSource.maxDistance = 50;
        car.health.onDestroyed.AddListener(PlayDeathSound);

        // Mine sound
    }

    public void StartSounds()
    {
        engineAudioSource.PlayOneShot(carStartClip, engineStartVolume);
        isSetUp = true;
    }

    void Update()
    {
        if (!isSetUp) return;
        SetEngineSound();
    }

    public void SetEngineSound()
    {
        // TODO FIX FOR NEW CAR CONTROLLER

        //float speed = carController.currentSpeed * (carController.carConfig.speedType == SpeedType.KPH ? C.KPHMult : C.MPHMult);

        Debug.Log(engineAudioSource.pitch);
        

        if (carController.convertedCurrentSpeed >= 0)
        {
            pitchRate = 0;
            engineAudioSource.pitch = pitchCurve.Evaluate(carController.convertedCurrentSpeed) / 100;
        }
        else if (engineAudioSource.pitch != 1)
        {
            pitchRate += Time.fixedDeltaTime / 10;
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 1f, pitchRate);
        }

        foreach (var gear in gears)
        {
            if (carController.convertedCurrentSpeed < gear.maxSpeed && carController.convertedCurrentSpeed > gear.minSpeed)
            {
                int temp = gears.IndexOf(gear);
                if (temp != currentGear)
                {
                    currentGear = temp;
                    // Plays Gearshift sfx
                    if (!isBot)
                    {
                        engineAudioSource.PlayOneShot(gearShiftSFX[Random.Range(0, gearShiftSFX.Count)], gearShiftVolume);
                    }

                    // Changes Engine Gear Sound
                    engineAudioSource.clip = gear.audioClip;
                    engineAudioSource.Play();
                }
            }
        }
    }

    public void PlayDeathSound()
    {
        deathAudioSource.clip = deathSFX[Random.Range(0, deathSFX.Count)];
        deathAudioSource.pitch = Random.Range(0.8f, 1.2f);
        deathAudioSource.Play();
    }

    public void PlayMineSound()
    {

    }

    public void PlayCrashSound()
    {
        crashAudioSource.clip = crashingSFX[Random.Range(0, crashingSFX.Count)];
        crashAudioSource.pitch = Random.Range(0.8f, 1.2f);
        crashAudioSource.Play();
    }

    public void PlayDriftSound()
    {
        if (!isDrifting && isSetUp)
        {
            isDrifting = true;
            driftAudioSource.volume = driftVolume;
            driftAudioSource.Play();
            StopAllCoroutines();
        }
    }

    public void StopDriftSound()
    {
        if (isDrifting && isSetUp)
        {
            isDrifting = false;
            StartCoroutine(LerpVolume());
        }
    }

    public IEnumerator LerpVolume()
    {
        for (float i = 0f; i < 1f; i+= 0.1f)
        {
            yield return new WaitForSeconds(0.05f);
            driftAudioSource.volume = Mathf.Lerp(driftVolume, 0, i);
        }
            driftAudioSource.Stop();
    }
}
