using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class RoomManager : MonoBehaviourPun
{
    public Routes[] routes;
    public Color highlightColor;
    Task currentTask;

    [PunRPC]
    public void SetTaskHighlight(int route, int room, int switchID)
    {
        if(currentTask != null)
        {
            currentTask.selectionRend.sprite = currentTask.standardSprite;
            if (NetworkPlayer.LocalPlayer.narrator)
            {
                currentTask.selectionRend.color = currentTask.sabotaged ? Color.red : Color.green;
            }
            else
            {
                currentTask.selectionRend.enabled = false;
            }
        }

        currentTask = routes[route].rooms[room].allTasks[switchID];

        currentTask.selectionRend.sprite = currentTask.selectionSprite;
        currentTask.selectionRend.enabled = true;
        currentTask.selectionRend.color = highlightColor;
    }

    [PunRPC]
    public void SetSwitch(int route, int room, int switchID, bool sabotaged)
    {
        routes[route].rooms[room].allTasks[switchID].sabotaged = sabotaged;

        bool allSabotaged = true;
        foreach(Task t in routes[route].rooms[room].allTasks)
        {
            if (!t.sabotaged)
            {
                allSabotaged = false;
            }
        }

        if (allSabotaged)
        {
            GameManager.singleton.notification.PlayNotification("Can't Sabotage All Tasks");

            int nextSwitchID = (switchID + 1) % 2;

            routes[route].rooms[room].allTasks[nextSwitchID].sabotaged = false;
            routes[route].rooms[room].allTasks[nextSwitchID].selectionRend.color = Color.green;
        }
    }

    [PunRPC]
    public void FinishRoom_RPC(int route, int roomNum)
    {
        if (routes[route].rooms[roomNum].doorToNextRoom != null)
        {
            routes[route].rooms[roomNum + 1].roomStarted = true;
            routes[route].rooms[roomNum].OpenDoor();
        }
        else
        {
            GameManager.singleton.notification.PlayNotification("Route Finished");
            RouteFinished();
        }
    }

    public void ResetRooms()
    {
        for (int i = 0; i < routes.Length; i++)
        {
            for (int j = 0; j < routes[i].rooms.Length; j++)
            {
                routes[i].rooms[j].ResetRoom();
            }
        }
    }

    public void RouteFinished()
    {
        ResetRooms();
        if (NetworkPlayer.LocalPlayer.dead)
        {
            NetworkPlayer.LocalPlayer.photonView.RPC(nameof(NetworkPlayer.Revive_RPC), RpcTarget.All);
        }
        NetworkPlayer.LocalPlayer.transform.position = NetworkManager.singleton.startTransform.position;
        NetworkPlayer.LocalPlayer.LocalPlayerRouteFinished();

        Hashtable activeState = new Hashtable() { ["GameActive"] = false };
        PhotonNetwork.CurrentRoom.SetCustomProperties(activeState);

        NetworkManager.singleton.holdingAreaDoor.CloseDoor();
        NetworkManager.singleton.initialAreaDoor.CloseDoor();

        GameManager.singleton.centerConsole.readyButton.SetActive(true);

        if (photonView.IsMine)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }
}

[System.Serializable]
public struct Routes
{
    public string routeName;
    public Transform routeTransform;
    public Room[] rooms;
}