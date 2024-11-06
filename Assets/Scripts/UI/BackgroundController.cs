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
    private Tween NR_RotateTween;
    private Tween BlueFR_RotateTween, GoldenFR_RotateTween, OrangeFR_RotateTween, GreenFR_RotateTween, PurpleFR_RotateTween;

    [Header("Rotation Tween Duration")]
    [SerializeField] private float NRTweenDuration = 30;
    [SerializeField] private float FRTweenDuration = 5;

    private void Start() {
        RotateBG();
    }

    private void SwitchBG(string s){
        switch(s){
            case "Base":
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(BlueFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(GoldenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(OrangeFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                if(GreenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(PurpleFR_CG.alpha!=0) PurpleFR_CG.DOFade(0, .5f).OnComplete(()=> {PurpleFR_ImageAnimation.StopAnimation();});
                if(PurpleFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                RotateBG();
                if(NRBG_CG.alpha!=1) NRBG_CG.DOFade(1, .5f);
                if(NRBGCircle_CG.alpha!=1) NRBGCircle_CG.DOFade(1, .5f);
                break;

            case "BlueFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(NRBGCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(GoldenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(OrangeFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                if(GreenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(PurpleFR_CG.alpha!=0) PurpleFR_CG.DOFade(0, .5f).OnComplete(()=> {PurpleFR_ImageAnimation.StopAnimation();});
                if(PurpleFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                BlueFR_ImageAnimation.StartAnimation();
                RotateFastBG(BlueFR_Image, "Blue");
                if(BlueFR_CG.alpha!=1) BlueFR_CG.DOFade(1, .5f);
                if(BlueFRCircle_CG.alpha!=1) BlueFRCircle_CG.DOFade(1, .5f);
                break;

            case "GreenFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(NRBGCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(BlueFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(GoldenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(OrangeFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(PurpleFR_CG.alpha!=0) PurpleFR_CG.DOFade(0, .5f).OnComplete(()=> {PurpleFR_ImageAnimation.StopAnimation();});
                if(PurpleFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });
                
                GreenFR_ImageAnimation.StartAnimation();
                RotateFastBG(GreenFR_Image, "Green");
                if(GreenFR_CG.alpha!=1) GreenFR_CG.DOFade(1, .5f);
                if(GreenFRCircle_CG.alpha!=1) GreenFRCircle_CG.DOFade(1, .5f);
                break;
            
            case "GoldenFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(NRBGCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(BlueFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(OrangeFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                if(GreenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(PurpleFR_CG.alpha!=0) PurpleFR_CG.DOFade(0, .5f).OnComplete(()=> {PurpleFR_ImageAnimation.StopAnimation();});
                if(PurpleFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                GoldenFR_ImageAnimation.StartAnimation();
                RotateFastBG(GoldenFR_Image, "Golden");
                if(GoldenFR_CG.alpha!=1) GoldenFR_CG.DOFade(1, .5f);
                if(GoldenFRCircle_CG.alpha!=1) GoldenFRCircle_CG.DOFade(1, .5f);
                break;

            case "OrangeFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(NRBGCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(BlueFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(GoldenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                if(GreenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(PurpleFR_CG.alpha!=0) PurpleFR_CG.DOFade(0, .5f).OnComplete(()=> {PurpleFR_ImageAnimation.StopAnimation();});
                if(PurpleFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                OrangeFR_ImageAnimation.StartAnimation();
                RotateFastBG(OrangeFR_Image, "Orange");
                if(OrangeFR_CG.alpha!=1) OrangeFR_CG.DOFade(1, .5f);
                if(OrangeFRCircle_CG.alpha!=1) OrangeFRCircle_CG.DOFade(1, .5f);
                break;

            case "PurpleFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(NRBGCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(BlueFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(GoldenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                if(GreenFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(OrangeFRCircle_CG.alpha!=0) BlueFRCircle_CG.DOFade(0, .5f);
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });
                
                PurpleFR_ImageAnimation.StartAnimation();
                RotateFastBG(PurpleFR_Image, "Purple");
                if(PurpleFR_CG.alpha!=1) PurpleFR_CG.DOFade(1, .5f);
                if(PurpleFRCircle_CG.alpha!=1) PurpleFRCircle_CG.DOFade(1, .5f);
                break;
        }
    }

    int index=0;
    private void Update() {
        if(Input.GetKeyDown(KeyCode.Keypad0)){
            SwitchBG("Base");
        }
        if(Input.GetKeyDown(KeyCode.Keypad1)){
            SwitchBG("BlueFR");
        }
        if(Input.GetKeyDown(KeyCode.Keypad2)){
            SwitchBG("GreenFR");
        }
        if(Input.GetKeyDown(KeyCode.Keypad3)){
            SwitchBG("GoldenFR");
        }
        if(Input.GetKeyDown(KeyCode.Keypad4)){
            SwitchBG("OrangeFR");
        }
        if(Input.GetKeyDown(KeyCode.Keypad5)){
            SwitchBG("PurpleFR");
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
            case "Blue":
            BlueFR_RotateTween = tween;
            break;
            case "Orange":
            OrangeFR_RotateTween = tween;
            break;
            case "Golden":
            GoldenFR_RotateTween = tween;
            break;
            case "Green":
            GreenFR_RotateTween = tween;
            break;
            case "Purple":
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


