using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour
{
    public GameMaster gameMaster;

    public Sprite keyboardIcon;
    public Sprite gamepadIcon;

    [Header("UI Elements")]
    public GameObject overlay;
    public GameObject[] slots;

    private void Update()
    {
        foreach (PlayerInput player in gameMaster.players)
        {
            if (player.GetComponent<PlayerController>().startGame)
            {
                gameMaster.LoadScene("TestScene");
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SetOverlay(false);
            gameMaster.DisablePlayerJoining();
        }

        UpdatePlayerSlots();
    }

    private void SetOverlay(bool state)
    {
        overlay.SetActive(state);
    }

    public void GetPlayers()
    {
        SetOverlay(true);
        gameMaster.EnablePlayerJoining();
    }

    private void UpdatePlayerSlots()
    {
        foreach (GameObject slot in slots)
        {
            slot.transform.GetChild(0).gameObject.SetActive(true);
            slot.transform.GetChild(1).gameObject.SetActive(false);
        }

        foreach (var (player, i) in gameMaster.players.Select((value, i) => (value, i)))
        {
            bool keyboardPlayer = player.currentControlScheme == "Keyboard";

            slots[i].transform.GetChild(0).gameObject.SetActive(false);
            slots[i].transform.GetChild(1).GetComponent<Image>().sprite = keyboardPlayer ? keyboardIcon : gamepadIcon;
            slots[i].transform.GetChild(1).GetComponent<RectTransform>().sizeDelta = keyboardPlayer ? new Vector2(200, 100) : new Vector2(200, 150);
            slots[i].transform.GetChild(1).gameObject.SetActive(true);
        }
    }
}
