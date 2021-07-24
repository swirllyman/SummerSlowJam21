using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using Photon.Pun;

public class Customizer : InteractionArea
{
    public static Customizer singleton;
    public static Color[] playerColors;
    public Image currentPlayerImage;
    public TMP_Text playerNameText;
    public TMP_InputField playerInputField;
    public Color[] playerColorOptions;
    public GameObject[] uiPanels;
    public Button[] colorButtons;

    private void Awake()
    {
        singleton = this;
    }

    public override void Start()
    {
        base.Start();
        playerColors = new Color[playerColorOptions.Length];
        for (int i = 0; i < playerColors.Length; i++)
        {
            playerColors[i] = playerColorOptions[i];
        }
    }

    public void UpdateAvailableColors()
    {
        foreach(Button b in colorButtons)
        {
            b.interactable = true;
        }
        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties.ContainsKey("ColorID"))
            {
                colorButtons[(int)p.CustomProperties["ColorID"]].interactable = false;
            }
        }
    }

    public int GetRandomAvailableColor()
    {
        List<int> availableColorIDs = new List<int>();
        for (int i = 0; i < colorButtons.Length; i++)
        {
            if (colorButtons[i].interactable)
            {
                availableColorIDs.Add(i);
            }
        }

        return availableColorIDs[Random.Range(0, availableColorIDs.Count)];
    }

    public override void ToggleArea(bool toggle)
    {
        base.ToggleArea(toggle);
        if (toggle)
        {
            currentPlayerImage.color = NetworkPlayer.LocalPlayer.myRend.color;
        }
    }

    public void TogglePanel(int panelID)
    {
        for (int i = 0; i < uiPanels.Length; i++)
        {
            uiPanels[i].SetActive(i == panelID);
        }
    }

    public void SetPlayerColor(int colorID)
    {
        currentPlayerImage.color = playerColorOptions[colorID];
        NetworkPlayer.LocalPlayer.LocalPlayerSetColor(colorID);
    }

    public void SetPlayerName()
    {
        playerNameText.text = playerInputField.text;
        NetworkPlayer.LocalPlayer.LocalPlayerSetName(playerInputField.text);
    }
}

[System.Serializable]
public struct NetworkColor
{
    public float r, g, b, a;
}