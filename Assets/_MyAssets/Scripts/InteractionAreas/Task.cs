using UnityEngine;

public class Task : InteractionArea
{
    public int taskID;
    public Room room;
    public Sprite selectionSprite;
    public Sprite standardSprite;
    public Collider2D myCollider;
    
    internal bool sabotaged = false;
    internal bool selected = false;

    public void ToggleHover(bool toggle)
    {
        if (toggle)
        {
            selectionRend.color = Color.yellow;
        }
        else if(!selected)
        {
            selectionRend.color = sabotaged ? Color.red : Color.green;
        }
    }

    public void ToggleSelect(bool toggle)
    {
        selected = toggle;
        selectionRend.sprite = toggle ? selectionSprite : standardSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == NetworkPlayer.LocalPlayer.myCollider &! NetworkPlayer.LocalPlayer.dead)
        {
            if (sabotaged)
            {
                NetworkPlayer.LocalPlayer.photonView.RPC(nameof(NetworkPlayer.Die_RPC), Photon.Pun.RpcTarget.All);

                bool allDead = true;
                foreach(NetworkPlayer p in FindObjectsOfType<NetworkPlayer>())
                {
                    if(!p.narrator &!p.dead)
                    {
                        allDead = false;
                    }
                }

                if (allDead)
                {
                    NetworkManager.singleton.photonView.RPC(nameof(NetworkManager.RouteFailed_RPC), Photon.Pun.RpcTarget.All);
                }
            }
            else
            {
                NetworkPlayer.LocalPlayer.photonView.RPC(nameof(NetworkPlayer.AddGold_RPC), Photon.Pun.RpcTarget.All, 100);
                GameManager.singleton.roomManager.photonView.RPC(nameof(RoomManager.FinishRoom_RPC), Photon.Pun.RpcTarget.All, room.routeNum, room.roomNum);
            }
            ToggleArea(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == NetworkPlayer.LocalPlayer.myCollider)
        {
            ToggleArea(false);
        }
    }
}
