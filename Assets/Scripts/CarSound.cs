using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    // Car Start Audio Variables
    private AudioSource carStartSource;
    public AudioClip carStartClip;


    // Engine Audio Variables
    private AudioSource engineAudioSource;
    [Range(0, 1)]
    public float engineVolume;
    private float pitchRate;
    [Header("Sounds")]
    [Tooltip("Y - Pitch. X - Vehicle speed (km/h)")]
    public AnimationCurve pitchCurve;

    // Gear Audio Variables
    public List<Gear> gears = new List<Gear>();
    [Range(0, 1)]
    public float gearShiftVolume;
    public int currentGear = 1;
    public List<AudioClip> gearShiftSFX = new List<AudioClip>();
    private AudioSource gearShiftSource;

    // Drfiting Audio Variables
    private AudioSource driftSource;
    public AudioClip driftingClip;
    [Range(0, 1)]
    public float driftVolume;
    private bool isDrifting = false;

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

    void Start()
    {
        arcadeCar = GetComponent<ArcadeCar>();

        // Car start sound
        carStartSource = gameObject.AddComponent<AudioSource>();
        carStartSource.clip = carStartClip;
        carStartSource.volume = engineVolume;
        carStartSource.Play();

        // Engine sound
        engineAudioSource = gameObject.AddComponent<AudioSource>();
        engineAudioSource.volume = engineVolume;

        // Shifting Sound
        gearShiftSource = gameObject.AddComponent<AudioSource>();
        gearShiftSource.volume = gearShiftVolume;

        // Drifting Sound
        driftSource = gameObject.AddComponent<AudioSource>();
        driftSource.clip = driftingClip;
        driftSource.loop = true;
    }

    void Update()
    {
        SetEngineSound();
        PlayDriftSound();
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
                    gearShiftSource.clip = gearShiftSFX[Random.Range(0 ,gearShiftSFX.Count)];
                    gearShiftSource.Play();

                    // Changes Engine Gear Sound
                    engineAudioSource.clip = gear.audioClip;
                    engineAudioSource.Play();
                }
            }
        }
    }

    public void PlayDriftSound()
    {
        if (arcadeCar.isHandBrakeNow && !isDrifting)
        {
            isDrifting = true;
            driftSource.volume = driftVolume;
            driftSource.Play();
            StopAllCoroutines();
        }
        else if (!arcadeCar.isHandBrakeNow && isDrifting)
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
