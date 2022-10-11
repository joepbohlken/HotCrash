using System;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    private float pitchRate;
    private ArcadeCar arcadeCar;
    private AudioSource gearShiftSource;

    private AudioSource driftSource;
    public AudioClip driftingClip;
    [Range(0, 1)]
    public float driftVolume;
    private bool isDrifting = false;

    [Range(0, 1)]
    public float gearShiftVolume;
    private AudioSource audioSource;
    public int currentGear = 1;
    public List<Gear> gears = new List<Gear>();
    public List<AudioClip> gearShiftSFX = new List<AudioClip>();
    [Header("Sounds")]
    [Tooltip("Y - Pitch. X - Vehicle speed (km/h)")]
    public AnimationCurve pitchCurve;


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
        audioSource = GetComponent<AudioSource>();
        gearShiftSource = gameObject.AddComponent<AudioSource>();
        gearShiftSource.volume = gearShiftVolume;
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
            audioSource.pitch = pitchCurve.Evaluate(speed) / 100;
        }
        else if (audioSource.pitch != 1)
        {
            pitchRate += Time.fixedDeltaTime / 10;
            audioSource.pitch = Mathf.Lerp(audioSource.pitch, 1f, pitchRate);
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

                    // Changes Engine Sound
                    audioSource.clip = gear.audioClip;
                    audioSource.Play();
                }
            }
        }
    }

    public void PlayDriftSound()
    {
        if (arcadeCar.isHandBrakeNow && !isDrifting)
        {
            isDrifting = true;
            Debug.Log("start playing");
            driftSource.volume = driftVolume;
            driftSource.Play();
            StopAllCoroutines();
        }
        else if (!arcadeCar.isHandBrakeNow && isDrifting)
        {
            isDrifting = false;
            Debug.Log("stop playing");
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
