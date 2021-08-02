using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;

public class NetworkPlayer : MonoBehaviourPunCallbacks
{
    public static NetworkPlayer LocalPlayer;
    public GameObject coinPrefab;
    public GameObject deathEffectPrefab;
    public GameObject playerSlotPrefab;
    public Collider2D myCollider;
    public SpriteRenderer myRend;
    public TMP_Text playerNameText;
    public Sprite idleSprite;
    public Sprite[] walkingSprites;

    internal int photonPlayerID = -1;
    internal int bankedGold = 0;
    internal bool narrator = false;
    internal bool usingMap = false;
    internal bool dead = false;
    internal bool levelFinished = false;
    internal PlayerSlot myPlayerSlot;

    int currentGold = 0;
    bool right = true;
    bool walking = false;

    Rigidbody2D myBody;

    Coroutine walkRoutine;

    public delegate void PlayerPropertiesUpdatedCallback();
    public event PlayerPropertiesUpdatedCallback onPlayerPropertiesUpdated;
    // Start is called before the first frame update
    void Start()
    {
        photonPlayerID = photonView.Owner.ActorNumber;
        myBody = GetComponent<Rigidbody2D>();
        myPlayerSlot = Instantiate(playerSlotPrefab, GameManager.singleton.playerListTransform).GetComponent<PlayerSlot>();

        if (photonView.IsMine)
        {
            LocalPlayer = this;
            GameManager.singleton.camController.SetTarget(transform);
            LocalPlayerSetColor(Customizer.singleton.GetRandomAvailableColor());
            LocalPlayerSetName(RandomNameGenerator.GetRandomName(1));

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

        myPlayerSlot.nameText.text = playerNameText.text;
        myPlayerSlot.bgImage.color = myRend.color;
        myPlayerSlot.currentGoldText.text = "Current: " + currentGold;
        myPlayerSlot.bankedGoldText.text = "Banked: " + bankedGold;

        GameManager.singleton.allPlayers = FindObjectsOfType<NetworkPlayer>();
    }

    private void OnDestroy()
    {
        GameManager.singleton.allPlayers = FindObjectsOfType<NetworkPlayer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        if (Input.GetKeyDown(KeyCode.D) & !right)
        {
            right = true;
            myRend.flipX = false;
            photonView.RPC(nameof(FlipVisuals_RPC), RpcTarget.Others, true);
        }

        if (Input.GetKeyDown(KeyCode.A) && right)
        {
            right = false;
            myRend.flipX = true;
            photonView.RPC(nameof(FlipVisuals_RPC), RpcTarget.Others, false);
        }

        myBody.velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f)
        {
            if (!walking)
            {
                photonView.RPC(nameof(SetWalking_RPC), RpcTarget.All, true);
            }
        }
        else
        {
            if (walking)
            {
                photonView.RPC(nameof(SetWalking_RPC), RpcTarget.All, false);
            }
        }
    }

    System.Collections.IEnumerator WalkRoutine()
    {
        while (true)
        {
            myRend.sprite = walkingSprites[0];
            yield return new WaitForSeconds(.1f);
            myRend.sprite = walkingSprites[1];
            yield return new WaitForSeconds(.1f);
        }
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
            if (myPlayerSlot != null)
                myPlayerSlot.bgImage.color = myRend.color;
        }

