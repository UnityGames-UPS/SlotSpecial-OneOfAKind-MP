using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using System;
using UnityEngine;
using UnityEngine.UI.Extensions;
using UnityEngine.Assertions.Must;
using Unity.VisualScripting;
using UnityEngine.Android;
using Best.SocketIO;

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] SlotSymbols;  //images taken initially
    [SerializeField] private Sprite GreenJokerSymbol;  //images taken initially
    [SerializeField] private Sprite RedJokerSymbol;  //images taken initially

    [Header("Slot Images")]
    [SerializeField] private Image[] TotalSlotImages;     //class to store total images
    [SerializeField] private Image ResultImage;     //class to store the result matrix

    [Header("Slots Transforms")]
    [SerializeField] private Transform Slot_Transform;

    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button TotalBetPlus_Button;
    [SerializeField] private Button TotalBetMinus_Button;
    [SerializeField] private Button LineBetPlus_Button;
    [SerializeField] private Button LineBetMinus_Button;
    [SerializeField] private Button SkipWinAnimation_Button;
    [SerializeField] private Button BonusSkipWinAnimation_Button;

    [Header("Animated Sprites")]
    [SerializeField] private List<Animations> SlotAnimations;
    [SerializeField] private Sprite[] BlueJokerAnimations;
    [SerializeField] private Sprite[] GreenJokerAnimations;
    [SerializeField] private Sprite[] RedJokerAnimations;

    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text Win_Anim_Text;
    [SerializeField] private Image Win_Text_BG;
    [SerializeField] private TMP_Text BigWin_Text;
    [SerializeField] private TMP_Text BonusWin_Text;
    [SerializeField] private CanvasGroup TopPayoutUI;
    [SerializeField] private CanvasGroup JokerUI;
    [SerializeField] private Image Glow_Minor_Image;
    [SerializeField] private Image Glow_Major_Image;
    [SerializeField] private Image Glow_Grand_Image;
    [SerializeField] private TMP_Text JokerCountText;
    [SerializeField] private Image BoosterImage;
    [SerializeField] private Image ActivatedImage;
    [SerializeField] private Image DiamondArrowImage;
    [SerializeField] private Image NormalArrowImage;
    [SerializeField] private Image MovementImage;
    [SerializeField] private ImageAnimation BlastImageAnimation;


    [SerializeField] private CanvasGroup Joker_Start_UI;
    [SerializeField] private CanvasGroup Minor_UI;
    [SerializeField] private CanvasGroup Major_UI;
    [SerializeField] private CanvasGroup Joker_End_UI;
    [SerializeField] private Button JokerStart_Cont_Bttn;
    [SerializeField] private Button MinorSecured_Cont_Bttn;
    [SerializeField] private Button MajorSecured_Cont_Bttn;
    [SerializeField] private Button Winnings_Cont_Bttn;

    [Header("Audio Management")]
    [SerializeField] private AudioController audioController;

    [SerializeField] private UIManager uiManager;
    [SerializeField] private BackgroundController BgController; 

    [Header("BonusGame Popup")]
    [SerializeField] private BonusController _bonusManager;

    [Header("Free Spins Board")]
    [SerializeField] private GameObject FSBoard_Object;
    [SerializeField] private TMP_Text FSnum_text;
    int tweenHeight = 0;  //calculate the height at which tweening is done
    // private List<Tweener> alltweens = new List<Tweener>();
    private Tween slotTween;
    [SerializeField] private List<ImageAnimation> TempList;  //stores the sprites whose animation is running at present 

    [SerializeField] private SocketIOManager SocketManager;

    private Coroutine AutoSpinRoutine = null;
    private Coroutine FreeSpinRoutine = null;
    private Coroutine tweenroutine;
    private Coroutine BoxAnimRoutine = null;

    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool WinAnimationFin = true;

    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    internal bool CheckPopups = false;
    internal bool wheelStopped = false;

    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    // protected int Lines = 20;
    [SerializeField] private int IconSizeFactor = 100;       //set this parameter according to the size of the icon and spacing
    private int numberOfSlots = 5;          //number of columns
    private ImageAnimation SlotImageAnimationScript;

    private void Start()
    {
        SlotImageAnimationScript = ResultImage.GetComponent<ImageAnimation>();
        // ShuffleSlot(); //test
        IsAutoSpin = false;

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
            Joker_End_UI.interactable=false;
            Joker_End_UI.blocksRaycasts=false;
            Joker_End_UI.DOFade(0, 0.5f);
        });

        if (SlotStart_Button) SlotStart_Button.onClick.RemoveAllListeners();
        if (SlotStart_Button) SlotStart_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StartSlots();
        });

        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.RemoveAllListeners();
        if (TotalBetPlus_Button) TotalBetPlus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(true);
        });

        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.RemoveAllListeners();
        if (TotalBetMinus_Button) TotalBetMinus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(false);
        });

        if (LineBetPlus_Button) LineBetPlus_Button.onClick.RemoveAllListeners();
        if (LineBetPlus_Button) LineBetPlus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(true);
        });

        if (LineBetMinus_Button) LineBetMinus_Button.onClick.RemoveAllListeners();
        if (LineBetMinus_Button) LineBetMinus_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            ChangeBet(false);
        });

        if (AutoSpin_Button) AutoSpin_Button.onClick.RemoveAllListeners();
        if (AutoSpin_Button) AutoSpin_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            AutoSpin();
        });

        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.RemoveAllListeners();
        if (AutoSpinStop_Button) AutoSpinStop_Button.onClick.AddListener(delegate
        {
            uiManager.CanCloseMenu();
            StopAutoSpin();
        });

        // if (SkipWinAnimation_Button) SkipWinAnimation_Button.onClick.RemoveAllListeners();
        // if (SkipWinAnimation_Button) SkipWinAnimation_Button.onClick.AddListener(delegate
        // {
        //     uiManager.CanCloseMenu();
        //     StopGameAnimation();
	    // });

        // if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.onClick.RemoveAllListeners();
        // if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.onClick.AddListener(delegate
        // {
        //     uiManager.CanCloseMenu();
        //     StopGameAnimation();
        // });

        if (FSBoard_Object) FSBoard_Object.SetActive(false);

        tweenHeight = (17 * IconSizeFactor) - 280;
        //Debug.Log("Tween Height: " + tweenHeight);
    }

    

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            IsAutoSpin = true;
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(true);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(false);

            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                AutoSpinRoutine = null;
            }
            AutoSpinRoutine = StartCoroutine(AutoSpinCoroutine());
        }
    }

    private void StopAutoSpin()
    {
        if (IsAutoSpin)
        { 
            StartCoroutine(StopAutoSpinCoroutine());
        }
    }

    private IEnumerator AutoSpinCoroutine()
    {
        while (IsAutoSpin)
        {
            StartSlots(IsAutoSpin);
            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        if (AutoSpinStop_Button) AutoSpinStop_Button.interactable = false;
        yield return new WaitUntil(() => !IsSpinning);
        ToggleButtonGrp(true);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
            if (AutoSpin_Button) AutoSpin_Button.gameObject.SetActive(true);
            AutoSpinStop_Button.interactable = true;
            tweenroutine = null;
            AutoSpinRoutine = null;
            IsAutoSpin = false;
            StopCoroutine(StopAutoSpinCoroutine());
        }
    }
    #endregion

    #region FreeSpin
    internal void FreeSpin(int spins)
    {
        if (!IsFreeSpin)
        {
            if (FSnum_text) FSnum_text.text = spins.ToString();
            IsFreeSpin = true;
            ToggleButtonGrp(false);

            if (FreeSpinRoutine != null)
            {
                StopCoroutine(FreeSpinRoutine);
                FreeSpinRoutine = null;
            }
            FreeSpinRoutine = StartCoroutine(FreeSpinCoroutine(spins));
        }
    }

    private IEnumerator FreeSpinCoroutine(int spinchances)
    {
        int i = 0;
        int j = spinchances;
        while (i < spinchances)
        {
            j -= 1;
            if (FSnum_text) FSnum_text.text = j.ToString();

            StartSlots(false);

            yield return tweenroutine;
            yield return new WaitForSeconds(.5f);
            i++;
        }
        ToggleButtonGrp(true);
        IsFreeSpin = false;
        // StartCoroutine(_bonusManager.BonusGameEndRoutine());
    }
    #endregion

    private void CompareBalance()
    {
        if (currentBalance < currentTotalBet)
        {
            uiManager.LowBalPopup();
            SlotStart_Button.interactable = true;
        }
    }

    private void ChangeBet(bool IncDec)
    {
        if (audioController) audioController.PlayButtonAudio();
        if (IncDec)
        {
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                BetCounter++;
            }
            if (BetCounter == SocketManager.initialData.Bets.Count - 1)
            {
                if (LineBetPlus_Button) LineBetPlus_Button.interactable = false;
                if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = false;
            }
            if (BetCounter > 0)
            {
                if (LineBetPlus_Button) LineBetMinus_Button.interactable = true;
                if (TotalBetMinus_Button) TotalBetMinus_Button.interactable = true;
            }
        }
        else
        {
            if (BetCounter > 0)
            {
                BetCounter--;
            }
            if (BetCounter == 0)
            {
                if(LineBetMinus_Button) LineBetMinus_Button.interactable = false;
                if(TotalBetMinus_Button) TotalBetMinus_Button.interactable = false;
            }
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
                if(LineBetPlus_Button) LineBetPlus_Button.interactable = true;
                if(TotalBetPlus_Button) TotalBetPlus_Button.interactable = true;
            }
        }
        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        currentTotalBet = SocketManager.initialData.Bets[BetCounter];
        CompareBalance();
    }

    #region InitialFunctions
    internal void ShuffleSlot(bool midTween = false)
    {
        const float selectionProbability = 0.4f;
        int maxRandomIndex = SlotSymbols.Count() - 3;

        for (int i = 0; i < TotalSlotImages.Length; i++)
        {
            // Skip index 6 if midTween is true
            if (midTween && (i == 6))
            {
                continue;
            }

            // Set slot sprite based on random selection
            TotalSlotImages[i].sprite = GetRandomSprite(selectionProbability, maxRandomIndex);
        }
    }

    // Helper function to select a sprite based on probability
    private Sprite GetRandomSprite(float probability, int maxIndex)
    {
        if (UnityEngine.Random.Range(0f, 1f) < probability)
        {
            int randomIndex = UnityEngine.Random.Range(1, maxIndex);
            return SlotSymbols[randomIndex];
        }
        else
        {
            return SlotSymbols[0];
        }
    }


    internal void SetInitialUI()
    {
        BetCounter = 0;
        if (TotalBet_text) TotalBet_text.text = SocketManager.initialData.Bets[BetCounter].ToString();
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Balance_text) Balance_text.text = SocketManager.playerdata.Balance.ToString("f2");
        currentBalance = SocketManager.playerdata.Balance;
        currentTotalBet = SocketManager.initialData.Bets[BetCounter];
        CompareBalance();
        uiManager.InitialiseUIData(SocketManager.initUIData.AbtLogo.link, SocketManager.initUIData.AbtLogo.logoSprite, SocketManager.initUIData.ToULink, SocketManager.initUIData.PopLink, SocketManager.initUIData.paylines);
    }
    #endregion

    private void OnApplicationFocus(bool focus)
    {
        audioController.CheckFocusFunction(focus, CheckSpinAudio);
    }

    //function to populate animation sprites accordingly
    private void PopulateAnimationSprites()
    {
        SlotImageAnimationScript.textureArray.Clear();
        SlotImageAnimationScript.textureArray.TrimExcess();
        int startIndex = SocketManager.resultData.resultSymbols;
        int lastIndex = SocketManager.resultData.levelup.level;
        for(int i = startIndex; i<=lastIndex; i++){
            for(int j = 0; j<SlotAnimations[i].Animation.Count;j++){
                SlotImageAnimationScript.textureArray.Add(SlotAnimations[i].Animation[j]);
            }
        }
        // if(SlotImageAnimationScript.textureArray.count)
        SlotImageAnimationScript.AnimationSpeed = 10 * (lastIndex-startIndex);

    }

    #region SlotSpin
    //starts the spin process
    private void StartSlots(bool autoSpin = false)
    {
        if (audioController) audioController.PlaySpinButtonAudio();

        if (!autoSpin)
        {
            if (AutoSpinRoutine != null)
            {
                StopCoroutine(AutoSpinRoutine);
                StopCoroutine(tweenroutine);
                tweenroutine = null;
                AutoSpinRoutine = null;
            }
        }

        ToggleButtonGrp(false);
        if (TotalWin_text) TotalWin_text.text = "0.00";
        CloseSlotWinningsUI();

        tweenroutine = StartCoroutine(TweenRoutine());
    }

    //manage the Routine for spinning of the slots
    private IEnumerator TweenRoutine()
    {
        if (currentBalance < currentTotalBet && !IsFreeSpin) // Check if balance is sufficient to place the bet
        {
            CompareBalance();
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            yield break;
        }

        // CheckSpinAudio = true;
        IsSpinning = true;

        if (currentBalance < SocketManager.playerdata.Balance) // Deduct balance if not a bonus
        {
            BalanceDeduction();
        }

        InitializeTweening();

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        currentBalance = SocketManager.playerdata.Balance;

        yield return new WaitForSeconds(1f);

        int resultID = SocketManager.resultData.resultSymbols;
        if(ResultImage) ResultImage.sprite = SlotSymbols[resultID];

        yield return new WaitForSeconds(.5f);

        yield return StopTweening();

        yield return new WaitForSeconds(0.3f);

        if(SocketManager.resultData.jokerResponse.isTriggered){
            Joker_Start_UI.transform.parent.gameObject.SetActive(true);
            Joker_Start_UI.interactable = true;
            Joker_Start_UI.blocksRaycasts = true;
            Joker_Start_UI.DOFade(1, 0.5f);
            yield break;
        }

        if(SocketManager.resultData.levelup.isLevelUp){
            TopUIToggle(false);
            int ResultValue = SocketManager.resultData.levelup.level-SocketManager.resultData.resultSymbols;
            List<int> temp = new();
            temp.Add(ResultValue);
            BgController.SwitchBG(BackgroundController.BackgroundType.GreenFR, temp, "LEVEL");
            yield return BoosterActivatedAnimation();
            StartCoroutine(StartLevelBoosterWheelGame(BackgroundController.BackgroundType.GreenFR));
            yield break;
        }

        if(SocketManager.resultData.booster.type != "NONE"){
            TopUIToggle(false);
            BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, SocketManager.resultData.booster.multipliers, "MULTIPLIER");
            yield return BoosterActivatedAnimation();
            StartCoroutine(StartMultiplierWheelGame(BackgroundController.BackgroundType.OrangeFR, SocketManager.initUIData.paylines.symbols[resultID].payout, 0));
            yield break;
        }

        if(SocketManager.playerdata.currentWining > 0 && !SocketManager.resultData.jokerResponse.isTriggered && !SocketManager.resultData.levelup.isLevelUp && SocketManager.resultData.booster.type == "NONE" && SocketManager.resultData.freespinType == "NONE"){
            yield return TotalWinningsAnimation(SocketManager.playerdata.currentWining);
        }
        else{
            ToggleButtonGrp(true);
        }
    }
    #endregion

    private void InitialJokerUI(){
        
    }

    private IEnumerator StartBlueJokerWheelGame(JokerResponse jokerResponse){
        Joker_Start_UI.interactable=false;
        Joker_Start_UI.blocksRaycasts=false;
        Joker_Start_UI.DOFade(0, 0.5f);
        JokerUIToggle(true);
        BgController.SwitchBG(BackgroundController.BackgroundType.BlueFR, null, "JOKER1");
        DiamondArrowImage.DOFade(1, 0.2f);
        
        yield return new WaitForSeconds(3f);
        
        if(jokerResponse.blueRound == 3){
            for(int i=0;i<jokerResponse.blueRound;i++){
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

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in BlueJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            
            Glow_Minor_Image.DOFade(1, 0.3f);
            Minor_UI.interactable = true;
            Minor_UI.blocksRaycasts = true;
            Minor_UI.DOFade(1, 0.5f);
        }
        else if(jokerResponse.blueRound>0&&jokerResponse.blueRound<3){
            for(int i=0;i<jokerResponse.blueRound;i++){
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

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in BlueJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.name ="Empty";
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

            EndJokerGame();
        }
        else if(jokerResponse.blueRound == 0){
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);

            EndJokerGame();
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

        yield return new WaitForSeconds(3f);
        
        if(jokerResponse.greenRound == 3){
            for(int i=0;i<jokerResponse.greenRound;i++){
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

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in GreenJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            
            Glow_Major_Image.DOFade(1, 0.3f);
            Major_UI.interactable=true;
            Major_UI.blocksRaycasts=true;
            Major_UI.DOFade(1, 0.5f);
        }
        else if(jokerResponse.greenRound>0&&jokerResponse.greenRound<3){
            for(int i=0;i<jokerResponse.greenRound;i++){
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

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in GreenJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.name ="Empty";
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

            EndJokerGame(true);
        }
        else if(jokerResponse.greenRound == 0){
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);

            EndJokerGame(true);
        }
    }

    private IEnumerator StartRedJokerWheelGame(JokerResponse jokerResponse){
        ResultImage.sprite = RedJokerSymbol;
        Major_UI.interactable=false;
        Major_UI.blocksRaycasts=false;
        Major_UI.DOFade(0, 0.5f);
        JokerCountText.text = "0 OF 3 JOKERS COLLECTED";
        BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, null, "JOKER2");
        DiamondArrowImage.DOFade(1, 0.2f);

        yield return new WaitForSeconds(3f);
        
        if(jokerResponse.redRound == 3){
            for(int i=0;i<jokerResponse.redRound;i++){
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

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in RedJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.name ="Empty";
                yield return new WaitForSeconds(1f);
            }
            
            EndJokerGame();
        }
        else if(jokerResponse.redRound>0&&jokerResponse.redRound<3){
            for(int i=0;i<jokerResponse.redRound;i++){
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

                ResultImageAnimation.textureArray.Clear();
                ResultImageAnimation.textureArray.TrimExcess();
                foreach(Sprite s in GreenJokerAnimations){
                    ResultImageAnimation.textureArray.Add(s);
                }

                ResultImageAnimation.StartAnimation();
                yield return new WaitUntil(()=> ResultImageAnimation.textureArray[^1] == ResultImageAnimation.rendererDelegate.sprite);
                JokerCountText.text = i+1 + " OF 3 JOKERS COLLECTED";
                ResultImageAnimation.name ="Empty";
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

            JokerUIToggle(false);
            BgController.SwitchBG(BackgroundController.BackgroundType.Base);
            ToggleButtonGrp(true);
        }
        else if(jokerResponse.redRound == 0){
            BgController.RotateWheel(); //ROTATE WHEEL
                
            yield return new WaitForSeconds(2f);

            DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
            DiamondArrowImage.GetComponent<Stopper>().stopAT="Empty";
            Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

            wheelStopped =false;
            DiamondArrowImage.GetComponent<Stopper>().stop=true;

            yield return new WaitUntil(()=> wheelStopped);
            yield return new WaitForSeconds(2f);

            EndJokerGame();
        }
    }

    private void EndJokerGame(bool showPayout=false){
        if(showPayout){
            int total = 0;
            foreach(int i in SocketManager.resultData.jokerResponse.payout){
                total += i;
            }
            Joker_End_UI.transform.GetChild(2).GetComponent<TMP_Text>().text = total.ToString();
            if(SocketManager.resultData.jokerResponse.payout.Count == 1){
                Joker_End_UI.transform.GetChild(1).GetComponent<TMP_Text>().text = "YOU HAVE WON THE MINOR PRIZE";
            }
            else if(SocketManager.resultData.jokerResponse.payout.Count == 2){
                Joker_End_UI.transform.GetChild(1).GetComponent<TMP_Text>().text = "YOU HAVE WON THE MAJOR PRIZE";
            }
            else if(SocketManager.resultData.jokerResponse.payout.Count == 3){
                Joker_End_UI.transform.GetChild(1).GetComponent<TMP_Text>().text = "YOU HAVE WON THE GRAND PRIZE";
            }
            Joker_End_UI.interactable=true;
            Joker_End_UI.blocksRaycasts=true;
            Joker_End_UI.DOFade(1, 0.5f);
        }
        JokerUIToggle(false);
        BgController.SwitchBG(BackgroundController.BackgroundType.Base);
        Joker_Start_UI.transform.parent.gameObject.SetActive(false);
        ToggleButtonGrp(true);
    }

    private IEnumerator StartLevelBoosterWheelGame(BackgroundController.BackgroundType type){
        NormalArrowImage.DOFade(1, 0.2f);
        yield return new WaitForSeconds(2f);
        BgController.RotateWheel(); //ROTATE WHEEL
        yield return new WaitForSeconds(2f); //WAITING FOR ROTATION ANIMATION

        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = true; //TURNING ON COLLIDER TO STOP THE ROTATIO
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();
        int stopAt=SocketManager.resultData.levelup.level-SocketManager.resultData.resultSymbols;
        stopper.stopAT=stopAt.ToString();
        Debug.Log("Stopping at: " + stopper.stopAT);

        wheelStopped=false;
        stopper.stop=true;

        yield return new WaitUntil(()=> wheelStopped);
        yield return new WaitForSeconds(2f); //WAITING FOR USER TO READ THE RESULT
        Transform ResultImage = stopper.ImageTransform;

        ResetNormalArrow();
        PopulateAnimationSprites();
        ResultImage.name="Empty";
        RectTransform resultImageRect = ResultImage.GetComponent<RectTransform>();
        MovementImage.rectTransform.position = resultImageRect.position;
        MovementImage.rectTransform.sizeDelta = new Vector2(resultImageRect.rect.width, resultImageRect.rect.height);
        MovementImage.sprite = ResultImage.GetComponent<Image>().sprite;
        ResultImage.GetComponent<Image>().DOFade(0, 0.1f);
        MovementImage.DOFade(1, 0); //USING THE MOVEMENT IMAGE INSTEAD OF THE WHEEL IMAGE

        Vector3 tempPosi = MovementImage.transform.localPosition;
        yield return MovementImage.transform.DOLocalMoveY(106, .5f).SetEase(Ease.InBack, 2.5f).WaitForCompletion(); //MOVING THE IMAGE TO THE MIDDLE
        MovementImage.DOFade(0, 0);
        MovementImage.transform.localPosition=tempPosi; //RESETTING THE IMAGE

        SlotImageAnimationScript.StartAnimation();

        yield return new WaitUntil(()=> SlotImageAnimationScript.textureArray[^1] == SlotImageAnimationScript.rendererDelegate.sprite);
        SlotImageAnimationScript.StopAnimation();

        this.ResultImage.sprite = SlotSymbols[SocketManager.resultData.levelup.level];
        yield return new WaitForSeconds(2f);
        yield return TotalWinningsAnimation(SocketManager.initUIData.paylines.symbols[SocketManager.resultData.levelup.level].payout, true, false);
        yield return new WaitForSeconds(2f);

        if(SocketManager.resultData.booster.type != "NONE"){
            CloseSlotWinningsUI();
            BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, SocketManager.resultData.booster.multipliers, "MULTIPLIER");
            yield return BoosterActivatedAnimation();
            StartCoroutine(StartMultiplierWheelGame(BackgroundController.BackgroundType.OrangeFR, SocketManager.initUIData.paylines.symbols[SocketManager.resultData.levelup.level].payout, 0));
            WinningsTextAnimation(0, false);
            yield break;
        }
        else{
            BgController.SwitchBG(BackgroundController.BackgroundType.Base); //ENDING SIMPLE MULTIPLAYER GAME
            TopUIToggle(true);
            ToggleButtonGrp(true);
        }
    }

    private IEnumerator StartMultiplierWheelGame(BackgroundController.BackgroundType type, int basePayout, int MultiplierIndex){
        StartCoroutine(TotalWinningsAnimation(basePayout, false));
        NormalArrowImage.DOFade(1, 0.2f);
        yield return new WaitForSeconds(2f);
        BgController.RotateWheel(); //ROTATE WHEEL
        yield return new WaitForSeconds(2f); //WAITING FOR ROTATION ANIMATION
        
        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = true; //TURNING ON COLLIDER TO STOP THE ROTATIO
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();

        if(MultiplierIndex!=-1){
            stopper.stopAT = SocketManager.resultData.booster.multipliers[MultiplierIndex].ToString(); //TELLING THE STOPPER THE LOCATION TO STOP
            Debug.Log("Stop AT:" + stopper.stopAT);
        }
        else{
            stopper.stopAT = MultiplierIndex.ToString();
            Debug.Log("Stop AT:" + stopper.stopAT);
        }

        wheelStopped = false; 
        stopper.stop = true; //TELLING STOPPER TO STOP THE ROTATION
        
        yield return new WaitUntil(()=> wheelStopped); //WAITING FOR WHEEL TO STOP
        yield return new WaitForSeconds(2f); //WAITING FOR USER TO READ THE RESULT
        Transform ResultImage = stopper.ImageTransform; //FETCHING THE RESULT IMAGE

        ResetNormalArrow();
        
        if(MultiplierIndex!=-1){
            ResultImage.name="Empty";
            RectTransform resultImageRect = ResultImage.GetComponent<RectTransform>();
            MovementImage.rectTransform.position = resultImageRect.position;
            MovementImage.rectTransform.sizeDelta = new Vector2(resultImageRect.rect.width, resultImageRect.rect.height);
            MovementImage.sprite = ResultImage.GetComponent<Image>().sprite;
            ResultImage.GetComponent<Image>().DOFade(0, 0.1f);
            MovementImage.DOFade(1, 0); //USING THE MOVEMENT IMAGE INSTEAD OF THE WHEEL IMAGE
        
            Vector3 tempPosi = MovementImage.transform.localPosition;
            yield return MovementImage.transform.DOLocalMoveY(106, .5f).SetEase(Ease.InBack, 2.5f).WaitForCompletion(); //MOVING THE IMAGE TO THE MIDDLE
            MovementImage.DOFade(0, 0);
            MovementImage.transform.localPosition=tempPosi; //RESETTING THE IMAGE

            Win_Anim_Text.DOFade(0, 0); 
            BlastImageAnimation.StartAnimation(); //STARTING AN ANIMATION
            
            yield return new WaitUntil(()=> BlastImageAnimation.textureArray[^1] == BlastImageAnimation.rendererDelegate.sprite); //WAITIN FOR ANIMATION TO FINISH

            BlastImageAnimation.StopAnimation(); //STOPPING ANIMATION
            
            int winnings = int.Parse(ResultImage.GetComponent<Image>().sprite.name) * basePayout;
            Win_Anim_Text.text = winnings.ToString("f2");
            Win_Anim_Text.DOFade(1, .3f);

            yield return new WaitForSeconds(2f);

            Vector3 tempPosition2 = Win_Anim_Text.transform.localPosition;
            bool scalingStarted = false;
            yield return Win_Anim_Text.transform.DOLocalMoveY(-367f, 0.5f)
            .OnUpdate(()=>{
                if(Win_Anim_Text.transform.localPosition.y < 299f && scalingStarted == false){
                    scalingStarted = true;
                    CloseSlotWinningsUI(true);
                    if(double.TryParse(TotalWin_text.text, out double currUIwin)){
                        winnings+=(int)currUIwin;
                        WinningsTextAnimation(winnings, false);
                    }
                    else{
                        Debug.LogError("Error while parsing string to double");
                    }
                }
            })
            .WaitForCompletion();
            Win_Anim_Text.transform.localPosition = tempPosition2;
        }
        else{ //ENDING EXHAUSTIVE MULTIPLAYER GAME
            TotalWin_text.text = "0";
            BgController.FadeOutChildren();
            // Win_Text_BG.DOFade(0.8f, .8f);
            yield return new WaitForSeconds(1f);
            yield return TotalWinningsAnimation(SocketManager.playerdata.currentWining, true, false);
        }

        yield return new WaitForSeconds(.5f);

        if(SocketManager.resultData.booster.type == "EXHAUSTIVE" && MultiplierIndex!=-1){
            yield return new WaitForSeconds(1f);
            if(SocketManager.resultData.booster.multipliers[MultiplierIndex] != SocketManager.resultData.booster.multipliers[^1]){
                StartCoroutine(StartMultiplierWheelGame(type, basePayout, MultiplierIndex+1));
                yield break;
            }
            else{
                StartCoroutine(StartMultiplierWheelGame(type, basePayout, -1));
                yield break;
            }
        }
        else if(SocketManager.resultData.booster.type == "SIMPLE"){
            yield return TotalWinningsAnimation(SocketManager.playerdata.currentWining, true, false);
        }
        
        BgController.SwitchBG(BackgroundController.BackgroundType.Base); //ENDING SIMPLE MULTIPLAYER GAME
        TopUIToggle(true);
        ToggleButtonGrp(true);
    }

    private void ResetNormalArrow(){
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();
        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = false; //RESET THE STOPPER
        NormalArrowImage.DOFade(0, 0.2f);
        stopper.ImageTransform = null;
        stopper.stop = false;
        stopper.stopAT = "";
    }
    private void ResetDiamondArrow(){
        Stopper stopper = DiamondArrowImage.GetComponent<Stopper>();
        DiamondArrowImage.GetComponent<BoxCollider2D>().enabled = false; //RESET THE STOPPER
        DiamondArrowImage.DOFade(0, 0.2f);
        stopper.ImageTransform = null;
        stopper.stop = false;
        stopper.stopAT = "";
    }

    private IEnumerator TotalWinningsAnimation(double amt, bool ShowTextAnimation = true, bool ToggleButtonsInTheEnd = true){
        if(ShowTextAnimation) WinningsTextAnimation(amt, ToggleButtonsInTheEnd);
        Win_Anim_Text.text = amt.ToString("f2");
        if(Win_Anim_Text.transform.localScale==Vector3.zero) Win_Anim_Text.transform.localScale=Vector3.one;
        Win_Text_BG.DOFade(.8f, 0.5f);
        yield return Win_Anim_Text.DOFade(1f, 0.5f);
    }

    private void CloseSlotWinningsUI(bool scaleAnim=false){
        if(scaleAnim){
            Win_Anim_Text.transform.DOScale(0, 0.3f);
        }
        else{
            Win_Anim_Text.DOFade(0f, 0.3f);
        }
        Win_Text_BG.DOFade(0f, 0.3f);
    }

    private IEnumerator BoosterActivatedAnimation(){
        Vector3 tempPosi1=BoosterImage.transform.localPosition;
        Vector3 tempPosi2=ActivatedImage.transform.localPosition;
        BoosterImage.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutExpo);
        yield return ActivatedImage.transform.DOLocalMoveX(0, .5f).SetEase(Ease.OutExpo);
        yield return new WaitForSeconds(1f);

        BoosterImage.transform.DOLocalMoveX(2173f, .5f);
        yield return ActivatedImage.transform.DOLocalMoveX(-2173f, .5f).OnComplete(()=> {
            BoosterImage.transform.localPosition = tempPosi1;
            ActivatedImage.transform.localPosition = tempPosi2;
        });
    }

    private void TopUIToggle(bool toggle){
        if(toggle){
            TopPayoutUI.DOFade(1, 0.5f);
        }
        else{
            TopPayoutUI.DOFade(0, 0.5f);
        }
    }

    private void JokerUIToggle(bool toggle)
    {
        if(toggle){
            TopPayoutUI.DOFade(0, 0.5f);
            JokerCountText.text = "0 OF 3 JOKERS COLLECTED";
            Glow_Minor_Image.DOFade(0,0);
            Glow_Major_Image.DOFade(0,0);
            Glow_Grand_Image.DOFade(0,0);
            JokerUI.DOFade(1, 0.5f);
        }
        else{
            TopPayoutUI.DOFade(1, 0.5f);
            JokerUI.DOFade(0, 0.5f);
        }
    }

    private void WinningsTextAnimation(double amount, bool ToggleButtonsInTheEnd = true)
    {
        if(double.TryParse(TotalWin_text.text, out double WinAmt)){
            DOTween.To(() => WinAmt, (val) => WinAmt = val, amount, 0.8f)
            .OnUpdate(() =>{
                if(TotalWin_text) TotalWin_text.text = WinAmt.ToString("f2");
            })
            .OnComplete(()=>{
                if(ToggleButtonsInTheEnd) ToggleButtonGrp(true);
            });
        }
    }

    private void BalanceDeduction()
    {
        double bet = 0;
        double balance = 0;
        try
        {
            bet = double.Parse(TotalBet_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }

        try
        {
            balance = double.Parse(Balance_text.text);
        }
        catch (Exception e)
        {
            Debug.Log("Error while conversion " + e.Message);
        }
        double initAmount = balance;

        balance = balance - bet;

        DOTween.To(() => initAmount, (val) => initAmount = val, balance, 0.8f).OnUpdate(() =>
        {
            if (Balance_text) Balance_text.text = initAmount.ToString("f2");
        });
    }

    //generate the payout lines generated 
    private void CheckPayoutLineBackend(List<int> LineId, List<string> points_AnimString, double jackpot = 0)
    {
        // List<int> points_anim = null;
        // if (LineId.Count > 0 || points_AnimString.Count > 0)
        // {
        //     if (jackpot > 0)
        //     {
        //         if (audioController) audioController.PlayWLAudio("megaWin");
        //         for (int i = 0; i < Tempimages.Count; i++)
        //         {
        //             for (int k = 0; k < Tempimages[i].slotImages.Count; k++)
        //             {
        //                 StartGameAnimation(Tempimages[i].slotImages[k].gameObject);
        //             }
        //         }
        //     }
        //     else
        //     {
        //         if (audioController) audioController.PlayWLAudio("win");
        //         for (int i = 0; i < points_AnimString.Count; i++)
        //         {
        //             points_anim = points_AnimString[i]?.Split(',')?.Select(Int32.Parse)?.ToList();

        //             for (int k = 0; k < points_anim.Count; k++)
        //             {
        //                 if (points_anim[k] >= 10)
        //                 {
        //                     StartGameAnimation(Tempimages[(points_anim[k] / 10) % 10].slotImages[points_anim[k] % 10].gameObject);
        //                 }
        //                 else
        //                 {
        //                     StartGameAnimation(Tempimages[0].slotImages[points_anim[k]].gameObject);
        //                 }
        //             }
        //         }
        //     }

        //     if (!SocketManager.resultData.freeSpins.isNewAdded)
        //     {
        //         if (SkipWinAnimation_Button) SkipWinAnimation_Button.gameObject.SetActive(true);
        //     }

        //     if (IsFreeSpin && !SocketManager.resultData.freeSpins.isNewAdded)
        //     {
        //         if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.gameObject.SetActive(true);
        //     }
        // }
        // else
        // {
        //     if (audioController) audioController.StopWLAaudio();
        // }

        // CheckSpinAudio = false;
    }

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    void ToggleButtonGrp(bool toggle)
    {
        if(SlotStart_Button) SlotStart_Button.gameObject.SetActive(toggle);
        if (AutoSpin_Button && !IsAutoSpin) AutoSpin_Button.gameObject.SetActive(toggle);
        if (LineBetPlus_Button) LineBetPlus_Button.interactable = toggle;
        if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = toggle;
        if (LineBetMinus_Button) LineBetMinus_Button.interactable = toggle;
        if (TotalBetMinus_Button) TotalBetMinus_Button.interactable = toggle;
    }

    //Start the icons animation
    private void StartGameAnimation(GameObject animObjects)
    {
        ImageAnimation temp = animObjects.GetComponent<ImageAnimation>();
        if (temp.textureArray.Count > 0)
        {
            temp.StartAnimation();
            TempList.Add(temp);
        }
    }

    //Stop the icons animation
    // internal void StopGameAnimation()
    // {
        // if (BoxAnimRoutine != null)
        // {
        //     StopCoroutine(BoxAnimRoutine);
        //     BoxAnimRoutine = null;
        //     WinAnimationFin = true;
        // }
        
        // if (SkipWinAnimation_Button) SkipWinAnimation_Button.gameObject.SetActive(false);
        // if (BonusSkipWinAnimation_Button) BonusSkipWinAnimation_Button.gameObject.SetActive(false);

        // if (TempList.Count > 0)
        // {
        //     for (int i = 0; i < TempList.Count; i++)
        //     {
        //         TempList[i].StopAnimation();
        //     }
        //     TempList.Clear();
        //     TempList.TrimExcess();
        // }
    // }

    #region TweeningCode
    private void InitializeTweening()
    {
        // ShuffleSlot(true); 
        Slot_Transform.DOLocalMoveY(-2000f, 1.2f)
        .SetEase(Ease.InBack, 1.8f)
        .OnComplete(()=> {
            Slot_Transform.localPosition = new Vector3(Slot_Transform.position.x, 2500f);

            slotTween = Slot_Transform.DOLocalMoveY(-2000f, 1.2f)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.Linear)
            .OnStepComplete(()=> ShuffleSlot(true));
        });
    }

    private IEnumerator StopTweening()
    {
        yield return null;
        bool IsRegister = false;
        yield return slotTween.OnStepComplete(delegate { IsRegister = true; });
        yield return new WaitUntil(() => IsRegister);

        slotTween.Kill();

        // int tweenpos = (reqpos * IconSizeFactor) - IconSizeFactor;
        slotTween = Slot_Transform.DOLocalMoveY(467f, 1.2f)
        .SetEase(Ease.OutBack, 1.8f);

        if (audioController) audioController.PlayWLAudio("spinStop");
        yield return slotTween.WaitForCompletion();
        slotTween.Kill();
    }
    #endregion

}

[Serializable]
public class Animations
{
    public List<Sprite> Animation = new List<Sprite>();
}