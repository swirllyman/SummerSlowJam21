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
    public Transform startTransform;
    
    public GameObject titleScreenGameObject;
    
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
        yield return new WaitForSeconds(.5f);
        titleScreenGameObject.SetActive(true);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined Room -- PUN");
        titleScreenGameObject.SetActive(false);

        if (playerPrefab != null && NetworkPlayer.LocalPlayer == null)
        {
            PhotonNetwork.Instantiate(playerPrefab.name, startTransform.position, Quaternion.identity, 0);
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
}
