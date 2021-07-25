using UnityEngine;

public class Task : InteractionArea
{
    public string taskName;
    public Sprite selectionSprite;
    public Sprite standardSprite;
    
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
}
