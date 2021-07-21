using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public Recorder primaryRecorder;
    public Transform startTransform;
    public GameObject connectOnlineButton;

    //[SerializeField]
    private byte maxPlayersPerRoom = 15;
    private void Awake()
    {
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
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("Failed To Join Room. | " + message + " | Creating Room Now! -- PUN");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = maxPlayersPerRoom });
    }

    //public override void OnRoomListUpdate(List<RoomInfo> roomList)
    //{
    //    base.OnRoomListUpdate(roomList);
    //    foreach(RoomInfo roomInfo in roomList)
    //    {
    //        print(roomInfo.Name);
    //    }
    //}
    #endregion
}
