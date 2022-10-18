using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public static int playerAmount;

    public void changeScene(string name)
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(name);
    }

    public void changeSceneMultiplayer(int PlayerAmount)
    {
        Time.timeScale = 1;
        playerAmount = PlayerAmount;
        SceneManager.LoadScene("MapScene");
    }
}
