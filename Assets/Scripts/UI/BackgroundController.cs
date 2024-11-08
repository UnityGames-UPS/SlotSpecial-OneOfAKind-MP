using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.Tweens;

public class BackgroundController : MonoBehaviour
{
    [Header("Background Images")]
    [SerializeField] private Image NR_BG_Image;
    [SerializeField] private Image BlueFR_Image, GoldenFR_Image, OrangeFR_Image, GreenFR_Image, PurpleFR_Image;

    [Header("BG Canvas Group")]
    [SerializeField] private CanvasGroup NRBG_CG;
    [SerializeField] private CanvasGroup BlueFR_CG;
    [SerializeField] private CanvasGroup GoldenFR_CG;
    [SerializeField] private CanvasGroup OrangeFR_CG;
    [SerializeField] private CanvasGroup GreenFR_CG;
    [SerializeField] private CanvasGroup PurpleFR_CG;

    [Header("Circle Canvas Group")]
    [SerializeField] private CanvasGroup NRBGCircle_CG;
    [SerializeField] private CanvasGroup BlueFRCircle_CG;
    [SerializeField] private CanvasGroup GoldenFRCircle_CG;
    [SerializeField] private CanvasGroup OrangeFRCircle_CG;
    [SerializeField] private CanvasGroup GreenFRCircle_CG;
    [SerializeField] private CanvasGroup PurpleFRCircle_CG;
    
    [Header("BG Image Animations")]
    [SerializeField] private ImageAnimation BlueFR_ImageAnimation;
    [SerializeField] private ImageAnimation GoldenFR_ImageAnimation;
    [SerializeField] private ImageAnimation OrangeFR_ImageAnimation;
    [SerializeField] private ImageAnimation GreenFR_ImageAnimation;
    [SerializeField] private ImageAnimation PurpleFR_ImageAnimation;
    [SerializeField] private Sprite[] sprites;
    private Tween NR_RotateTween;
    private Tween BlueFR_RotateTween, GoldenFR_RotateTween, OrangeFR_RotateTween, GreenFR_RotateTween, PurpleFR_RotateTween;

    [Header("Rotation Tween Duration")]
    [SerializeField] private float NRTweenDuration = 30;
    [SerializeField] private float FRTweenDuration = 5;

    public enum BackgroundType {
        Base,
        BlueFR,
        GreenFR,
        GoldenFR,
        OrangeFR,
        PurpleFR
    }

    private Dictionary<BackgroundType, (CanvasGroup CG, CanvasGroup CircleCG, ImageAnimation ImageAnim)> backgrounds;

    private void Awake() {
        backgrounds = new Dictionary<BackgroundType, (CanvasGroup, CanvasGroup, ImageAnimation)> {
            { BackgroundType.BlueFR, (BlueFR_CG, BlueFRCircle_CG, BlueFR_ImageAnimation) },
            { BackgroundType.GreenFR, (GreenFR_CG, GreenFRCircle_CG, GreenFR_ImageAnimation) },
            { BackgroundType.GoldenFR, (GoldenFR_CG, GoldenFRCircle_CG, GoldenFR_ImageAnimation) },
            { BackgroundType.OrangeFR, (OrangeFR_CG, OrangeFRCircle_CG, OrangeFR_ImageAnimation) },
            { BackgroundType.PurpleFR, (PurpleFR_CG, PurpleFRCircle_CG, PurpleFR_ImageAnimation) }
        };
    }
    private void Start() {
        RotateBG();
    }

    internal void SwitchBG(BackgroundType bgType, List<int> values = null) {
        foreach (var kvp in backgrounds) {
            if (kvp.Key != bgType) {
                kvp.Value.CG.DOFade(0, 0.5f).OnComplete(() => kvp.Value.ImageAnim.StopAnimation());
                kvp.Value.CircleCG.DOFade(0, 0.5f);
            }
        }

        DOVirtual.DelayedCall(0.5f, () => StopRotation(bgType.ToString()));

        if (bgType == BackgroundType.Base) {
            NRBG_CG.DOFade(1, 0.5f);
            NRBGCircle_CG.DOFade(1, 0.5f);
            RotateBG();
        } else {
            var (cg, circleCg, anim) = backgrounds[bgType];
            anim.StartAnimation();
            RotateFastBG(cg.transform.GetChild(0).GetComponent<Image>(), bgType.ToString());
            cg.DOFade(1, 0.5f);
            if(values.Count>0) PopuplateWheel(circleCg.transform, values);
            circleCg.DOFade(1, 0.5f);
        }
    }
    
