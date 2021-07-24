using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Notification : MonoBehaviour
{
    public LeanTweenType appearEaseType = LeanTweenType.easeOutBounce;
    public TMP_Text notificationText;

    public Color startColor;
    public Color fadeColor;

    Coroutine notificationRoutine;

    private void Awake()
    {
        notificationText.text = "";
    }

    public void PlayNotification(string newNotification)
    {
        notificationText.text = newNotification;
        notificationText.transform.localScale = Vector3.zero;
        LeanTween.value(notificationText.gameObject, TextColorUpdateCallback, fadeColor, startColor, 0.0f);
        LeanTween.scale(notificationText.rectTransform, Vector3.one, .25f);

        if (notificationRoutine != null)
        {
            StopCoroutine(notificationRoutine);
        }

        notificationRoutine = StartCoroutine(NotificationRoutine());
    }

    IEnumerator NotificationRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        LeanTween.value(notificationText.gameObject, TextColorUpdateCallback, startColor, fadeColor, 1.0f);
    }

    void TextColorUpdateCallback(Color newColor)
    {
        notificationText.color = newColor;
    }
}
