using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public void changeScene(string name)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(name);
    }
}
