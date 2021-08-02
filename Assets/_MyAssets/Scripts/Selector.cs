using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Camera))]
public class Selector : MonoBehaviour
{
    public LayerMask hitLayer;
    public GameObject taskUI;
    public TMP_Text taskUIName;
    public Button safeButton;
    public Button sabotagebutton;

    public GameObject tutorial;
    public GameObject tutorialArrow;

    internal Task selectedTask;

    Camera myCam;
    Vector3 mousePos;
    RaycastHit2D hit;

    Task hoveredTask;

    Room hoveredRoom;
    Room selectedRoom;

    private void Awake()
    {
        myCam = GetComponent<Camera>();
        hit = new RaycastHit2D();
        taskUI.SetActive(false);

        if (tutorial != null)
        {
            tutorial.SetActive(false);
            LeanTween.moveLocalY(tutorialArrow, 125, 1.0f).setLoopPingPong().setEaseInBounce();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetworkPlayer.LocalPlayer != null && NetworkPlayer.LocalPlayer.narrator && NetworkPlayer.LocalPlayer.usingMap)
        {
            CheckRaycast();

            if(Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                if(hoveredTask != null)
                {
                    SelectTask();
                }
                else if(selectedTask != null)
                {
                    DeselectTask();
                }


                if (hoveredRoom != null)
                {
                    SelectRoom();
                }

                if(hoveredTask == null && hoveredRoom == null && selectedRoom != null)
                {
                    DeselectRoom();
                }
            }
        }
    }

    void SelectRoom()
    {
        if(selectedRoom != null)
        {
            selectedRoom.selectionRend.enabled = false;
        }

        selectedRoom = hoveredRoom;
        selectedRoom.selectionRend.enabled = true;
        selectedRoom.selectionRend.color = selectedRoom.selectionColor;

        GameManager.singleton.camController.SetTarget(selectedRoom.selectionRend.transform);
        GameManager.singleton.camController.ToggleOverviewMode(true, 1.5f);

        if (tutorial != null)
        {
            tutorial.SetActive(false);
            GameManager.singleton.mapView.tutorialized = true;
        }
    }

    void DeselectRoom()
    {
        selectedRoom.selectionRend.enabled = false;
        selectedRoom = null;

        GameManager.singleton.camController.SetTarget(GameManager.singleton.roomManager.routes[GameManager.singleton.roomManager.currentRoute].routeTransform);
        GameManager.singleton.camController.ToggleOverviewMode(true);
    }

    void SelectTask()
    {
        if (selectedTask != null)
        {
            selectedTask.ToggleHover(false);
            selectedTask.ToggleSelect(false);
            selectedTask = null;
        }

        selectedTask = hoveredTask;
        selectedTask.ToggleSelect(true);
        taskUIName.text = "Task " + selectedTask.taskID + "-" + selectedTask.room.roomNum;

        UpdateButtons();
        taskUI.SetActive(true);
    }

    void DeselectTask()
    {
        selectedTask.ToggleSelect(false);
        selectedTask.ToggleHover(false);
        selectedTask = null;
        taskUI.SetActive(false);
    }

    void CheckRaycast()
    {
        mousePos = myCam.ScreenToWorldPoint(Input.mousePosition);
        hit = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, 100, hitLayer);

        if (hit.collider != null)
        {
            switch (hit.collider.tag)
            {
                case "Task":

                    Task hitTask = hit.collider.GetComponent<Task>();

                    if (hoveredTask != hitTask && hitTask != selectedTask)
                    {
                        if (hoveredTask != null)
                        {
                            hoveredTask.ToggleHover(false);
                        }

                        hoveredTask = hitTask;
                        hoveredTask.ToggleHover(true);
                    }
                    break;

                case "Room":
                    if (hoveredTask != null)
                    {
                        hoveredTask.ToggleHover(false);
                        hoveredTask = null;
                    }

                    Room hitRoom = hit.collider.GetComponentInParent<Room>();

                    if (hoveredRoom != hitRoom)
                    {
                        if (hoveredRoom != null)
                        {
                            if (hoveredRoom != selectedRoom)
                            {
                                hoveredRoom.selectionRend.enabled = false;
                            }
                            hoveredRoom = null;
                        }

                        hoveredRoom = hitRoom;
                        if (hoveredRoom != null && hitRoom != selectedRoom)
                        {
                            if (hoveredRoom != selectedRoom)
                            {
                                hoveredRoom.selectionRend.enabled = false;
                            }

                            hoveredRoom.selectionRend.enabled = true;
                            hoveredRoom.selectionRend.color = hoveredRoom.hoverColor;
                        }
                    }
                    break;
            }
        }
        else
        {
            if(hoveredRoom != null)
            {
                if (hoveredRoom != selectedRoom)
                {
                    hoveredRoom.selectionRend.enabled = false;
                }
               hoveredRoom = null;
            }


            if(hoveredTask != null)
            {
                hoveredTask.ToggleHover(false);
                hoveredTask = null;
            }
        }
    }

    public void SetTaskSabotaged()
    {
        GameManager.singleton.roomManager.photonView.RPC(nameof(RoomManager.SetSwitch), Photon.Pun.RpcTarget.All, selectedTask.room.routeNum, selectedTask.room.roomNum, selectedTask.taskID - 1, true);
        //if (selectedTask != null) selectedTask.sabotaged = true;
        UpdateButtons();
    }

    public void SetTaskSafe()
    {
        GameManager.singleton.roomManager.photonView.RPC(nameof(RoomManager.SetSwitch), Photon.Pun.RpcTarget.All, selectedTask.room.routeNum, selectedTask.room.roomNum, selectedTask.taskID - 1, false);
        //if (selectedTask != null) selectedTask.sabotaged = false;
        UpdateButtons();
    }

    public void HighlightTask()
    {
        GameManager.singleton.roomManager.photonView.RPC(nameof(RoomManager.SetTaskHighlight), Photon.Pun.RpcTarget.All, selectedTask.room.routeNum, selectedTask.room.roomNum, selectedTask.taskID - 1);
    }

    internal void UpdateButtons()
    {
        if (selectedTask.room.roomStarted)
        {
            safeButton.interactable = false;
            sabotagebutton.interactable = false;
        }
        else
        {
            safeButton.interactable = selectedTask.sabotaged;
            sabotagebutton.interactable = !selectedTask.sabotaged;
        }

        if (selectedTask.sabotaged)
        {
            selectedTask.selectionRend.color = Color.red;
        }
        else
        {
            selectedTask.selectionRend.color = Color.green;
        }
    }

    public void HideAll()
    {
        if (selectedTask != null)
        {
            DeselectTask();
        }
        if (selectedRoom != null)
        {
            DeselectRoom();
        }

        if (hoveredRoom != null)
        {
            hoveredRoom.selectionRend.enabled = false;
            hoveredRoom = null;
        }

        if (hoveredTask != null)
        {
            hoveredTask.ToggleHover(false);
            hoveredTask = null;
        }

        tutorial.SetActive(false);
    }
}
