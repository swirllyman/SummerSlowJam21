using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public static NetworkPlayer LocalPlayer;
    public Collider2D myCollider;
    public SpriteRenderer myRend;
    public TMP_Text playerNameText;
    internal int photonPlayerID = -1;
    internal bool narrator = false;
    internal bool usingMap = false;

    Rigidbody2D myBody;

    public delegate void PlayerPropertiesUpdatedCallback();
    public event PlayerPropertiesUpdatedCallback onPlayerPropertiesUpdated;
    // Start is called before the first frame update
    void Start()
    {
        narrator = true;
        photonPlayerID = photonView.Owner.ActorNumber;
        myBody = GetComponent<Rigidbody2D>();

        if (photonView.IsMine)
        {
            LocalPlayer = this;
            GameManager.singleton.camController.SetTarget(transform);
            LocalPlayerSetColor(Customizer.singleton.GetRandomAvailableColor());
            LocalPlayerSetName("No Name Set");

            onPlayerPropertiesUpdated += GameManager.singleton.centerConsole.UpdateCenterConsoleText;
        }
        else
        {
            playerNameText.text = photonView.Owner.NickName;
            if (photonView.Owner.CustomProperties.ContainsKey("ColorID"))
            {
                int newColorID = (int)photonView.Owner.CustomProperties["ColorID"];
                myRend.color = Customizer.playerColors[newColorID];
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        myBody.velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    //override void Prop
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);

        if (changedProps.ContainsKey("ColorID") && targetPlayer.ActorNumber == photonPlayerID)
        {
            int newColorID = (int)changedProps["ColorID"];
            myRend.color = Customizer.playerColors[newColorID];
            Customizer.singleton.UpdateAvailableColors();
        }

        onPlayerPropertiesUpdated?.Invoke();
    }

    public void LocalPlayerSetColor(int colorID)
    {
        Hashtable readyState = new Hashtable() { ["ColorID"] = colorID };
        PhotonNetwork.LocalPlayer.SetCustomProperties(readyState);
    }

    public void LocalPlayerReady()
    {
        Hashtable readyState = new Hashtable() { ["Ready"] = true };
        PhotonNetwork.LocalPlayer.SetCustomProperties(readyState);
    }

    public void LocalPlayerSetName(string newName)
    {
        PhotonNetwork.LocalPlayer.NickName = newName;
        photonView.RPC(nameof(SetPlayerName_RPC), RpcTarget.All, newName);
    }

    [PunRPC]
    void SetPlayerName_RPC(string newName)
    {
        playerNameText.text = newName;
    }
}
