using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapView : InteractionArea
{
    public override void ToggleArea(bool toggle)
    {
        base.ToggleArea(toggle);

        if (NetworkPlayer.LocalPlayer == null) return;

        if (toggle)
        {
            NetworkPlayer.LocalPlayer.usingMap = true;
            GameManager.singleton.camController.SetTarget(transform);
        }
        else
        {
            NetworkPlayer.LocalPlayer.usingMap = false;
            GameManager.singleton.camController.SetTarget(NetworkPlayer.LocalPlayer.transform);
        }

        GameManager.singleton.camController.ToggleOverviewMode(toggle);
    }
}
