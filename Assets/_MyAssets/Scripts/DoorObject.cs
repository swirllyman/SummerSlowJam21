using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorObject : MonoBehaviour
{
    public GameObject closedDoorObject;
    public GameObject openDoorObject;

    [ContextMenu("Open Door")]
    public void OpenDoor()
    {
        closedDoorObject.SetActive(false);
        openDoorObject.SetActive(true);
    }

    [ContextMenu("Close Door")]
    public void CloseDoor()
    {
        closedDoorObject.SetActive(true);
        openDoorObject.SetActive(false);
    }
}
