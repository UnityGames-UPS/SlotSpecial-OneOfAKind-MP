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
    private Tween BlueFR_RotateTween, GoldenFR_RotateTween, OrangeFR_RotateTween, GreenFR_RotateTween, PurpleFR_RotateTween, wheelRoutine;

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
    private BackgroundType currentBG;

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
        currentBG = BackgroundType.Base;
        RotateBG();
    }

    internal void SwitchBG(BackgroundType bgType, List<int> values = null) {
        BackgroundType temp = currentBG;
        currentBG = bgType;

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
            if(values!= null) PopuplateWheel(circleCg.transform, values);
            circleCg.DOFade(1, 0.5f);
        }

        DOVirtual.DelayedCall(.5f, ()=> {
            if(temp!=BackgroundType.Base){
                int childCount = backgrounds[temp].CircleCG.transform.childCount;
                for(int i=0;i<childCount;i++){
                    Image image=backgrounds[temp].CircleCG.transform.GetChild(i).GetComponent<Image>();
                    image.name = "Image(" + i + ")";
                    image.sprite = null;
                    image.DOFade(1, 0);
                }
            }
        });
        
    }

    internal void FadeOutChildren(){
        int childCount = backgrounds[currentBG].CircleCG.transform.childCount;
        for(int i=0;i<childCount;i++){
            Image image=backgrounds[currentBG].CircleCG.transform.GetChild(i).GetComponent<Image>();
            image.DOFade(0, 0.4f);
        }
    }
    
    private void PopuplateWheel(Transform CircleTransform ,List<int> values){
        int childCount = CircleTransform.childCount;
        List<int> availableIndices = new List<int>();

        // Create a list of all available indices on the wheel
        for (int i = 0; i < childCount; i++) availableIndices.Add(i);

        System.Random random = new System.Random();

        // Place each value at a random position on the wheel
        foreach (int value in values) {
            // Get a random index from availableIndices
            int randomIndex = availableIndices[random.Next(availableIndices.Count)];
            availableIndices.Remove(randomIndex);  // Remove the used index

            // Assign the corresponding sprite to the randomly chosen index
            Sprite targetSprite = Array.Find(sprites, sprite => sprite.name == value.ToString());
            if (targetSprite != null) {
                Image childImage = CircleTransform.GetChild(randomIndex).GetComponent<Image>();
                if (childImage != null) {
                    childImage.sprite = targetSprite;
                    childImage.name = targetSprite.name;
                }
            } else {
                Debug.LogWarning($"Sprite for value {value} not found in sprites array.");
            }
        }

        // Fill remaining indices with random sprites
        foreach (int index in availableIndices) {
            Sprite randomSprite = sprites[random.Next(sprites.Length)];
            Image childImage = CircleTransform.GetChild(index).GetComponent<Image>();
            if (childImage != null) {
                childImage.sprite = randomSprite;
                childImage.name = randomSprite.name;
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
        // if(Input.GetKeyDown(KeyCode.Space)){
        //     RotateWheel(currentBG);
        // }
        // if(Input.GetKeyDown(KeyCode.RightAlt)){
        //     wheelRoutine.Kill();
        // }
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


    internal void RotateWheel(BackgroundType type){
        Transform Wheel_Transform = backgrounds[type].CircleCG.transform;
        wheelRoutine =  Wheel_Transform.DORotate(new Vector3(0, 0, -360f), 1.5f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
    }

    internal void StopWheel(){
        wheelRoutine.Kill();
    }

    private void StopRotation(string s){
        switch(s){
            case "Base":
                wheelRoutine.Kill();
                BlueFR_RotateTween?.Kill();
                BlueFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                GreenFR_RotateTween?.Kill();
                GreenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                OrangeFR_RotateTween?.Kill();
                OrangeFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                GoldenFR_RotateTween?.Kill();
                GoldenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                PurpleFR_RotateTween?.Kill();
                PurpleFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                break;

            case "BlueFR":
                wheelRoutine.Kill();
                NR_RotateTween?.Kill();
                NRBGCircle_CG.transform.localEulerAngles = Vector3.zero;
                GreenFR_RotateTween?.Kill();
                GreenFR_CG.transform.localEulerAngles = Vector3.zero;
                OrangeFR_RotateTween?.Kill();
                OrangeFR_CG.transform.localEulerAngles = Vector3.zero;
                GoldenFR_RotateTween?.Kill();
                GoldenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                PurpleFR_RotateTween?.Kill();
                PurpleFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                break;

            case "GreenFR":
                wheelRoutine.Kill();
                BlueFR_RotateTween?.Kill();
                BlueFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                NR_RotateTween?.Kill();
                NRBGCircle_CG.transform.localEulerAngles = Vector3.zero;
                OrangeFR_RotateTween?.Kill();
                OrangeFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                GoldenFR_RotateTween?.Kill();
                GoldenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                PurpleFR_RotateTween?.Kill();
                PurpleFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                break;
            
            case "GoldenFR":
                wheelRoutine.Kill();
                BlueFR_RotateTween?.Kill();
                BlueFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                GreenFR_RotateTween?.Kill();
                GreenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                OrangeFR_RotateTween?.Kill();
                OrangeFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                NR_RotateTween?.Kill();
                NRBGCircle_CG.transform.localEulerAngles = Vector3.zero;
                PurpleFR_RotateTween?.Kill();
                PurpleFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                break;

            case "OrangeFR":
                wheelRoutine.Kill();
                BlueFR_RotateTween?.Kill();
                BlueFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                GreenFR_RotateTween?.Kill();
                GreenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                NR_RotateTween?.Kill();
                NRBGCircle_CG.transform.localEulerAngles = Vector3.zero;
                GoldenFR_RotateTween?.Kill();
                GoldenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                PurpleFR_RotateTween?.Kill();
                PurpleFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                break;

            case "PurpleFR":
                wheelRoutine.Kill();
                BlueFR_RotateTween?.Kill();
                BlueFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                GreenFR_RotateTween?.Kill();
                GreenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                NR_RotateTween?.Kill();
                NRBGCircle_CG.transform.localEulerAngles = Vector3.zero;
                GoldenFR_RotateTween?.Kill();
                GoldenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                OrangeFR_RotateTween?.Kill();
                OrangeFRCircle_CG.transform.localEulerAngles = Vector3.zero;
                break;
        }
    }
}


