using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static CarSound;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CarSound : MonoBehaviour
{
    [Serializable]
    public class Gear
    {
        public string name;
        public AudioClip audioClip;
        [Range(0,250)]
        public float minSpeed;
        [Range(0, 250)]
        public float maxSpeed;
        [Range(0.1f, 2f)]
        public float minAudioPitch;
        [Range(0.1f, 2f)]
        public float maxAudioPitch;
    }

    public List<Gear> gears = new List<Gear>();

    // Debugging
    [HideInInspector] public int gearIndex;
    [Header("Debugging")]
    [SerializeField] 
    [Range(0f, 1f)]
    private float _rpm;
    [Range(0, 300)]
    public float speed;

    public AnimationCurve animationCurve; 

    private AudioSource audioSource;
    private Gear currentGear;
    private float t;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        t += Time.deltaTime / 10000f;
        speed = Mathf.Lerp(speed, 200, t);
        SetEngineSound();
    }

    public void SetEngineSound()
    {
        audioSource.pitch = animationCurve.Evaluate(speed)/100;

        foreach (var gear in gears)
        {
            if (speed < gear.maxSpeed && speed > gear.minSpeed)
            {
                currentGear = gear;
                gearIndex = gears.IndexOf(gear);
                audioSource.clip = gear.audioClip;
                audioSource.Play();
            }
        }


        //if (gears.IndexOf(currentGear) != gearNumber)
        //{
        //    currentGear = gears.FirstOrDefault(g => g.name == "Gear" + (gearNumber + 1));
        //    audioSource.clip = currentGear.audioClip;
        //    audioSource.Play();
        //}

        //audioSource.pitch = Mathf.Lerp(currentGear.minAudioPitch, currentGear.maxAudioPitch, rpm);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(CarSound))]
public class GearDropdown : Editor
{
    private List<string> gearList = new List<string>();

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var myScript = target as CarSound;

        gearList.Clear();
        foreach(Gear gear in myScript.gears)
        {
            gearList.Add(gear.name);
        }

        myScript.gearIndex = EditorGUILayout.Popup("Current Gear", myScript.gearIndex, gearList.ToArray());
    }
}
#endif