    private void PopuplateWheel(Transform CircleTransform ,List<int> values){
        int childCount = CircleTransform.childCount;
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < childCount; i++) availableIndices.Add(i);

        // Ensure a random non-adjacent assignment for each value
        System.Random random = new System.Random();
        List<int> usedIndices = new List<int>();

        foreach (int value in values) {
            // Find non-adjacent random index
            int index;
            do {
                index = availableIndices[random.Next(availableIndices.Count)];
            } while (usedIndices.Contains(index - 1) || usedIndices.Contains(index + 1));

            usedIndices.Add(index);
            availableIndices.Remove(index);

            // Assign the corresponding sprite
            Sprite targetSprite = Array.Find(sprites, sprite => sprite.name == value.ToString());
            if (targetSprite != null) {
                Image childImage = CircleTransform.GetChild(index).GetComponent<Image>();
                if (childImage != null) {
                    childImage.sprite = targetSprite;
                }
            } else {
                Debug.LogWarning($"Sprite for value {value} not found in sprites array.");
            }
        }

        // Fill remaining slots with random sprites
        foreach (int index in availableIndices) {
            Sprite randomSprite = sprites[random.Next(sprites.Length)];
            Image childImage = CircleTransform.GetChild(index).GetComponent<Image>();
            if (childImage != null) {
                childImage.sprite = randomSprite;
            }
        }
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Keypad0)){
            SwitchBG(BackgroundType.Base);
        }
        if(Input.GetKeyDown(KeyCode.Keypad1)){
            SwitchBG(BackgroundType.BlueFR);
        }
        if(Input.GetKeyDown(KeyCode.Keypad2)){
            SwitchBG(BackgroundType.OrangeFR);
        }
        if(Input.GetKeyDown(KeyCode.Keypad3)){
            SwitchBG(BackgroundType.GreenFR);
        }
        if(Input.GetKeyDown(KeyCode.Keypad4)){
            SwitchBG(BackgroundType.PurpleFR);
        }
        if(Input.GetKeyDown(KeyCode.Keypad5)){
            SwitchBG(BackgroundType.GoldenFR);
        }
    }

    private void RotateBG(){
        float z= NR_BG_Image.transform.eulerAngles.z;
        z-=360;
        NR_RotateTween = NR_BG_Image.transform.DORotate(new Vector3(0, 0 , z), NRTweenDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
    }
    
    private void RotateFastBG(Image image, string s){
        float z= image.transform.eulerAngles.z;
        z-=360;
        Tween tween = image.transform.DORotate(new Vector3(0, 0, z), FRTweenDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        switch(s){
            case "BlueFR":
            BlueFR_RotateTween = tween;
            break;
            case "OrangeFR":
            OrangeFR_RotateTween = tween;
            break;
            case "GoldenFR":
            GoldenFR_RotateTween = tween;
            break;
            case "GreenFR":
            GreenFR_RotateTween = tween;
            break;
            case "PurpleFR":
            PurpleFR_RotateTween = tween;
            break;
        }
    }

    private void StopRotation(string s){
        switch(s){
            case "Base":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                PurpleFR_RotateTween?.Kill();
                break;

            case "BlueFR":
                NR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                PurpleFR_RotateTween?.Kill();
                break;

            case "GreenFR":
                BlueFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                PurpleFR_RotateTween?.Kill();
                break;
            
            case "GoldenFR":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                PurpleFR_RotateTween?.Kill();
                break;

            case "OrangeFR":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                PurpleFR_RotateTween?.Kill();
                break;

            case "PurpleFR":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                break;
        }
    }
}


