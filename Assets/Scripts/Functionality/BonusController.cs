using System.Collections;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class BonusController : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite GreenJokerSymbol;  //images taken initially
    [SerializeField] private Sprite RedJokerSymbol;  //images taken initially
    [SerializeField] private Sprite[] BlueJokerAnimations;
    [SerializeField] private Sprite[] GreenJokerAnimations;
    [SerializeField] private Sprite[] RedJokerAnimations;
    [SerializeField] private Sprite Emprty_Sprite;
    [Header("Image Animation")]
    [SerializeField] private ImageAnimation JokerEndAnimation;  
    [Header("Canvas Group")]
    [SerializeField] private CanvasGroup TopPayoutUI;
    [SerializeField] private CanvasGroup Joker_Start_UI;
    [SerializeField] private CanvasGroup JokerUI;
    [SerializeField] private CanvasGroup Minor_UI;
    [SerializeField] private CanvasGroup Major_UI;
    [SerializeField] private CanvasGroup Joker_End_UI;
    [Header("Images")]
    [SerializeField] private Image ResultImage;     //class to store the result matrix
    [SerializeField] private Image Glow_Minor_Image;
    [SerializeField] private Image Glow_Major_Image;
    [SerializeField] private Image Glow_Grand_Image;
    [SerializeField] private Image DiamondArrowImage;
    [Header("Text")]
    [SerializeField] private TMP_Text JokerCountText;
    [Header("Transform")]
    [SerializeField] private Transform BigWinImage;
    [SerializeField] private Transform HugeWinImage;
    [SerializeField] private Transform MegaWinImage;
    [Header("Buttons")]
    [SerializeField] private Button JokerStart_Cont_Bttn;
    [SerializeField] private Button MinorSecured_Cont_Bttn;
    [SerializeField] private Button MajorSecured_Cont_Bttn;
    [SerializeField] private Button Winnings_Cont_Bttn;
    [Header("Controller")]
    [SerializeField] private SocketIOManager SocketManager;
    [SerializeField] private BackgroundController BgController; 
    [SerializeField] private SlotBehaviour slotBehaviour;
    [SerializeField] private UIManager uIManager;
    [SerializeField] private AudioController audioController;
    internal bool wheelStopped = false;
    
    
    private void Start()
    {
        if(JokerStart_Cont_Bttn) JokerStart_Cont_Bttn.onClick.RemoveAllListeners();
        if(JokerStart_Cont_Bttn) JokerStart_Cont_Bttn.onClick.AddListener(()=>{
            StartCoroutine(StartBlueJokerWheelGame(SocketManager.resultData.jokerResponse));
        });

        if(MinorSecured_Cont_Bttn) MinorSecured_Cont_Bttn.onClick.RemoveAllListeners();
        if(MinorSecured_Cont_Bttn) MinorSecured_Cont_Bttn.onClick.AddListener(()=> {
            StartCoroutine(StartGreenJokerWheelGame(SocketManager.resultData.jokerResponse));
        });

        if(MajorSecured_Cont_Bttn) MajorSecured_Cont_Bttn.onClick.RemoveAllListeners();
        if(MajorSecured_Cont_Bttn) MajorSecured_Cont_Bttn.onClick.AddListener(()=>{
            StartCoroutine(StartRedJokerWheelGame(SocketManager.resultData.jokerResponse));
        });

        if(Winnings_Cont_Bttn) Winnings_Cont_Bttn.onClick.RemoveAllListeners();
        if(Winnings_Cont_Bttn) Winnings_Cont_Bttn.onClick.AddListener(()=>{
            StartCoroutine(EndJoker());
        });
    }

    private IEnumerator StartBlueJokerWheelGame(JokerResponse jokerResponse){
        audioController.SwitchBGSound(true);
        Joker_Start_UI.interactable=false;
        Joker_Start_UI.blocksRaycasts=false;
        Joker_Start_UI.DOFade(0, 0.5f);
        JokerUIToggle(true);
        BgController.SwitchBG(BackgroundController.BackgroundType.BlueFR, null, "JOKER1");
        DiamondArrowImage.DOFade(1, 0.2f);
        
        yield return new WaitForSeconds(2f);
        
        if(jokerResponse.blueRound == 3){
            for(int i=0;i<jokerResponse.blueRound;i++){
                if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                    yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                    yield return new WaitForSeconds(1f);
                }
                BgController.RotateWheel(); //ROTATE WHEEL
                
                yield return new WaitForSeconds(2f);

                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="Blue";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(2f);

                ImageAnimation ResultImageAnimation = DiamondArrowImage.GetComponent<Stopper>().ImageTransform.GetComponent<ImageAnimation>();
                ResetDiamondArrow();
                ResultImageAnimation.AnimationSpeed=6f;

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in BlueJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                ResultImageAnimation.StopAnimation();
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.rendererDelegate.sprite=Emprty_Sprite;
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            
            yield return Glow_Minor_Image.DOFade(1, 0.3f).WaitForCompletion();
            yield return new WaitForSeconds(1f);
            Minor_UI.interactable = true;
            Minor_UI.blocksRaycasts = true;
            Minor_UI.DOFade(1, 0.5f);
        }
        else if(jokerResponse.blueRound>0&&jokerResponse.blueRound<3){
            for(int i=0;i<jokerResponse.blueRound;i++){
                if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                    yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                    yield return new WaitForSeconds(1f);
                }
                BgController.RotateWheel(); //ROTATE WHEEL
                
                yield return new WaitForSeconds(2f);

                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="Blue";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(2f);

                ImageAnimation ResultImageAnimation = DiamondArrowImage.GetComponent<Stopper>().ImageTransform.GetComponent<ImageAnimation>();
                ResetDiamondArrow();
                ResultImageAnimation.AnimationSpeed=6f;

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in BlueJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                ResultImageAnimation.StopAnimation();
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.rendererDelegate.sprite=Emprty_Sprite;
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
            }
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);
            ResetDiamondArrow();

            OpenJokerGameEndingUI();
        }
        else if(jokerResponse.blueRound == 0){
            if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
            }
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);
            ResetDiamondArrow();

            OpenJokerGameEndingUI();
        }
    }

    private IEnumerator StartGreenJokerWheelGame(JokerResponse jokerResponse){
        ResultImage.sprite = GreenJokerSymbol;
        Minor_UI.interactable=false;
        Minor_UI.blocksRaycasts=false;
        Minor_UI.DOFade(0, 0.5f);
        JokerCountText.text = "0 OF 3 JOKERS COLLECTED";
        BgController.SwitchBG(BackgroundController.BackgroundType.GreenFR, null, "JOKER2");
        DiamondArrowImage.DOFade(1, 0.2f);

        yield return new WaitForSeconds(2f);
        
        if(jokerResponse.greenRound == 3){
            for(int i=0;i<jokerResponse.greenRound;i++){
                if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                    yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                    yield return new WaitForSeconds(1f);
                }
                BgController.RotateWheel(); //ROTATE WHEEL
                
                yield return new WaitForSeconds(2f);

                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="Green";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(2f);

                ImageAnimation ResultImageAnimation = DiamondArrowImage.GetComponent<Stopper>().ImageTransform.GetComponent<ImageAnimation>();
                ResetDiamondArrow();
                ResultImageAnimation.AnimationSpeed=6f;

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in GreenJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                ResultImageAnimation.StopAnimation();
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.rendererDelegate.sprite=Emprty_Sprite;
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            
            yield return Glow_Major_Image.DOFade(1, 0.3f).WaitForCompletion();
            yield return new WaitForSeconds(1f);
            Major_UI.interactable=true;
            Major_UI.blocksRaycasts=true;
            Major_UI.DOFade(1, 0.5f);
        }
        else if(jokerResponse.greenRound>0&&jokerResponse.greenRound<3){
            for(int i=0;i<jokerResponse.greenRound;i++){
                if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                    yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                    yield return new WaitForSeconds(1f);
                }
                BgController.RotateWheel(); //ROTATE WHEEL
                
                yield return new WaitForSeconds(2f);

                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="Green";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(2f);

                ImageAnimation ResultImageAnimation = DiamondArrowImage.GetComponent<Stopper>().ImageTransform.GetComponent<ImageAnimation>();
                ResetDiamondArrow();
                ResultImageAnimation.AnimationSpeed=6f;

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in GreenJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                ResultImageAnimation.StopAnimation();
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.rendererDelegate.sprite=Emprty_Sprite;
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
            }
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);
            ResetDiamondArrow();

            OpenJokerGameEndingUI();
        }
        else if(jokerResponse.greenRound == 0){
            if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
            }
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);
            ResetDiamondArrow();

            OpenJokerGameEndingUI();
        }
    }

    private IEnumerator StartRedJokerWheelGame(JokerResponse jokerResponse){
        ResultImage.sprite = RedJokerSymbol;
        Major_UI.interactable=false;
        Major_UI.blocksRaycasts=false;
        Major_UI.DOFade(0, 0.5f);
        JokerCountText.text = "0 OF 3 JOKERS COLLECTED";
        BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, null, "JOKER3");
        DiamondArrowImage.DOFade(1, 0.2f);

        yield return new WaitForSeconds(2f);
        
        if(jokerResponse.redRound == 3){
            for(int i=0;i<jokerResponse.redRound;i++){
                if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                    yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                    yield return new WaitForSeconds(1f);
                }
                BgController.RotateWheel(); //ROTATE WHEEL
                
                yield return new WaitForSeconds(2f);

                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="Red";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(2f);

                ImageAnimation ResultImageAnimation = DiamondArrowImage.GetComponent<Stopper>().ImageTransform.GetComponent<ImageAnimation>();
                ResetDiamondArrow();
                ResultImageAnimation.AnimationSpeed=6f;

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in RedJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                ResultImageAnimation.StopAnimation();
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.rendererDelegate.sprite=Emprty_Sprite;
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            yield return Glow_Grand_Image.DOFade(1, 0.3f).WaitForCompletion();
            yield return new WaitForSeconds(1f);

            OpenJokerGameEndingUI();
        }
        else if(jokerResponse.redRound>0&&jokerResponse.redRound<3){
            for(int i=0;i<jokerResponse.redRound;i++){
                if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                    yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                    yield return new WaitForSeconds(1f);
                }
                BgController.RotateWheel(); //ROTATE WHEEL
                
                yield return new WaitForSeconds(2f);

                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="Red";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(2f);

                ImageAnimation ResultImageAnimation = DiamondArrowImage.GetComponent<Stopper>().ImageTransform.GetComponent<ImageAnimation>();
                ResetDiamondArrow();
                ResultImageAnimation.AnimationSpeed=6f;

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in GreenJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                ResultImageAnimation.StopAnimation();
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.rendererDelegate.sprite=Emprty_Sprite;
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
            }
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);
            ResetDiamondArrow();

            OpenJokerGameEndingUI();
        }
        else if(jokerResponse.redRound == 0){
            if(DiamondArrowImage.color!=new Color(DiamondArrowImage.color.r, DiamondArrowImage.color.g, DiamondArrowImage.color.b, 255f)){
                yield return DiamondArrowImage.DOFade(1, 0.2f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
            }
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);
            ResetDiamondArrow();

            OpenJokerGameEndingUI();
        }
    }

    private void OpenJokerGameEndingUI(){
        if(SocketManager.resultData.jokerResponse.payout[0]!=0){
            int total = 0;
            foreach(int i in SocketManager.resultData.jokerResponse.payout){
                total += i;
            }
            Joker_End_UI.transform.GetChild(2).GetComponent<TMP_Text>().text = total.ToString();
            if(SocketManager.playerdata.currentWining==SocketManager.initialData.Joker[0]){
                Joker_End_UI.transform.GetChild(1).GetComponent<TMP_Text>().text = "YOU HAVE WON THE MINOR PRIZE";
            }
            else if(SocketManager.playerdata.currentWining>SocketManager.initialData.Joker[1] && SocketManager.playerdata.currentWining<SocketManager.initialData.Joker[2]){
                Joker_End_UI.transform.GetChild(1).GetComponent<TMP_Text>().text = "YOU HAVE WON THE MAJOR PRIZE";
            }
            else if(SocketManager.playerdata.currentWining>SocketManager.initialData.Joker[2]){
                Joker_End_UI.transform.GetChild(1).GetComponent<TMP_Text>().text = "YOU HAVE WON THE GRAND PRIZE";
            }
            Joker_End_UI.interactable=true;
            Joker_End_UI.blocksRaycasts=true;
            Joker_End_UI.DOFade(1, 0.5f);
        }
        else{
            audioController.SwitchBGSound(false);
        }
        JokerUIToggle(false);
        BgController.SwitchBG(BackgroundController.BackgroundType.Base);
        slotBehaviour.ToggleButtonGrp(true);
    }

    private IEnumerator EndJoker(){
        audioController.SwitchBGSound(false);
        Joker_End_UI.interactable=false;
        Joker_End_UI.blocksRaycasts=false;
        yield return Joker_End_UI.DOFade(0, 0.3f).WaitForCompletion();
        Joker_Start_UI.transform.parent.gameObject.SetActive(false);


        uIManager.OpenPopup(BigWinImage.parent.gameObject);

        Transform ImageTransform = null;
        if(SocketManager.playerdata.currentWining==SocketManager.initialData.Joker[0]){
            ImageTransform = BigWinImage;
        }
        else if(SocketManager.playerdata.currentWining>SocketManager.initialData.Joker[1] && SocketManager.playerdata.currentWining<SocketManager.initialData.Joker[2]){
            ImageTransform = HugeWinImage;
        }
        else if(SocketManager.playerdata.currentWining>SocketManager.initialData.Joker[2]){
            ImageTransform = MegaWinImage;
        }

        Tween tween = null;
        JokerEndAnimation.doLoopAnimation=true;
        JokerEndAnimation.StartAnimation();
        audioController.PlayWLAudio("coin");

        TMP_Text text = ImageTransform.GetChild(0).GetComponent<TMP_Text>();
        double WinAmt = 0;
        DOTween.To(() => WinAmt, (val) => WinAmt = val, SocketManager.playerdata.currentWining, 1.2f)
        .OnUpdate(() =>{
            if(text) text.text = ((int)WinAmt).ToString();
        });
        

        ImageTransform.DOScale(1.2f, 0.5f)
        .SetEase(Ease.OutQuad)
        .OnComplete(() =>
        {
            tween = ImageTransform.DOScale(0.8f, 0.5f)
                .SetEase(Ease.InQuad)
                .SetLoops(-1, LoopType.Yoyo);
        });
        StartCoroutine(slotBehaviour.TotalWinningsAnimation(SocketManager.playerdata.currentWining, true, false)); 
        yield return new WaitForSeconds(2f);
        tween.Kill();
        ImageTransform.DOScale(1, 0.5f);
        yield return new WaitForSeconds(2f);
        JokerEndAnimation.StopAnimation();
        ImageTransform.DOScale(0,0.5f);
        yield return new WaitForSeconds(1f);
        uIManager.ClosePopup(BigWinImage.parent.gameObject);
    }

    private void ResetDiamondArrow(){
        Stopper stopper = DiamondArrowImage.GetComponent<Stopper>();
        DiamondArrowImage.GetComponent<BoxCollider2D>().enabled = false; //RESET THE STOPPER
        DiamondArrowImage.DOFade(0, 0.2f);
        stopper.ImageTransform = null;
        stopper.stop = false;
        stopper.stopAT = "";
    }

    private void JokerUIToggle(bool toggle)
    {
        if(toggle){
            TopPayoutUI.DOFade(0, 0.5f);
            Glow_Minor_Image.DOFade(0,0);
            Glow_Major_Image.DOFade(0,0);
            Glow_Grand_Image.DOFade(0,0);
            JokerCountText.text = "0 OF 3 JOKERS COLLECTED";
            JokerUI.DOFade(1, 0.5f);
        }
        else{
            TopPayoutUI.DOFade(1, 0.5f);
            JokerUI.DOFade(0, 0.5f);
        }
    }
}
