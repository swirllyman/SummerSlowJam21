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

    public void SelectArea()
    {
        selectionRend.enabled = true;
        selectionRend.color = Color.green;
    }

    public void DeselectArea()
    {
        selectionRend.enabled = false;
    }

    public virtual void ToggleArea(bool toggle)
    {
        canvasObject.SetActive(toggle);
        if (toggle)
        {
            SelectArea();
        }
        else
        {
            DeselectArea();
        }
    }
}
