using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : MonoBehaviourPun
{
    public static NetworkPlayer LocalPlayer;
    Rigidbody2D myBody;
    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine)
            LocalPlayer = this;

        myBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine) return;

        myBody.velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
}
