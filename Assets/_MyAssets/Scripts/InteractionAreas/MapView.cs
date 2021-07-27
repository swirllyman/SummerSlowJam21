using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapView : InteractionArea
{
    internal bool inUse = false;
    internal bool tutorialized = false;

    public override void ToggleArea(bool toggle)
    {
        base.ToggleArea(toggle);

        if (NetworkPlayer.LocalPlayer == null) return;
        inUse = toggle;
        if (toggle)
        {
            NetworkPlayer.LocalPlayer.usingMap = true;
            GameManager.singleton.camController.SetTarget(GameManager.singleton.roomManager.routes[0].routeTransform);
        }
        else
        {
            NetworkPlayer.LocalPlayer.usingMap = false;
            GameManager.singleton.selector.HideAll();
            GameManager.singleton.camController.SetTarget(NetworkPlayer.LocalPlayer.transform);
        }

        GameManager.singleton.camController.ToggleOverviewMode(toggle);
    }
}
