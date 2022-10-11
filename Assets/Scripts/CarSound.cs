using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    private AudioSource audioSource;

    public int currentGear = 1;
    public List<Gear> gears = new List<Gear>();



    [Header("Sounds")]
    [Tooltip("Y - Pitch. X - Vehicle speed (km/h)")]
    public AnimationCurve pitchCurve;

    private float pitchRate;

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
        audioSource = GetComponent<AudioSource>();

    }

    void Update()
    {
        SetEngineSound();
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
                    audioSource.clip = gear.audioClip;
                    audioSource.Play();
                }
            }
        }
    }
}
