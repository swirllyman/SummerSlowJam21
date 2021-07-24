using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    public CameraController camController;
    public CenterConsole centerConsole;
    public Notification notification;

    private void Awake()
    {
        singleton = this;
    }
}
