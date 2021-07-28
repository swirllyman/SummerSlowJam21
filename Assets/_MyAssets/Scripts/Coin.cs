using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    public TMPro.TMP_Text myTextObject;
    public void SetValue(int newValue)
    {
        myTextObject.text = "+" +newValue;
        Destroy(gameObject, 2.5f);
    }
}
