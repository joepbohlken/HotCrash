using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [SerializeField]
    public AudioMixer audioMixer;
    [SerializeField]
    [Range(0, -80f)]
    public float volume;
    private void Update()
    {
        setVolume(volume);
    }
    public void setVolume(float volume)
    {
        Debug.Log(volume);
        audioMixer.SetFloat("volume", volume);
    }
}
