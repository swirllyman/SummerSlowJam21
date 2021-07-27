using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public static NetworkPlayer LocalPlayer;
    public GameObject deathEffectPrefab;
    public Collider2D myCollider;
    public SpriteRenderer myRend;
    public TMP_Text playerNameText;
    internal int photonPlayerID = -1;
    internal bool narrator = false;
    internal bool usingMap = false;
    internal bool dead = false;

    int currentGold = 0;
    int bankedGold = 0;

    Rigidbody2D myBody;

    public delegate void PlayerPropertiesUpdatedCallback();
    public event PlayerPropertiesUpdatedCallback onPlayerPropertiesUpdated;
    // Start is called before the first frame update
    void Start()
    {
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

    public void LocalPlayerRouteFinished()
    {
        Hashtable readyState = new Hashtable() { ["Ready"] = false };
        PhotonNetwork.LocalPlayer.SetCustomProperties(readyState);

        photonView.RPC(nameof(FinishRoute_RPC), RpcTarget.All);
    }

    [PunRPC]
    void FinishRoute_RPC()
    {
        bankedGold += currentGold;
        currentGold = 0;

        if (photonView.IsMine)
        {
            GameManager.singleton.currentGoldText.text = "Current Gold: " + currentGold;
            GameManager.singleton.bankedGoldText.text = "Banked Gold: " + bankedGold;
        }
    }

    [PunRPC]
    void SetPlayerName_RPC(string newName)
    {
        playerNameText.text = newName;
    }

    [PunRPC]
    public void Die_RPC()
    {
        dead = true;
        if (photonView.IsMine)
        {
            GameManager.singleton.notification.PlayNotification("You Died");
            myRend.color = new Color(myRend.color.r, myRend.color.g, myRend.color.b, .25f);
        }
        else
        {
            GameManager.singleton.notification.PlayNotification(playerNameText.text + " Died");
            playerNameText.enabled = false;
            myRend.enabled = false;
        }

        int alivePlayers = 0;
        NetworkPlayer[] allPlayers = FindObjectsOfType<NetworkPlayer>();
        foreach (NetworkPlayer p in allPlayers)
        {
            if (!p.narrator & !p.dead)
            {
                alivePlayers++;
            }
        }


        StartCoroutine(PlayerDeathAnnoucement(alivePlayers > 0, currentGold));
        if (LocalPlayer.narrator)
        {
            if (alivePlayers > 0)
            {
                int splitGold = currentGold / alivePlayers;
                foreach (NetworkPlayer p in allPlayers)
                {
                    if (!p.narrator & !p.dead)
                    {
                        p.photonView.RPC(nameof(AddGold_RPC), RpcTarget.All, splitGold);
                    }
                }
            }
            else
            {
                LocalPlayer.photonView.RPC(nameof(AddGold_RPC), RpcTarget.All, currentGold);
            }
        }

        Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        currentGold = 0;
        GameManager.singleton.currentGoldText.text = "Current Gold: " + currentGold;
    }

    System.Collections.IEnumerator PlayerDeathAnnoucement(bool split, float amount)
    {
        yield return new WaitForSeconds(1.0f);
        if (split)
        {
            GameManager.singleton.notification.PlayNotification("Splitting: +" + amount+" Gold for living members");
        }
        else
        {
            GameManager.singleton.notification.PlayNotification("Boss Wins: +"+amount);
        }
    }

    [PunRPC]
    public void Revive_RPC()
    {
        dead = true;
        if (photonView.IsMine)
        {
            myRend.color = new Color(myRend.color.r, myRend.color.g, myRend.color.b, 1.0f);
        }
        else
        {
            playerNameText.enabled = true;
            myRend.enabled = true;
        }
    }

    [PunRPC]
    public void SetNarrator_RPC()
    {
        narrator = true;
        if (photonView.IsMine)
        {
            GameManager.singleton.notification.PlayNotification("<color=red>You</color> Are The Boss");
            transform.position = NetworkManager.singleton.holdingArea.position;
            if (!GameManager.singleton.mapView.tutorialized)
                GameManager.singleton.selector.tutorial.SetActive(true);
        }
        else
        {
            GameManager.singleton.notification.PlayNotification(playerNameText.text + " Is The Boss");
            LocalPlayer.transform.position = NetworkManager.singleton.startTransform.position;
        }
    }

    [PunRPC]
    public void AddGold_RPC(int goldToAdd)
    {
        currentGold += goldToAdd;

        if (photonView.IsMine)
        {
            GameManager.singleton.currentGoldText.text = "Current Gold: " + currentGold;
        }
    }
}
