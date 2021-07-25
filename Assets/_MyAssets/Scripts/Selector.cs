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

    Camera myCam;
    Vector3 mousePos;
    RaycastHit2D hit;

    Task hoveredTask;
    Task selectedTask;

    Room hoveredRoom;
    Room selectedRoom;

    private void Awake()
    {
        myCam = GetComponent<Camera>();
        hit = new RaycastHit2D();
        taskUI.SetActive(false);
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
            selectedRoom.HideAllRoomTasks();
            selectedRoom.selectionRend.enabled = false;
        }

        selectedRoom = hoveredRoom;
        selectedRoom.ShowAllRoomTasks();
        selectedRoom.selectionRend.enabled = true;
        selectedRoom.selectionRend.color = Color.green;
    }

    void DeselectRoom()
    {
        selectedRoom.HideAllRoomTasks();
        selectedRoom.selectionRend.enabled = false;
        selectedRoom = null;
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
        taskUIName.text = selectedTask.taskName;

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
                        hoveredRoom = hitRoom;
                        if (hoveredRoom != null && hitRoom != selectedRoom)
                        {
                            if (hoveredRoom != selectedRoom)
                            {
                                hoveredRoom.HideAllRoomTasks();
                                hoveredRoom.selectionRend.enabled = false;
                            }

                            hoveredRoom.ShowAllRoomTasks();
                            hoveredRoom.selectionRend.enabled = true;
                            hoveredRoom.selectionRend.color = Color.yellow;
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
                    hoveredRoom.HideAllRoomTasks();
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
        if (selectedTask != null) selectedTask.sabotaged = true;
        UpdateButtons();
    }

    public void SetTaskSafe()
    {
        if (selectedTask != null) selectedTask.sabotaged = false;
        UpdateButtons();
    }

    void UpdateButtons()
    {
        safeButton.interactable = selectedTask.sabotaged;
        sabotagebutton.interactable = !selectedTask.sabotaged;

        if (selectedTask.sabotaged)
        {
            selectedTask.selectionRend.color = Color.red;
        }
        else
        {
            selectedTask.selectionRend.color = Color.green;
        }
    }
}
