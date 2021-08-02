using Photon.Pun;

public class LevelFinish : InteractionArea
{
    public override void ToggleArea(bool toggle)
    {
        base.ToggleArea(toggle);
        if (!PhotonNetwork.IsConnected) return;

        if (toggle)
        {
            GameManager.singleton.roomManager.photonView.RPC(nameof(RoomManager.PlayerFinishedLevel_RPC), RpcTarget.All, NetworkPlayer.LocalPlayer.photonView.ViewID);
            GameManager.singleton.notification.PlayNotification("Level Complete!");
        }
    }
}
