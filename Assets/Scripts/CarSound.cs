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

    [Header("--- Pitch ---")]
    [Tooltip("Y - Pitch. X - Vehicle speed (km/h)")]
    [SerializeField] private AnimationCurve pitchCurve;

    private AudioSource engineAudioSource;
    private AudioSource driftSource;

    private int currentGear = 1;
    private float pitchRate;
    private bool isDrifting = false;
    private bool isSetUp = false;

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

    public void SetUpSources(ArcadeCar car)
    {
        arcadeCar = car;

        // Engine source
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.volume = engineVolume;
        engineAudioSource.loop = true;
        engineAudioSource.spatialBlend = 1f;
        engineAudioSource.minDistance = 15;
        engineAudioSource.maxDistance = 50;

        // Drifting source
        driftSource = gameObject.AddComponent<AudioSource>();
        driftSource.clip = driftingClip;
        driftSource.loop = true;
        driftSource.spatialBlend = 1f;
        engineAudioSource.minDistance = 15;
        driftSource.maxDistance = 50;
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
                    engineAudioSource.PlayOneShot(gearShiftSFX[Random.Range(0, gearShiftSFX.Count)], gearShiftVolume);

                    // Changes Engine Gear Sound
                    engineAudioSource.clip = gear.audioClip;
                    engineAudioSource.Play();
                }
            }
        }
    }

    public void PlayDriftSound()
    {
        if (!isDrifting && isSetUp)
        {
            isDrifting = true;
            driftSource.volume = driftVolume;
            driftSource.Play();
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
            driftSource.volume = Mathf.Lerp(driftVolume, 0, i);
        }
            driftSource.Stop();
    }
}
