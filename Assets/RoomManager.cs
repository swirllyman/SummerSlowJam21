using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public Routes[] routes;

    public Room currentRoom;

    public void ShowRoomTasks()
    {
        if(NetworkPlayer.LocalPlayer != null && NetworkPlayer.LocalPlayer.narrator)
        {
            currentRoom.ShowAllRoomTasks();
        }
    }

    public void SelectRoom()
    {
        if(currentRoom != null)
        {
            //Deselect Current Room
        }

        //currentRoom = 
    }
}

[System.Serializable]
public struct Routes
{
    public string routeName;
    public Room[] route;
}