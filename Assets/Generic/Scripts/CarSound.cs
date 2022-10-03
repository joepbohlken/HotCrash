using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour
{
    [Serializable]
    protected class Gears 
    {
        public string name;
        public AudioClip audioClip;
        [Range(0.1f, 2f)]
        public float minAudioPitch;
        [Range(0.1f, 2f)]
        public float maxAudioPitch;
    }

    [SerializeField] private List<Gears> gears = new List<Gears>();

    [Range(0.75f, 1.5f)]
    public float audioPitch = 0.75f;
    public AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.pitch = audioPitch;
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.pitch = audioPitch;
    }
}
