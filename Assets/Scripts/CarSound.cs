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


    [Header("--- Pitch ---")]
    [Tooltip("Y - Pitch. X - Vehicle speed (km/h)")]
    [SerializeField] private AnimationCurve pitchCurve;

    private AudioSource engineAudioSource;
    private AudioSource driftAudioSource;
    private AudioSource crashAudioSource;

    private int currentGear = 1;
    private float pitchRate;
    private bool isDrifting = false;
    private bool isSetUp = false;
    private bool isBot = true;

    private ArcadeCar arcadeCar;

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

    public void SetUpSources(ArcadeCar car, CarDeformation carDeformation)
    {
        arcadeCar = car;
        isBot = arcadeCar.isBot;

        // Engine source
        engineAudioSource = gameObject.AddComponent<AudioSource>();
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
        engineAudioSource.minDistance = 15;
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
        float speed =  arcadeCar.GetSpeed() * 3.6f;

        if (arcadeCar.isAcceleration || arcadeCar.isReverseAcceleration)
        {
            pitchRate = 0;
            engineAudioSource.pitch = pitchCurve.Evaluate(speed) / 100;
        }
        else if (engineAudioSource.pitch != 1)
        {
            pitchRate += Time.fixedDeltaTime / 10;
            engineAudioSource.pitch = Mathf.Lerp(engineAudioSource.pitch, 1f, pitchRate);
        }

        foreach (var gear in gears)
        {
            if (speed < gear.maxSpeed && speed > gear.minSpeed)
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

    public void PlayCrashSound()
    {
        crashAudioSource.clip = crashingSFX[Random.Range(0, crashingSFX.Count)];
        crashAudioSource.pitch = Random.Range(0.8f, 1.2f);
        crashAudioSource.Play();
        //engineAudioSource.PlayOneShot(crashingSFX[Random.Range(0, crashingSFX.Count)], crashVolume);
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
