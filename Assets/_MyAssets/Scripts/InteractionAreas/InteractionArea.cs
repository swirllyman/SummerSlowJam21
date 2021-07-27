using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionArea : MonoBehaviour
{
    public SpriteRenderer selectionRend;
    public GameObject canvasObject;

    public virtual void Start()
    {
        ToggleArea(false);
        selectionRend.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == NetworkPlayer.LocalPlayer.myCollider)
        {
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

    public virtual void ToggleArea(bool toggle)
    {
        canvasObject.SetActive(toggle);
        if (toggle)
        {
            //selectionRend.enabled = true;
            //selectionRend.color = Color.green;
        }
        else
        {
            //selectionRend.enabled = false;
        }
    }
}
