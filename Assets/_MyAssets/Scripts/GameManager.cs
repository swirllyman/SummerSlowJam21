using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    public CameraController camController;
    public CenterConsole centerConsole;
    public MapView mapView;
    public Notification notification;
    public Selector selector;
    public RoomManager roomManager;

    public TMP_Text currentGoldText;
    public TMP_Text bankedGoldText;

    private void Awake()
    {
        singleton = this;
        currentGoldText.text = "Current Gold: 0";
        bankedGoldText.text = "Current Gold: 0";
    }
}
