using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    public SpriteRenderer selectionRend;

    public Task[] allTasks;

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
}
