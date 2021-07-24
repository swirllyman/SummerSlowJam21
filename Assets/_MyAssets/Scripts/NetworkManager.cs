using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager singleton;
    public GameObject playerPrefab;
    public Recorder primaryRecorder;
    public Transform startTransform;
    public Transform holdingArea;
    public GameObject connectOnlineButton;
    public DoorObject holdingAreaDoor;
    public DoorObject initialAreaDoor;

    //[SerializeField]
    private byte maxPlayersPerRoom = 15;

    public delegate void PlayerCountUpdatedCallback();
    public event PlayerCountUpdatedCallback onPlayerCountUpdated;

    private void Awake()
    {
        singleton = this;

        PhotonNetwork.AutomaticallySyncScene = true;
        connectOnlineButton.SetActive(true);
    }

    [ContextMenu("Force Connect")]
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            // #Critical, we must first and foremost connect to Photon Online Server.
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public void Disconnect()
    {
        StartCoroutine(DisconnectRoutine());
    }

    #region Pun Callbacks

    #region Connection and Disconnection
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected To Master -- PUN");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Diconnected: " + cause.ToString() + " ||| -- PUN");
        StartCoroutine(DisconnectRoutine());
    }

    IEnumerator DisconnectRoutine()
    {
        primaryRecorder.StopRecording();
        yield return new WaitForSeconds(.5f);
        connectOnlineButton.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room -- PUN");
        connectOnlineButton.SetActive(false);

        if (playerPrefab != null && NetworkPlayer.LocalPlayer == null)
        {
            PhotonNetwork.Instantiate(this.playerPrefab.name, startTransform.position, Quaternion.identity, 0);
            primaryRecorder.StartRecording();
            Customizer.singleton.UpdateAvailableColors();
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed To Join Room. | " + message + " | Creating Room Now! -- PUN");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }
    #endregion

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        onPlayerCountUpdated?.Invoke();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        onPlayerCountUpdated?.Invoke();
    }
    #endregion

    [PunRPC]
    public void StartGame_RPC()
    {
        NetworkPlayer.LocalPlayer.transform.position = holdingArea.position;
        StartCoroutine(StartGameRoutine());
    }

    IEnumerator StartGameRoutine()
    {
        for (int i = 10; i > 0; i--)
        {
            GameManager.singleton.notification.PlayNotification("Game Starting: " + i);
            yield return new WaitForSeconds(1.0f);
        }
        holdingAreaDoor.OpenDoor();
        initialAreaDoor.OpenDoor();
    }
}
