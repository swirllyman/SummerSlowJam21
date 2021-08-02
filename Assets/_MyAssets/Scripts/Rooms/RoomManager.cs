using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class RoomManager : MonoBehaviourPun
{
    public Route[] routes;
    public Color highlightColor;
    internal int currentRoom = 0;
    internal int currentRoute = 0;
    public DoorObject holdingAreaDoor;
    public Transform holdingArea;
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
        routes[route].rooms[roomNum].ToggleAllTasks(false);
        if (routes[route].rooms[roomNum].doorToNextRoom != null)
        {
            StartCoroutine(NextRoomRoutine(route, roomNum));
        }
        else
        {
            GameManager.singleton.notification.PlayNotification("Route Finished");
            RouteFinished();
        }
    }

    System.Collections.IEnumerator NextRoomRoutine(int route, int roomNum)
    {
        GameManager.singleton.notification.PlayNotification("Room "+roomNum+" Complete!");
        yield return new WaitForSeconds(1.5f);
        currentRoom = roomNum + 1;
        if (NetworkPlayer.LocalPlayer.narrator)
        {
            if(currentRoom < routes[currentRoute].rooms.Length)
            {
                GameManager.singleton.notification.PlayNotification("<color=red>Sabotage</color> Phase!");
                GameManager.singleton.camController.SetTarget(routes[currentRoute].rooms[currentRoom].selectionRend.transform);
            }
            GameManager.singleton.camController.ToggleOverviewMode(true, 1.5f);
        }
        yield return new WaitForSeconds(1.0f);
        for (int i = 10; i > 0; i--)
        {
            GameManager.singleton.notification.PlayNotification("Next Room Opening In: " + i);
            yield return new WaitForSeconds(1.0f);
        }

        if (currentRoom < routes[currentRoute].rooms.Length)
        {
            routes[route].rooms[currentRoom].roomStarted = true;
            if(GameManager.singleton.selector.selectedTask != null)
                GameManager.singleton.selector.UpdateButtons();
        }
        
        routes[route].rooms[roomNum].OpenDoor();
    }

    public void ResetRooms()
    {
        for (int i = 0; i < routes.Length; i++)
        {
            routes[i].routeDoor.CloseDoor();
            for (int j = 0; j < routes[i].rooms.Length; j++)
            {
                routes[i].rooms[j].ResetRoom();
            }
        }
        currentRoom = 0;
    }

    public void RouteFinished()
    {
        ResetRooms();
        if (NetworkPlayer.LocalPlayer.dead)
        {
            NetworkPlayer.LocalPlayer.photonView.RPC(nameof(NetworkPlayer.Revive_RPC), RpcTarget.All);
        }
        if (!NetworkPlayer.LocalPlayer.narrator)
        {
            NetworkPlayer.LocalPlayer.transform.position = NetworkManager.singleton.startTransform.position;
            NetworkPlayer.LocalPlayer.LocalPlayerRouteFinished();
        }
        else if (PhotonNetwork.PlayerList.Length <= 1)
        {
            NetworkPlayer.LocalPlayer.transform.position = holdingArea.position;
        }

        GameManager.singleton.roomManager.ToggleAllRouteTasks(false);
        currentRoute = (currentRoute + 1) % routes.Length;
        if (NetworkPlayer.LocalPlayer.narrator)
        {
            GameManager.singleton.roomManager.ToggleAllRouteTasks(true);
        }
        StartCoroutine(StartRouteRoutine(2.5f));
    }

    [PunRPC]
    public void PlayerFinishedLevel_RPC(int viewID)
    {
        PhotonView.Find(viewID).GetComponent<NetworkPlayer>().FinishLevel();

        if (photonView.IsMine)
        {
            bool allFinished = true;
            foreach (NetworkPlayer p in GameManager.singleton.allPlayers)
            {
                if (!p.narrator || !p.levelFinished)
                {
                    allFinished = false;
                    break;
                }
            }

            if (allFinished)
            {
                photonView.RPC(nameof(GameFinished_RPC), RpcTarget.All);
            }
        }
    }

    [PunRPC]
    public void GameFinished_RPC()
    {
        Hashtable activeState = new Hashtable() { ["GameActive"] = false };
        PhotonNetwork.CurrentRoom.SetCustomProperties(activeState);

        holdingAreaDoor.CloseDoor();
        GameManager.singleton.centerConsole.readyButton.SetActive(true);

        if (photonView.IsMine)
        {
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }

        currentRoute = 0;

        StartCoroutine(GameFinishRoutine());
    }

    System.Collections.IEnumerator GameFinishRoutine()
    {
        yield return new WaitForSeconds(2.5f);

        if(GameManager.singleton.allPlayers.Length <= 1)
        {
            GameManager.singleton.notification.PlayNotification("You Win!!");
        }
        else
        {
            NetworkPlayer currentWinner = GameManager.singleton.allPlayers[0];
            for (int i = 1; i < GameManager.singleton.allPlayers.Length; i++)
            {
                if (currentWinner.bankedGold < GameManager.singleton.allPlayers[i].bankedGold)
                {
                    currentWinner = GameManager.singleton.allPlayers[i];
                }
            }
            GameManager.singleton.notification.PlayNotification(currentWinner == NetworkPlayer.LocalPlayer ? "You Win!!" : currentWinner.playerNameText.text);
        }

        GameManager.singleton.roomManager.ToggleAllRouteTasks(false);
        GameManager.singleton.camController.SetTarget(NetworkPlayer.LocalPlayer.transform);
        NetworkPlayer.LocalPlayer.transform.position = NetworkManager.singleton.startTransform.position;
        NetworkPlayer.LocalPlayer.LocalPlayerFinishedGame();

        yield return new WaitForSeconds(1.5f);
        foreach(Route r in routes)
        {
            foreach(Room room in r.rooms)
            {
                room.ToggleAllTasks(true);
            }
        }

        foreach (NetworkPlayer p in GameManager.singleton.allPlayers)
        {
            p.ResetGame();
        }
    }


    [PunRPC]
    public void StartGame_RPC()
    {
        if (photonView.IsMine)
        {
            NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();
            allPlayers[Random.Range(0, allPlayers.Length)].photonView.RPC(nameof(NetworkPlayer.SetNarrator_RPC), RpcTarget.All);
        }
        currentRoute = 0;

        StartCoroutine(StartRouteRoutine(1.5f));
    }

    System.Collections.IEnumerator StartRouteRoutine(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        GameManager.singleton.notification.PlayNotification("Route " + GetRouteName() + " Opening Next!");
        yield return new WaitForSeconds(2.0f);
        for (int i = 5; i > 0; i--)
        {
            GameManager.singleton.notification.PlayNotification("Game Starting: " + i);
            yield return new WaitForSeconds(1.0f);
        }
        GameManager.singleton.roomManager.routes[currentRoute].routeDoor.OpenDoor();

        if (PhotonNetwork.PlayerListOthers.Length <= 0)
        {
            holdingAreaDoor.OpenDoor();
        }
    }

    internal void ToggleAllRouteTasks(bool toggle)
    {
        if (toggle)
        {
            foreach(Room r in routes[currentRoute].rooms)
            {
                r.ShowAllRoomTasks();
            }
        }
        else
        {
            foreach (Room r in routes[currentRoute].rooms)
            {
                r.HideAllRoomTasks();
            }
        }
    }

    string GetRouteName()
    {
        string routeName = "";
        switch (currentRoute)
        {
            case 0:
                routeName = "A";
                break;
            case 1:
                routeName = "B";
                break;
            case 2:
                routeName = "C";
                break;
        }
        return routeName;
    }
}

[System.Serializable]
public struct Route
{
    public string routeName;
    public Transform routeTransform;
    public Room[] rooms;
    public DoorObject routeDoor;
}