        onPlayerPropertiesUpdated?.Invoke();
    }

    public void FinishLevel()
    {
        bankedGold += currentGold;
        currentGold = 0;

        myPlayerSlot.currentGoldText.text = "Current: " + currentGold;
        myPlayerSlot.bankedGoldText.text = "Banked: " + bankedGold;
        if (photonView.IsMine)
        {
            GameManager.singleton.currentGoldText.text = "Current Gold: " + currentGold;
            GameManager.singleton.bankedGoldText.text = "Banked Gold: " + bankedGold;
        }

        levelFinished = true;
        myRend.enabled = false;
        playerNameText.enabled = false;
    }

    public void ResetGame()
    {
        myPlayerSlot.nameText.color = Color.white;
        currentGold = 0;
        bankedGold = 0;
        myPlayerSlot.currentGoldText.text = "Current: " + currentGold;
        myPlayerSlot.bankedGoldText.text = "Banked: " + bankedGold;

        if (photonView.IsMine)
        {
            GameManager.singleton.currentGoldText.text = "Current Gold: 0";
            GameManager.singleton.bankedGoldText.text = "Banked Gold: 0";
        }
    }

    #region Local Player Calls
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
        name = newName;
        PhotonNetwork.LocalPlayer.NickName = newName;
        GameManager.singleton.notification.PlayNotification("Name Set: " + newName);
        photonView.RPC(nameof(SetPlayerName_RPC), RpcTarget.All, newName);
    }

    public void LocalPlayerFinishedGame()
    {
        Hashtable readyState = new Hashtable() { ["Ready"] = false };
        PhotonNetwork.LocalPlayer.SetCustomProperties(readyState);
        photonView.RPC(nameof(ResetAfterGame_RPC), RpcTarget.All);
    }

    public void LocalPlayerRouteFinished()
    {
        photonView.RPC(nameof(FinishRoute_RPC), RpcTarget.All);
    }
    #endregion
    [PunRPC]
    public void ResetAfterGame_RPC()
    {
        myRend.enabled = true;
        playerNameText.enabled = true;
        narrator = false;
    }

    [PunRPC]
    void SetWalking_RPC(bool isWalking)
    {
        walking = isWalking;
        if (isWalking)
        {
            if (walkRoutine != null) StopCoroutine(walkRoutine);
            walkRoutine = StartCoroutine(WalkRoutine());
        }
        else
        {
            if (walkRoutine != null) StopCoroutine(walkRoutine);
            myRend.sprite = idleSprite;
        }
    }

    [PunRPC]
    void FlipVisuals_RPC(bool isRight)
    {
        right = isRight;
        myRend.flipX = !right;
    }

    [PunRPC]
    void FinishRoute_RPC()
    {
        bankedGold += currentGold;
        currentGold = 0;

        myPlayerSlot.currentGoldText.text = "Current: " + currentGold;
        myPlayerSlot.bankedGoldText.text = "Banked: " + bankedGold;
        if (photonView.IsMine)
        {
            GameManager.singleton.currentGoldText.text = "Current Gold: " + currentGold;
            GameManager.singleton.bankedGoldText.text = "Banked Gold: " + bankedGold;
        }
    }

    [PunRPC]
    void SetPlayerName_RPC(string newName)
    {
        name = newName;
        playerNameText.text = newName;
        if(myPlayerSlot != null)
            myPlayerSlot.nameText.text = playerNameText.text;
    }

    [PunRPC]
    public void Die_RPC()
    {
        dead = true;
        myPlayerSlot.nameText.color = Color.gray;
        if (photonView.IsMine)
        {
            GameManager.singleton.PlayDeathEffect();
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
        dead = false;
        myPlayerSlot.nameText.color = Color.white;
        if (photonView.IsMine)
        {
            GameManager.singleton.PlayReviveEffect();
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
        myPlayerSlot.nameText.color = Color.red;
        if (photonView.IsMine)
        {
            GameManager.singleton.notification.PlayNotification("<color=red>You</color> Are The Boss");
            transform.position = GameManager.singleton.roomManager.holdingArea.position;
            if (!GameManager.singleton.mapView.tutorialized)
                GameManager.singleton.selector.tutorial.SetActive(true);

            GameManager.singleton.notification.PlayNotification("Sabotage Phase!");
            GameManager.singleton.camController.SetTarget(GameManager.singleton.roomManager.routes[GameManager.singleton.roomManager.currentRoute].rooms[GameManager.singleton.roomManager.currentRoom].selectionRend.transform);
            GameManager.singleton.roomManager.ToggleAllRouteTasks(true);
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
        if (narrator)
        {
            bankedGold += goldToAdd;
        }
        else 
        {
            currentGold += goldToAdd;
        }

        GameObject coinObject = Instantiate(coinPrefab, transform.position, Quaternion.identity);

        LeanTween.moveLocalY(coinObject, coinObject.transform.localPosition.y + .15f, 1.0f).setEaseInOutExpo();
        coinObject.GetComponent<Coin>().SetValue(goldToAdd);

        myPlayerSlot.currentGoldText.text = "Current: " + currentGold;
        myPlayerSlot.bankedGoldText.text = "Banked: " + bankedGold;

        if (photonView.IsMine)
        {
            GameManager.singleton.currentGoldText.text = "Current Gold: " + currentGold;
            GameManager.singleton.currentGoldText.text = "Banked Gold: " + bankedGold;
        }
    }
}
