using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectionSlot : MonoBehaviour
{
    public Transform carParent;
    public TextMeshProUGUI carNameText;
    public Image changeColorKey;

    [HideInInspector]
    public MainMenu menu = null;
    private Animator animator;
    private ParticleSystem dustParticleSystem;

    private int currentCar = 0;
    private int actualColor = 0;

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
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            NextCarLeft();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            NextCarRight();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            ChangeColor();
        }
    }

    public void PlayParticleSystem()
    {
        dustParticleSystem.Play();
    }

    private void NextCarLeft()
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
            currentCar = menu.availableCars.Length;
        }
        else
        {
            currentCar--;
        }

        GameObject car = menu.availableCars[currentCar];
        Instantiate(car, carParent);

        animator.Play("CarSelector_ChangeCar");
        carNameText.text = car.name;

    }

    private void NextCarRight()
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

        if (currentCar == menu.availableCars.Length)
        {
            currentCar = 0;
        }
        else
        {
            currentCar++;
        }

        GameObject car = menu.availableCars[currentCar];
        Instantiate(car, carParent);

        animator.Play("CarSelector_ChangeCar");
        carNameText.text = car.name;

    }

    private void RotateCar()
    {

    }

    private void ChangeColor()
    {

        if (actualColor == 4)
        {
            actualColor = 0;
        }
        else
        {
            actualColor++;
        }


        carParent.GetChild(0).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = menu.availableColors[actualColor];
    }

    public void ResetSlot()
    {
        currentCar = 0;
        actualColor = 0;

        if (carParent.childCount > 0)
        {
            Destroy(carParent.GetChild(0).gameObject);
        }

        GameObject car = menu.availableCars[currentCar];
        Instantiate(car, carParent);
    }
}
