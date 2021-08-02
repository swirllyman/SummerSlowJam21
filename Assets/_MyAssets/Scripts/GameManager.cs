using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    public static GameManager singleton;
    public NetworkPlayer[] allPlayers;

    public Volume psotProcessingVolume;
    public CameraController camController;
    public CenterConsole centerConsole;
    public MapView mapView;
    public Notification notification;
    public Selector selector;
    public RoomManager roomManager;
    public Transform playerListTransform;

    public TMP_Text gameTitle;
    public TMP_Text currentGoldText;
    public TMP_Text bankedGoldText;

    Vignette vignette;
    ColorAdjustments colorAdjustments;
    Coroutine caRoutine;

    private void Awake()
    {
        singleton = this;
        currentGoldText.text = "Current Gold: 0";
        bankedGoldText.text = "Banked Gold: 0";

        LeanTween.scale(gameTitle.gameObject, gameTitle.transform.localScale * 1.05f, 1.0f).setLoopPingPong();

        if (psotProcessingVolume.profile.TryGet(out Vignette tmpVig))
        {
            vignette = tmpVig;
        }

        if (psotProcessingVolume.profile.TryGet(out ColorAdjustments tmpCA))
        {
            colorAdjustments = tmpCA;
        }
    }

    public void PlayDeathEffect()
    {
        notification.PlayNotification("You Died");
        
        if (caRoutine != null)
            StopCoroutine(caRoutine);

        caRoutine = StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine() 
    {
        float lerpTime = .25f;
        float startColor = colorAdjustments.saturation.value;
        for (float i = 0; i < lerpTime; i += Time.deltaTime)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(startColor, -100, i / lerpTime);
            yield return null;
        }
    }

    public void PlayReviveEffect()
    {
        if (caRoutine != null)
            StopCoroutine(caRoutine);

        caRoutine = StartCoroutine(ReviveRoutine());
    }

    IEnumerator ReviveRoutine()
    {
        float lerpTime = .25f;
        float startColor = colorAdjustments.saturation.value;
        for (float i = 0; i < lerpTime; i += Time.deltaTime)
        {
            colorAdjustments.saturation.value = Mathf.Lerp(startColor, 14, i / lerpTime);
            yield return null;
        }
    }
}
