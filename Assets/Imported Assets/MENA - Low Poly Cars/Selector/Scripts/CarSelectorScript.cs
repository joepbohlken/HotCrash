using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarSelectorScript : MonoBehaviour
{
    public Animator main_Camera_Animator, camera_Container_Animator, car_Container_Animator;
    public GameObject carListGameobject;
    public Text carNameText;
    public Material ColorVar_1, ColorVar_2, ColorVar_3, ColorVar_4, ColorVar_5;
    int totalCars;
    int currentCar = 0;
    int actualColor = 0;
    // Start is called before the first frame update
    void Start()
    {
      totalCars = carListGameobject.transform.childCount - 1;
    }

    public void NextCarLeft(){

      if(actualColor != 0){
        actualColor = 0;
        carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_1;
      }
      carListGameobject.transform.GetChild(currentCar).gameObject.SetActive(false);

      if(currentCar == 0){
        currentCar = totalCars;
      }else{
        currentCar --;
      }

      carListGameobject.transform.GetChild(currentCar).gameObject.SetActive(true);
      car_Container_Animator.Play("Cars_Container_Car_Change_Anim", -1, 0f);
      main_Camera_Animator.Play("Main_Camera_Car_Change_Anim", -1, 0f);
      carNameText.text = carListGameobject.transform.GetChild(currentCar).gameObject.name;

    }

    public void NextCarRight(){

      if(actualColor != 0){
        actualColor = 0;
        carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_1;
      }
      carListGameobject.transform.GetChild(currentCar).gameObject.SetActive(false);

      if(currentCar == totalCars){
        currentCar = 0;
      }else{
        currentCar ++;
      }

      carListGameobject.transform.GetChild(currentCar).gameObject.SetActive(true);
      car_Container_Animator.Play("Cars_Container_Car_Change_Anim", -1, 0f);
      main_Camera_Animator.Play("Main_Camera_Car_Change_Anim", -1, 0f);
      carNameText.text = carListGameobject.transform.GetChild(currentCar).gameObject.name;

    }

    public void changeColor(){

      if(actualColor == 4){
        actualColor = 0;
      }else{
        actualColor ++;
      }

      switch(actualColor){
        case 0:
            carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_1;
        break;

        case 1:
            carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_2;
        break;

        case 2:
            carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_3;
        break;

        case 3:
            carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_4;
        break;

        case 4:
            carListGameobject.transform.GetChild(currentCar).gameObject.transform.Find("Body").gameObject.GetComponent<Renderer>().material = ColorVar_5;
        break;
      }

    }

}
