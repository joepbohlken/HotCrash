using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionSlot : MonoBehaviour
{
    public Animator canvasAnimator;
    public Transform carParent;
    public TextMeshProUGUI carNameText;
    public GameObject changeColor;
    public Image changeColorKey;
    public float rotationSpeed = 100f;

    [HideInInspector]
    public MainMenu menu = null;
    private Animator animator;
    private ParticleSystem dustParticleSystem;

    [HideInInspector]
    public PlayerController player;

    private int currentCar = 0;
    private int actualColor = 0;

    [HideInInspector]
    public string carName;
    [HideInInspector]
    public Material carColor;

    [HideInInspector]
    public bool interactable = false;
    [HideInInspector]
    public float h = 0f;
    [HideInInspector]
    public bool ready = false;

    private void Start()
    {
        animator = GetComponent<Animator>();
        dustParticleSystem = GetComponentInChildren<ParticleSystem>();
    }

    // Update is called once per frame
    private void Update()
    {
        RotateCar();
    }

    public void PlayParticleSystem()
    {
        dustParticleSystem.Play();
    }

    public void NextCarLeft()
    {
        // Return if animation is still playing
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("CarSelector_ChangeCar"))
        {
            return;
        }

        // Destroy old car game object
        if (carParent.childCount > 0)
        {
            Destroy(carParent.GetChild(0).gameObject);
        }

        if (currentCar == 0)
        {
            currentCar = menu.availableCars.Length - 1;
        }
        else
        {
            currentCar--;
        }

        GameObject car = menu.availableCars[currentCar];
        Instantiate(car, carParent);

        carName = car.name;

        animator.Play("CarSelector_ChangeCar");
        carNameText.text = car.name;

    }

    public void NextCarRight()
    {
        // Return if animation is still playing
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("CarSelector_ChangeCar"))
        {
            return;
        }

        // Destroy old car game object
        if (carParent.childCount > 0)
        {
            Destroy(carParent.GetChild(0).gameObject);
        }

        if (currentCar == menu.availableCars.Length - 1)
        {
            currentCar = 0;
        }
        else
        {
            currentCar++;
        }

        GameObject car = menu.availableCars[currentCar];
        Instantiate(car, carParent);

        carName = car.name;

        animator.Play("CarSelector_ChangeCar");
        carNameText.text = car.name;

    }

    private void RotateCar()
    {
        carParent.transform.localEulerAngles += Vector3.up * Time.deltaTime * h * rotationSpeed;
    }

    public void ChangeColor()
    {

        if (actualColor == menu.availableColors.Length - 1)
        {
            actualColor = 0;
        }
        else
        {
            actualColor++;
        }

        Material newColor = menu.availableColors[actualColor];

        carParent.GetChild(0).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = newColor;
        carColor = newColor;
    }

    public void Ready()
    {
        canvasAnimator.Play("ReadyPlayer");
        ready = true;
        interactable = false;

        menu.CheckStartGame();
    }

    public void UnReady()
    {
        canvasAnimator.Play("UnreadyPlayer");
        ready = false;
        interactable = true;
    }

    public void ResetSlot()
    {
        ready = false;
        interactable = false;

        canvasAnimator.CrossFade("UnreadyPlayer", 0, 0, 1);

        currentCar = 0;
        actualColor = 0;

        if (carParent.childCount > 0)
        {
            Destroy(carParent.GetChild(0).gameObject);
        }

        GameObject car = menu.availableCars[currentCar];
        Instantiate(car, carParent);

        carName = car.name;
        carNameText.text = carName;
        carColor = menu.availableColors[actualColor];
    }
}
