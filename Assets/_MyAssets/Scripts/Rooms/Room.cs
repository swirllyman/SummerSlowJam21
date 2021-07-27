using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Room : MonoBehaviour
{
    public SpriteRenderer selectionRend;
    public int roomNum, routeNum;
    public Color hoverColor;
    public Color selectionColor;
    public Task[] allTasks;
    public DoorObject doorToNextRoom;

    private void Awake()
    {
        selectionRend.enabled = false;
    }

    public void ShowAllRoomTasks()
    {
        foreach(Task t in allTasks)
        {
            t.selectionRend.enabled = true;
            t.selectionRend.color = t.sabotaged ? Color.red : Color.green;
        }
    }

    public void HideAllRoomTasks()
    {
        foreach (Task t in allTasks)
        {
            t.selectionRend.enabled = false;
        }
    }

    public void OpenDoor()
    {
        if(doorToNextRoom == null)
        {
            NetworkPlayer.LocalPlayer.transform.position = NetworkManager.singleton.startTransform.position;
        }
        else
        {
            foreach (Task t in allTasks)
            {
                t.myCollider.enabled = false;
            }
            doorToNextRoom.OpenDoor();
        }
    }

    public void ResetRoom()
    {
        if (doorToNextRoom != null)
        {
            doorToNextRoom.CloseDoor();
        }
        foreach (Task t in allTasks)
        {
            t.myCollider.enabled = true;
        }
    }
}
