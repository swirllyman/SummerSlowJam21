using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;

public class CenterConsole : InteractionArea
{
    public TMP_Text centerConsoleText;
    public GameObject readyButton;
    public GameObject[] uiPanels;

    public int playersRequiredToStart = 2;

    public override void Start()
    {
        NetworkManager.singleton.onPlayerCountUpdated += UpdateCenterConsoleText;
        ToggleArea(false);
    }

    private void OnDestroy()
    {
        NetworkManager.singleton.onPlayerCountUpdated += UpdateCenterConsoleText;
    }

    public override void ToggleArea(bool toggle)
    {
        base.ToggleArea(toggle);
        if (PhotonNetwork.IsConnected && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameActive") && (bool)PhotonNetwork.CurrentRoom.CustomProperties["GameActive"])
        {
            if (toggle)
            {
                uiPanels[1].SetActive(true);
                uiPanels[0].SetActive(false);
            }
        }
        else
        {
            if (toggle)
            {
                uiPanels[1].SetActive(false);
                uiPanels[0].SetActive(true);
                UpdateCenterConsoleText();
            }
        }
    }

    public void UpdateCenterConsoleText()
    {
        if (!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("GameActive") || !(bool)PhotonNetwork.CurrentRoom.CustomProperties["GameActive"])
        {

            if(PhotonNetwork.PlayerList.Length < playersRequiredToStart)
            {
                centerConsoleText.text = "Waiting For More Players. (Required at least 2)";
                readyButton.SetActive(false);
                return;
            }
            else
            {
                bool playerReady = false;
                if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("Ready"))
                {
                    bool isPlayerReady = (bool)PhotonNetwork.LocalPlayer.CustomProperties["Ready"];
                    if (!isPlayerReady)
                    {
                        playerReady = true;
                    }
                }

                if (!playerReady)
                {
                    readyButton.SetActive(true);
                }
            }

            int readyPlayers = 0;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.ContainsKey("Ready"))
                {
                    bool playerReady = (bool)p.CustomProperties["Ready"];
                    if (playerReady)
                        readyPlayers++;
                }
            }

            centerConsoleText.text = "Waiting for (" + (PhotonNetwork.CurrentRoom.PlayerCount - readyPlayers) + ") Players";

            if (readyPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
            {
                if (NetworkManager.singleton.photonView.IsMine)
                {
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    GameManager.singleton.roomManager.photonView.RPC(nameof(RoomManager.StartGame_RPC), RpcTarget.All);
                    Hashtable activeState = new Hashtable() { ["GameActive"] = true };
                    PhotonNetwork.CurrentRoom.SetCustomProperties(activeState);
                }
            }
        }
    }

    public void ReadyButtonPressed()
    {
        readyButton.SetActive(false);
        NetworkPlayer.LocalPlayer.LocalPlayerReady();
        canvasObject.SetActive(false);
    }
}