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
    public GameObject titleScreenGameObject;
    public DoorObject holdingAreaDoor;
    public DoorObject initialAreaDoor;

    //[SerializeField]
    private byte maxPlayersPerRoom = 4;

    public delegate void PlayerCountUpdatedCallback();
    public event PlayerCountUpdatedCallback onPlayerCountUpdated;

    private void Awake()
    {
        singleton = this;

        PhotonNetwork.AutomaticallySyncScene = true;
        titleScreenGameObject.SetActive(true);
    }

    [ContextMenu("Force Connect")]
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
        {
            // #Critical we need at this point to attempt joining a Random Room. If it fails, we'll get notified in OnJoinRandomFailed() and we'll create one.
            ExitGames.Client.Photon.Hashtable openRoom = new ExitGames.Client.Photon.Hashtable() { ["GameActive"] = false };
            PhotonNetwork.JoinRandomRoom(openRoom, maxPlayersPerRoom);
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
        titleScreenGameObject.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room -- PUN");
        titleScreenGameObject.SetActive(false);

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
    public void RouteFailed_RPC()
    {
        StartCoroutine(ResetAfterFailure());
    }

    IEnumerator ResetAfterFailure()
    {
        yield return new WaitForSeconds(2.0f);
        GameManager.singleton.roomManager.RouteFinished();
    }

    [PunRPC]
    public void StartGame_RPC()
    {
        StartCoroutine(StartGameRoutine());

        if (photonView.IsMine)
        {
            NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();
            allPlayers[Random.Range(0, allPlayers.Length)].photonView.RPC(nameof(NetworkPlayer.SetNarrator_RPC), RpcTarget.All);
        }
    }

    IEnumerator StartGameRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        for (int i = 5; i > 0; i--)
        {
            GameManager.singleton.notification.PlayNotification("Game Starting: " + i);
            yield return new WaitForSeconds(1.0f);
        }
        initialAreaDoor.OpenDoor();

        if (PhotonNetwork.PlayerListOthers.Length <= 0) 
        {
            holdingAreaDoor.OpenDoor();
        }
    }
}
