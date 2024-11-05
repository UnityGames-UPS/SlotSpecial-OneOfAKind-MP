using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions.Tweens;

public class BackgroundController : MonoBehaviour
{
    [SerializeField] private Image NR_BG_Image;
    [SerializeField] private Image BlueFR_Image, GoldenFR_Image, OrangeFR_Image, GreenFR_Image;
    [SerializeField] private CanvasGroup NRBG_CG, BlueFR_CG, GoldenFR_CG, OrangeFR_CG, GreenFR_CG;
    [SerializeField] private ImageAnimation BlueFR_ImageAnimation, GoldenFR_ImageAnimation, OrangeFR_ImageAnimation, GreenFR_ImageAnimation;
    [SerializeField] private Tween NR_RotateTween;
    [SerializeField] private Tween BlueFR_RotateTween, GoldenFR_RotateTween, OrangeFR_RotateTween, GreenFR_RotateTween;
    [SerializeField] private float NRTweenDuration = 30;
    [SerializeField] private float FRTweenDuration = 5;

    private void Start() {
        RotateBG();
    }

    private void SwitchBG(string s){
        switch(s){
            case "Base":
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                RotateBG();
                if(NRBG_CG.alpha!=1) NRBG_CG.DOFade(1, .5f);
                break;

            case "BlueFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                BlueFR_ImageAnimation.StartAnimation();
                RotateFastBG(BlueFR_Image, "Blue");
                if(BlueFR_CG.alpha!=1) BlueFR_CG.DOFade(1, .5f);
                break;

            case "GreenFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });
                
                GreenFR_ImageAnimation.StartAnimation();
                RotateFastBG(GreenFR_Image, "Green");
                if(GreenFR_CG.alpha!=1) GreenFR_CG.DOFade(1, .5f);
                break;
            
            case "GoldenFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(OrangeFR_CG.alpha!=0) OrangeFR_CG.DOFade(0, .5f).OnComplete(()=> {OrangeFR_ImageAnimation.StopAnimation();});
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                GoldenFR_ImageAnimation.StartAnimation();
                RotateFastBG(GoldenFR_Image, "Golden");
                if(GoldenFR_CG.alpha!=1) GoldenFR_CG.DOFade(1, .5f);
                break;

            case "OrangeFR":
                if(NRBG_CG.alpha!=0) NRBG_CG.DOFade(0, .5f);
                if(BlueFR_CG.alpha!=0) BlueFR_CG.DOFade(0, .5f).OnComplete(()=> {BlueFR_ImageAnimation.StopAnimation();});
                if(GoldenFR_CG.alpha!=0) GoldenFR_CG.DOFade(0, .5f).OnComplete(()=> {GoldenFR_ImageAnimation.StopAnimation();});
                if(GreenFR_CG.alpha!=0) GreenFR_CG.DOFade(0, .5f).OnComplete(()=> {GreenFR_ImageAnimation.StopAnimation();});
                DOVirtual.DelayedCall(.5f, ()=>{ StopRotation(s); });

                OrangeFR_ImageAnimation.StartAnimation();
                RotateFastBG(OrangeFR_Image, "Orange");
                if(OrangeFR_CG.alpha!=1) OrangeFR_CG.DOFade(1, .5f);
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
        }
    }

    private void StopRotation(string s){
        switch(s){
            case "Base":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                break;

            case "BlueFR":
                NR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                break;

            case "GreenFR":
                BlueFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                break;
            
            case "GoldenFR":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                OrangeFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                break;

            case "OrangeFR":
                BlueFR_RotateTween?.Kill();
                GreenFR_RotateTween?.Kill();
                NR_RotateTween?.Kill();
                GoldenFR_RotateTween?.Kill();
                break;
        }
    }
}


