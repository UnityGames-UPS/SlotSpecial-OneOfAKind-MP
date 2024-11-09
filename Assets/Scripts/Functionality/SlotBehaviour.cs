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

public class SlotBehaviour : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] SlotSymbols;  //images taken initially

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
    [SerializeField] private Sprite[] Bonus_Sprite;
    [SerializeField] private Sprite[] Cleopatra_Sprite;

    [Header("Miscellaneous UI")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text Win_Anim_Text;
    [SerializeField] private Image Win_Text_BG;
    [SerializeField] private TMP_Text BigWin_Text;
    [SerializeField] private TMP_Text BonusWin_Text;
    [SerializeField] private CanvasGroup TopPayoutUI;
    [SerializeField] private Image BoosterImage;
    [SerializeField] private Image ActivatedImage;
    [SerializeField] private Image ScatterArrowImage;
    [SerializeField] private Image NormalArrowImage;
    [SerializeField] private Image MovementImage;
    [SerializeField] private ImageAnimation BlastImageAnimation;

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


    private void Start()
    {
        IsAutoSpin = false;

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
    private void PopulateAnimationSprites(ImageAnimation animScript, int val)
    {
        animScript.textureArray.Clear();
        animScript.textureArray.TrimExcess();
        switch (val)
        {
            case 11:
                for(int i=0; i < Bonus_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Bonus_Sprite[i]);
                }
                animScript.AnimationSpeed = 12f;
                break;

            case 12:
                for(int i=0;i<Cleopatra_Sprite.Length; i++)
                {
                    animScript.textureArray.Add(Cleopatra_Sprite[i]);
                }
                animScript.AnimationSpeed = 15f;
                break;
        }
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

        // if (SlotStart_Button) SlotStart_Button.gameObject.SetActive(false);
        ToggleButtonGrp(false);
        if (TotalWin_text) TotalWin_text.text = "0.00";
        if (Win_Anim_Text.alpha!=0) Win_Anim_Text.DOFade(0, 0.5f);
        if(Win_Text_BG) Win_Text_BG.DOFade(0f, 0.5f);

        tweenroutine = StartCoroutine(TweenRoutine());
        // StopGameAnimation();
        
        //PayCalculator.ResetLines();
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
        // ToggleButtonGrp(false);

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

        // SlotStart_Button.gameObject.SetActive(true); //testing line, remove after

        if(SocketManager.resultData.booster.type != "NONE"){
            TopUIToggle(false);
            BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, SocketManager.resultData.booster.multipliers);
            yield return BoosterActivatedAnimation();
            yield return new WaitForSeconds(1f);
            StartCoroutine(StartWheelGame(BackgroundController.BackgroundType.OrangeFR, SocketManager.initUIData.paylines.symbols[resultID].payout, 0));
            yield break;
        }

        if(SocketManager.playerdata.currentWining > 0 && !SocketManager.resultData.jokerResponse.isTriggered && !SocketManager.resultData.levelup.isLevelUp && SocketManager.resultData.booster.type == "NONE" && SocketManager.resultData.freespinType == "NONE"){
            yield return TotalWinningsAnimation(SocketManager.playerdata.currentWining);
        }
        else{
            ToggleButtonGrp(true);
        }
        // if(SocketManager.playerdata.currentWining <= 0 && SocketManager.resultData.jackpot <= 0 && !SocketManager.resultData.freeSpins.isNewAdded)
        // {
        //     audioController.PlayWLAudio("lose");
        // }

        // if (SocketManager.resultData.freeSpins.isNewAdded)
        // {
        //     Debug.Log(IsFreeSpin ? "Bonus In Bonus" : "First Time Bonus");

        //     yield return new WaitForSeconds(1.5f);

        //     if (BoxAnimRoutine != null && !WinAnimationFin)
        //     {
        //         yield return new WaitUntil(() => WinAnimationFin);
        //         StopGameAnimation();
        //     }

        //     yield return new WaitForSeconds(1f);

        //     if (!IsFreeSpin)
        //     {
        //         _bonusManager.StartBonus(SocketManager.resultData.freeSpins.count);
        //     }
        //     else
        //     {
        //         IsFreeSpin = false;
        //         yield return StartCoroutine(_bonusManager.BonusInBonus());
        //     }

        //     if (IsAutoSpin)
        //     {
        //         IsSpinning = false;
        //         StopAutoSpin();
        //     }

        // }

        // if (!IsAutoSpin && !IsFreeSpin) // Reset spinning state and toggle buttons
        // {
        //     ToggleButtonGrp(true);
        //     IsSpinning = false;
        // }
        // else
        // {
        //     IsSpinning = false;
        //     yield return new WaitForSeconds(2f);
        // }
    }
    #endregion

    private IEnumerator StartWheelGame(BackgroundController.BackgroundType type, int basePayout, int MultiplierIndex){
        
        StartCoroutine(TotalWinningsAnimation(basePayout, false));
        NormalArrowImage.DOFade(1, 0.2f);
        BgController.RotateWheel(type); //ROTATE WHEEL
        yield return new WaitForSeconds(2f); //WAITING FOR ROTATION ANIMATION
        
        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = true; //TURNING ON COLLIDER TO STOP THE ROTATION
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();
        if(MultiplierIndex!=-1){
            stopper.stopAT = SocketManager.resultData.booster.multipliers[MultiplierIndex].ToString(); //TELLING THE STOPPER THE LOCATION TO STOP\
            Debug.Log("Stop AT:" + stopper.stopAT);
        }
        else{
            stopper.stopAT = MultiplierIndex.ToString();
            Debug.Log("Stop AT:" + stopper.stopAT);
        }
        stopper.stop = true; //TELLING STOPPER TO STOP THE ROTATION
        
        wheelStopped = false; 
        yield return new WaitUntil(()=> wheelStopped); //WAITING FOR WHEEL TO STOP
        Transform ResultImage = stopper.ImageTransform; //FETCHING THE RESULT IMAGE

        NormalArrowImage.DOFade(0, 0.2f);
        stopper.ImageTransform = null;
        stopper.stop = false;
        stopper.stopAT = "";
        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = false; //RESET THE STOPPER
        
        yield return new WaitForSeconds(1f); //WAITING FOR USER TO READ THE RESULT
        if(MultiplierIndex!=-1){
            ResultImage.GetComponent<Image>().DOFade(0, 0.1f);
            MovementImage.sprite = ResultImage.GetComponent<Image>().sprite;
            MovementImage.DOFade(1, 0); //USING THE MOVEMENT IMAGE INSTEAD OF THE WHEEL IMAGE
        
            Vector3 tempPosi = MovementImage.transform.localPosition;
            yield return MovementImage.transform.DOLocalMoveY(106, .5f).SetEase(Ease.InBack, 2.5f).WaitForCompletion(); //MOVING THE IMAGE TO THE MIDDLE
            MovementImage.DOFade(0, 0);
            MovementImage.transform.localPosition=tempPosi; //RESETTING THE IMAGE

            Win_Anim_Text.DOFade(0, 0); 
            BlastImageAnimation.StartAnimation(); //STARTING AN ANIMATION
            
            yield return new WaitUntil(()=> BlastImageAnimation.textureArray[^1] == BlastImageAnimation.rendererDelegate.sprite);

            BlastImageAnimation.StopAnimation();
            
            int winnings = int.Parse(ResultImage.GetComponent<Image>().sprite.name) * basePayout;
            Win_Anim_Text.text = winnings.ToString("f2");
            Win_Anim_Text.DOFade(1, .3f);

            yield return new WaitForSeconds(2f);

            Vector3 tempPosition2 = Win_Anim_Text.transform.localPosition;
            yield return Win_Anim_Text.transform.DOLocalMoveY(-367f, 0.5f).WaitForCompletion();
            CloseSlotWinningsUI();
            Win_Anim_Text.transform.localPosition = tempPosition2;

            WinningsTextAnimation(winnings);
        }
        else{
            TotalWin_text.text = "0";
            BgController.FadeOutChildren();
            Win_Text_BG.DOFade(1, .8f);
            yield return new WaitForSeconds(1f);
            yield return TotalWinningsAnimation(SocketManager.playerdata.currentWining);
        }

        yield return new WaitForSeconds(.5f);

        if(SocketManager.resultData.booster.type == "EXHAUSTIVE" && MultiplierIndex!=-1){
            if(SocketManager.resultData.booster.multipliers[MultiplierIndex] != SocketManager.resultData.booster.multipliers[^1]){
                StartCoroutine(StartWheelGame(type, basePayout, MultiplierIndex+1));
                yield break;
            }
            else{
                StartCoroutine(StartWheelGame(type, basePayout, -1));
                yield break;
            }
        }
        
        BgController.SwitchBG(BackgroundController.BackgroundType.Base);
        TopUIToggle(true);
        ToggleButtonGrp(true);
    }

    private IEnumerator TotalWinningsAnimation(double amt, bool ShowTextAnimation = true){
        if(ShowTextAnimation) WinningsTextAnimation(amt);
        Win_Anim_Text.text = amt.ToString("f2");
        Win_Text_BG.DOFade(.8f, 0.5f);
        yield return Win_Anim_Text.DOFade(1f, 0.5f);
    }

    private void CloseSlotWinningsUI(){
        Win_Anim_Text.DOFade(0f, 0.3f);
        Win_Text_BG.DOFade(0f, 0.2f);
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

    internal void CheckWinPopups()
    {
        // if (SocketManager.resultData.WinAmout >= currentTotalBet * 5 && SocketManager.resultData.WinAmout < currentTotalBet * 10)
        // {
        //     uiManager.PopulateWin(1);
        // }
        // else if (SocketManager.resultData.WinAmout >= currentTotalBet * 10 && SocketManager.resultData.WinAmout < currentTotalBet * 15)
        // {
        //     uiManager.PopulateWin(2);
        // }
        // else if (SocketManager.resultData.WinAmout >= currentTotalBet * 15)
        // {
        //     uiManager.PopulateWin(3);
        // }
        // else
        // {
        //     CheckPopups = false;
        // }
    }

    private void WinningsTextAnimation(double amount)
    {
        if(double.TryParse(TotalWin_text.text, out double WinAmt)){
            DOTween.To(() => WinAmt, (val) => WinAmt = val, amount, 0.8f)
            .OnUpdate(() =>{
                if(TotalWin_text) TotalWin_text.text = WinAmt.ToString("f2");
            })
            .OnComplete(()=>{
                ToggleButtonGrp(true);
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
        Slot_Transform.DOLocalMoveY(-2518f, 1f)
        .SetEase(Ease.InBack, 1.8f)
        .OnComplete(()=> {
            Slot_Transform.localPosition = new Vector3(Slot_Transform.position.x, 2536);

            slotTween = Slot_Transform.DOLocalMoveY(-2518f - (512*2), 1f)
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
        slotTween = Slot_Transform.DOLocalMoveY(303f, 1f)
        .SetEase(Ease.OutBack, 1.8f);

        if (audioController) audioController.PlayWLAudio("spinStop");
        yield return slotTween.WaitForCompletion();
        slotTween.Kill();
    }
    #endregion

}

[Serializable]
public class SlotImage
{
    public List<Image> slotImages = new List<Image>(10);
}