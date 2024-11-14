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
    [Header("Slot Images")]
    [SerializeField] private Image[] TotalSlotImages;     //class to store total images
    [SerializeField] private Image ResultImage;     //class to store the result matrix
    [Header("Slots Transforms")]
    [SerializeField] private Transform Slot_Transform;
    [Header("Animated Sprites")]
    [SerializeField] private List<Animations> SlotAnimations;
    [Header("Images")]
    [SerializeField] private Image[] TopPurpleTransforms;
    [SerializeField] private Image DiamondArrowImage;
    [SerializeField] private Image Win_Text_BG;
    [SerializeField] private Image BoosterImage;
    [SerializeField] private Image ActivatedImage;
    [SerializeField] private Image NormalArrowImage;
    [SerializeField] private Image MovementImage;
    [Header("Buttons")]
    [SerializeField] private Button SlotStart_Button;
    [SerializeField] private Button AutoSpin_Button;
    [SerializeField] private Button AutoSpinStop_Button;
    [SerializeField] private Button TotalBetPlus_Button;
    [SerializeField] private Button TotalBetMinus_Button;
    [SerializeField] private Button FSContinue_Button;
    [SerializeField] private Button FSendContinue_Button;
    [Header("Texts")]
    [SerializeField] private TMP_Text Balance_text;
    [SerializeField] private TMP_Text TotalBet_text;
    [SerializeField] private TMP_Text TotalWin_text;
    [SerializeField] private TMP_Text SlotWinnings_Text;
    [SerializeField] private TMP_Text FreeSpinAnim_Text;
    [SerializeField] private TMP_Text TWCount_Text;
    [SerializeField] private TMP_Text FSCount_Text;
    [SerializeField] private TMP_Text FreeSpinPopupCountText;
    [Header ("Canvas Groups")]
    [SerializeField] private CanvasGroup TopPayoutUI;
    [SerializeField] private CanvasGroup FreeSpinsUI;
    [SerializeField] private CanvasGroup TopPurpleUI;
    [SerializeField] private CanvasGroup Joker_Start_UI;
    [Header ("Misc UI")]
    [SerializeField] private ImageAnimation BlastImageAnimation;
    [SerializeField] private Transform FreeSpinPopupUI;
    [SerializeField] private RectTransform FreeSpinTextLoc;
    [Header("Controllers")]
    [SerializeField] private AudioController audioController;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private BackgroundController BgController; 
    [SerializeField] private BonusController _bonusManager;
    [SerializeField] private SocketIOManager SocketManager;
    //private variables
    private GameData FreeSpinData;
    private ImageAnimation SlotImageAnimationScript;
    private Tween slotTween;
    private Coroutine AutoSpinRoutine = null;
    private Coroutine tweenroutine;
    private bool IsAutoSpin = false;
    private bool IsFreeSpin = false;
    private bool IsSpinning = false;
    private bool CheckSpinAudio = false;
    private int BetCounter = 0;
    private double currentBalance = 0;
    private double currentTotalBet = 0;
    private int multiplierWinnings;
    private int freeSpinCount;
    private int FreeSpinWinnings;
    private bool EmptyResult = false;
    //internal variables
    internal bool wheelStopped = false;
    internal bool featuredTriggered = false;    

    private void Start()
    {
        SlotImageAnimationScript = ResultImage.GetComponent<ImageAnimation>();
        IsAutoSpin = false;

        if (FSContinue_Button) FSContinue_Button.onClick.RemoveAllListeners();
        if (FSContinue_Button) FSContinue_Button.onClick.AddListener(delegate
        {
            StartCoroutine(StartFreeSpinGame());
        });

        if(FSendContinue_Button) FSendContinue_Button.onClick.RemoveAllListeners();
        if(FSendContinue_Button) FSendContinue_Button.onClick.AddListener(()=> {
            EndFreeSpin();
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
    }

    #region Autospin
    private void AutoSpin()
    {
        if (!IsAutoSpin)
        {
            ToggleButtonGrp(false);
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
            yield return new WaitForSeconds(3f);
        }
    }

    private IEnumerator StopAutoSpinCoroutine()
    {
        if (AutoSpinStop_Button) AutoSpinStop_Button.gameObject.SetActive(false);
        yield return new WaitUntil(() => !IsSpinning);
        if (AutoSpinRoutine != null || tweenroutine != null)
        {
            StopCoroutine(AutoSpinRoutine);
            StopCoroutine(tweenroutine);
            tweenroutine = null;
            AutoSpinRoutine = null;
            IsAutoSpin = false;
            if(!featuredTriggered) ToggleButtonGrp(true);
            StopCoroutine(StopAutoSpinCoroutine());
        }
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
                if (TotalBetPlus_Button) TotalBetPlus_Button.interactable = false;
            }
            if (BetCounter > 0)
            {
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
                if(TotalBetMinus_Button) TotalBetMinus_Button.interactable = false;
            }
            if (BetCounter < SocketManager.initialData.Bets.Count - 1)
            {
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
        const float selectionProbability = 0.5f;
        int maxRandomIndex = SlotSymbols.Count() - 3;

        for (int i = 0; i < TotalSlotImages.Length; i++)
        {
            // Skip index 6 if midTween is true
            if (midTween && (i == 6))
            {
                continue;
            }
            else if(midTween && (i == 5 || i ==4) && EmptyResult){
                TotalSlotImages[i].sprite = GetRandomSprite(1f, maxRandomIndex);
            }

            // Set slot sprite based on random selection
            TotalSlotImages[i].sprite = GetRandomSprite(selectionProbability, maxRandomIndex);
        }
    }

    // Helper function to select a sprite based on probability
    private Sprite GetRandomSprite(float probability, int maxIndex)
    {
        if (UnityEngine.Random.Range(0f, 1f) <= probability)
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
    private void PopulateAnimationSprites(int index = 0)
    {
        SlotImageAnimationScript.textureArray.Clear();
        SlotImageAnimationScript.textureArray.TrimExcess();
        int startIndex;
        int lastIndex;
        if(!IsFreeSpin){
            startIndex = SocketManager.resultData.resultSymbols;
            lastIndex = SocketManager.resultData.levelup.level;
        }
        else{
            startIndex = FreeSpinData.freeSpinResponse.symbols[index];
            lastIndex = FreeSpinData.freeSpinResponse.levelUp[index].level;
        }
        
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
        featuredTriggered = false;
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
        if (currentBalance < currentTotalBet) // Check if balance is sufficient to place the bet
        {
            CompareBalance();
            IsSpinning = false;
            StopAutoSpin();
            yield return new WaitForSeconds(1);
            yield break;
        }
        IsSpinning = true;

        BalanceDeduction();

        InitializeTweening();

        SocketManager.AccumulateResult(BetCounter);
        yield return new WaitUntil(() => SocketManager.isResultdone);
        currentBalance = SocketManager.playerdata.Balance;

        yield return new WaitForSeconds(1f);

        int resultID = SocketManager.resultData.resultSymbols;
        if(resultID == 0){
            EmptyResult = true;
        }
        if(ResultImage) ResultImage.sprite = SlotSymbols[resultID];

        yield return new WaitForSeconds(1f);

        yield return StopTweening();

        yield return new WaitForSeconds(0.3f);

        if(SocketManager.resultData.freeSpinResponse.isTriggered){
            IsFreeSpin = true;
            FreeSpinData = SocketManager.resultData;
            featuredTriggered = true;
            StopAutoSpin();
            yield return new WaitForSeconds(1.5f);
            TopUIToggle(false);
            BgController.SwitchBG(BackgroundController.BackgroundType.FreeSpin);
            DiamondArrowImage.DOFade(1, 0.5f);

            yield return new WaitForSeconds(2f);
            BgController.RotateWheel(); //ROTATE WHEEL    
            yield return new WaitForSeconds(3f);

            if(FreeSpinData.freespinType == "BLUE"){
                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="BLUE";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(1f);
            }
            else{
                DiamondArrowImage.GetComponent<BoxCollider2D>().enabled=true;
                DiamondArrowImage.GetComponent<Stopper>().stopAT="PURPLE";
                Debug.Log("Stopping at: " + DiamondArrowImage.GetComponent<Stopper>().stopAT);

                wheelStopped =false;
                DiamondArrowImage.GetComponent<Stopper>().stop=true;

                yield return new WaitUntil(()=> wheelStopped);
                yield return new WaitForSeconds(1f);
            }
            DiamondArrowImage.DOFade(0, 0.5f);
            yield return new WaitForSeconds(1f);
            FreeSpinPopupCountText.text = FreeSpinData.freespinType == "BLUE" ? "5" : "10";
            FreeSpinPopupUI.gameObject.SetActive(true);
            CanvasGroup FSstartCG = FreeSpinPopupUI.GetChild(0).GetComponent<CanvasGroup>();
            FSstartCG.interactable = true;
            FSstartCG.blocksRaycasts = true;
            FSstartCG.DOFade(1, 0.5f);
            IsSpinning = false;
            yield break;
        }

        if(SocketManager.resultData.jokerResponse.isTriggered){
            featuredTriggered = true;
            StopAutoSpin();
            yield return new WaitForSeconds(1.5f);
            Joker_Start_UI.transform.parent.gameObject.SetActive(true);
            Joker_Start_UI.interactable = true;
            Joker_Start_UI.blocksRaycasts = true;
            Joker_Start_UI.DOFade(1, 0.5f);
            IsSpinning = false;
            yield break;
        }

        if(SocketManager.resultData.levelup.isLevelUp){
            featuredTriggered = true;
            StopAutoSpin();
            TopUIToggle(false);
            int ResultValue = SocketManager.resultData.levelup.level-SocketManager.resultData.resultSymbols;
            List<int> temp = new()
            {
                ResultValue
            };
            BgController.SwitchBG(BackgroundController.BackgroundType.GreenFR, temp, "LEVEL");
            yield return BoosterActivatedAnimation();
            StartCoroutine(StartLevelBoosterWheelGame(BackgroundController.BackgroundType.GreenFR));
            IsSpinning = false;
            yield break;
        }

        if(SocketManager.resultData.booster.type != "NONE"){
            featuredTriggered = true;
            StopAutoSpin();
            TopUIToggle(false);
            BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, SocketManager.resultData.booster.multipliers, "MULTIPLIER");
            yield return BoosterActivatedAnimation();
            StartCoroutine(StartMultiplierWheelGame(SocketManager.initUIData.paylines.symbols[resultID].payout, 0));
            IsSpinning = false;
            yield break;
        }

        if(SocketManager.playerdata.currentWining > 0 && !SocketManager.resultData.jokerResponse.isTriggered && !SocketManager.resultData.levelup.isLevelUp && SocketManager.resultData.booster.type == "NONE" && SocketManager.resultData.freespinType == "NONE"){
            yield return TotalWinningsAnimation(SocketManager.playerdata.currentWining);
        }
        else{
            ToggleButtonGrp(true);
        }
        IsSpinning = false;
    }
    #endregion

    private void EndFreeSpin(){
        IsFreeSpin = false;
        TotalWin_text.text = "0";
        CanvasGroup FSendUI = FreeSpinPopupUI.GetChild(1).GetComponent<CanvasGroup>();
        FSendUI.interactable = false;
        FSendUI.blocksRaycasts = false;
        FSendUI.DOFade(0, 0.5f);
        FreeSpinsUI.DOFade(0, 0.5f);
        TopPurpleUI.DOFade(0, 0.5f);

        BgController.SwitchBG(BackgroundController.BackgroundType.Base);
        StartCoroutine(TotalWinningsAnimation(FreeSpinData.freeSpinResponse.payout));
        TopUIToggle(true);
        freeSpinCount =0;
    }

    private IEnumerator StartFreeSpinGame(){
        CanvasGroup FSstartCG = FreeSpinPopupUI.GetChild(0).GetComponent<CanvasGroup>();
        FSstartCG.interactable = false;
        FSstartCG.blocksRaycasts = false;
        yield return FSstartCG.DOFade(0, 0.5f).WaitForCompletion();

        freeSpinCount = FreeSpinData.freespinType == "BLUE" ? 5 : 10;
        FSCount_Text.text = freeSpinCount.ToString();
        TWCount_Text.text = "0";
        FreeSpinsUI.DOFade(1, 0.5f);

        if(FreeSpinData.freespinType == "BLUE"){
            BgController.SwitchBG(BackgroundController.BackgroundType.GoldenFR);
        }
        else{
            BgController.SwitchBG(BackgroundController.BackgroundType.PurpleFR);
            PopulateTopSymbolUI(FreeSpinData.freeSpinResponse.topSymbols[0]);
            TopPurpleUI.DOFade(1, 0.5f);
        }
        yield return new WaitForSeconds(2f);    
        for(int i = 0; i<FreeSpinData.freeSpinResponse.symbols.Count;i++){
            yield return FSTweenRoutine(i);
            yield return new WaitForSeconds(2f);
        }

        CanvasGroup FSendUI = FreeSpinPopupUI.GetChild(1).GetComponent<CanvasGroup>();
        if(FreeSpinWinnings != SocketManager.playerdata.currentWining){
            Debug.LogError("Error while checking if the winnings showed is equal to the winngs sent through backend");
        }
        FSendUI.transform.GetChild(2).GetComponent<TMP_Text>().text = FreeSpinWinnings.ToString("f2");
        FSendUI.interactable = true;
        FSendUI.blocksRaycasts = true;
        FSendUI.DOFade(1, 0.5f);
    }

    private void PopulateTopSymbolUI(List<int> topSymbols)
    {
        for(int i = 0; i < TopPurpleTransforms.Length; i++){
            TopPurpleTransforms[i].sprite = SlotSymbols[topSymbols[i]];
        }
    }

    private IEnumerator FSTweenRoutine(int index){
        TotalWin_text.text="0.00";
        CloseSlotWinningsUI();
        if(FreeSpinData.freespinType == "BLUE"){
            freeSpinCount--;
            FSCount_Text.text = freeSpinCount.ToString();
        }
        else{
            freeSpinCount--;
            FSCount_Text.text = freeSpinCount.ToString();
        }

        InitializeTweening();
        yield return new WaitForSeconds(2f);
        int resultID = FreeSpinData.freeSpinResponse.symbols[index];
        if(resultID == 0){
            EmptyResult = true;
        }
        ResultImage.sprite = SlotSymbols[resultID];
        yield return StopTweening();
        yield return new WaitForSeconds(.5f);

        //if purple and symbol matches top ui fade out the symbol
        if(FreeSpinData.freespinType == "PURPLE"){
            foreach(Image i in TopPurpleTransforms){
                if(i.sprite == ResultImage.sprite){
                    yield return i.transform.DOScale(0, 1f).WaitForCompletion();
                    ImageAnimation imageAnimation = i.GetComponent<ImageAnimation>();
                    imageAnimation.StartAnimation();
                    yield return new WaitUntil(() => imageAnimation.rendererDelegate.sprite == imageAnimation.textureArray[^1]);
                    imageAnimation.StopAnimation();
                    i.sprite = SlotSymbols[0];
                    i.transform.DOScale(1, 0);
                }
            }

            bool check = false;
            foreach(Image i in TopPurpleTransforms){
                if(i.sprite != SlotSymbols[0]){
                    check=true;
                }
            }
            if(!check && FreeSpinData.freeSpinResponse.reTriggered[index] == 1){
                Debug.Log("Refilling top purple symbols");
                PopulateTopSymbolUI(FreeSpinData.freeSpinResponse.topSymbols[index+1]);
            }
            yield return new WaitForSeconds(1f);
        }

        //check if free spin count increased and do an animation
        if(FreeSpinData.freespinType == "BLUE"){
            if(FreeSpinData.freeSpinResponse.count[index] > freeSpinCount){
                int count = FreeSpinData.freeSpinResponse.count[index] - freeSpinCount;
                FreeSpinAnim_Text.text = "+"+count.ToString();
                yield return FreeSpinAnim_Text.DOFade(1, 1f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
                Vector3 temp = FreeSpinAnim_Text.transform.localPosition; // Save the local position
                bool scalingStarted = false;
                yield return FreeSpinAnim_Text.transform.DOLocalMove(FreeSpinTextLoc.transform.localPosition, 0.2f)
                .OnUpdate(()=>{
                    if(FreeSpinAnim_Text.transform.position.x>390 && !scalingStarted){
                        scalingStarted = true;
                        FreeSpinAnim_Text.transform.DOScale(0, 0.2f);
                    } 
                }).WaitForCompletion();
                FreeSpinAnim_Text.DOFade(0 ,0);
                FreeSpinAnim_Text.transform.localPosition = temp; // Reset to the saved local position
                FreeSpinAnim_Text.transform.DOScale(1, 0);
                freeSpinCount += count;
                if(FreeSpinData.freeSpinResponse.count[index] != freeSpinCount){
                    Debug.LogError("Free spin count calc is wrong when eqauted to backend data, freeSpinCount: " + freeSpinCount.ToString() + " and backend: " +FreeSpinData.freeSpinResponse.count[index]);
                }
                FSCount_Text.text = freeSpinCount.ToString();
                yield return new WaitForSeconds(1f);
            }
        }
        else{
            if(FreeSpinData.freeSpinResponse.count[index+1]!= 0 && FreeSpinData.freeSpinResponse.count[index+1] > freeSpinCount){
                int count = FreeSpinData.freeSpinResponse.count[index+1] - freeSpinCount;
                FreeSpinAnim_Text.text = "+"+count.ToString();
                yield return FreeSpinAnim_Text.DOFade(1, 1f).WaitForCompletion();
                yield return new WaitForSeconds(1f);
                Vector3 temp = FreeSpinAnim_Text.transform.localPosition; // Save the local position
                bool scalingStarted = false;
                yield return FreeSpinAnim_Text.transform.DOLocalMove(FreeSpinTextLoc.transform.localPosition, 0.2f)
                .OnUpdate(()=>{
                    if(FreeSpinAnim_Text.transform.position.x>390 && !scalingStarted){
                        scalingStarted = true;
                        FreeSpinAnim_Text.transform.DOScale(0, 0.2f);
                    } 
                }).WaitForCompletion();
                FreeSpinAnim_Text.DOFade(0 ,0);
                FreeSpinAnim_Text.transform.localPosition = temp; // Reset to the saved local position
                FreeSpinAnim_Text.transform.DOScale(1, 0);
                freeSpinCount += count;
                if(FreeSpinData.freeSpinResponse.count[index+1] != freeSpinCount){
                    Debug.LogError("Free spin count calc is wrong when eqauted to backend data, freeSpinCount: " + freeSpinCount.ToString() + " and backend: " +FreeSpinData.freeSpinResponse.count[index+1]);
                }
                FSCount_Text.text = freeSpinCount.ToString();
                yield return new WaitForSeconds(1f);
            }
        }
        
        if(FreeSpinData.freeSpinResponse.levelUp[index].isLevelUp){
            int ResultValue = FreeSpinData.freeSpinResponse.levelUp[index].level-resultID;
            List<int> temp = new()
            {
                ResultValue
            };
            BgController.SwitchBG(BackgroundController.BackgroundType.GreenFR, temp, "LEVEL");
            yield return BoosterActivatedAnimation();
            yield return StartLevelBoosterWheelGame(BackgroundController.BackgroundType.GreenFR, true, index);
        }
        else if(FreeSpinData.freeSpinResponse.booster[index].type != "NONE"){
            BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, FreeSpinData.freeSpinResponse.booster[index].multipliers, "MULTIPLIER");
            yield return BoosterActivatedAnimation();
            yield return StartMultiplierWheelGame(SocketManager.initUIData.paylines.symbols[resultID].payout, 0, true, index);
        }
        else if(FreeSpinData.freeSpinResponse.symbols[index]!=0){
            FreeSpinWinningsTextAnimation(SocketManager.initUIData.paylines.symbols[FreeSpinData.freeSpinResponse.symbols[index]].payout);
            yield return TotalWinningsAnimation(SocketManager.initUIData.paylines.symbols[FreeSpinData.freeSpinResponse.symbols[index]].payout, true, false);
        }

    }

    private IEnumerator StartLevelBoosterWheelGame(BackgroundController.BackgroundType type, bool FS = false, int i = 0){
        NormalArrowImage.DOFade(1, 0.2f);
        yield return new WaitForSeconds(2f);
        BgController.RotateWheel(); //ROTATE WHEEL
        yield return new WaitForSeconds(2f); //WAITING FOR ROTATION ANIMATION

        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = true; //TURNING ON COLLIDER TO STOP THE ROTATIO
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();
        int stopAt = 0;
        if(FS){
            stopAt = FreeSpinData.freeSpinResponse.levelUp[i].level-FreeSpinData.freeSpinResponse.symbols[i];
            Debug.Log("FS level id: " + FreeSpinData.freeSpinResponse.levelUp[i].level + " symbol id: " + FreeSpinData.freeSpinResponse.symbols[i]);
        }
        else{
            stopAt = SocketManager.resultData.levelup.level-SocketManager.resultData.resultSymbols;
        }
        stopper.stopAT=stopAt.ToString();
        Debug.Log("Stopping at: " + stopper.stopAT);

        wheelStopped=false;
        stopper.stop=true;

        yield return new WaitUntil(()=> wheelStopped);
        yield return new WaitForSeconds(2f); //WAITING FOR USER TO READ THE RESULT
        Transform ResultImage = stopper.ImageTransform;

        ResetNormalArrow();
        PopulateAnimationSprites(i);
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

        if(!FS) 
            this.ResultImage.sprite = SlotSymbols[SocketManager.resultData.levelup.level];
        else    
            this.ResultImage.sprite = SlotSymbols[FreeSpinData.freeSpinResponse.levelUp[i].level];

        yield return new WaitForSeconds(2f);
        int payout = 0;
        if(!FS){
            payout = SocketManager.initUIData.paylines.symbols[SocketManager.resultData.levelup.level].payout;
            if(SocketManager.resultData.booster.type != "NONE"){
                yield return TotalWinningsAnimation(payout, false);
                yield return new WaitForSeconds(2f);
                CloseSlotWinningsUI();
                BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, SocketManager.resultData.booster.multipliers, "MULTIPLIER");
                yield return BoosterActivatedAnimation();
                StartCoroutine(StartMultiplierWheelGame(SocketManager.initUIData.paylines.symbols[SocketManager.resultData.levelup.level].payout, 0));
                WinningsTextAnimation(0, false);
                yield break;
            }
            else{
                yield return TotalWinningsAnimation(payout, true, false);
                yield return new WaitForSeconds(2f);
                BgController.SwitchBG(BackgroundController.BackgroundType.Base); //ENDING SIMPLE MULTIPLAYER GAME
                TopUIToggle(true);
                ToggleButtonGrp(true);
            }
        }
        else{
            payout = SocketManager.initUIData.paylines.symbols[FreeSpinData.freeSpinResponse.levelUp[i].level].payout;
            if(FreeSpinData.freeSpinResponse.booster[i].type != "NONE"){
                yield return TotalWinningsAnimation(payout, false);
                yield return new WaitForSeconds(2f);
                CloseSlotWinningsUI();
                BgController.SwitchBG(BackgroundController.BackgroundType.OrangeFR, FreeSpinData.freeSpinResponse.booster[i].multipliers, "MULTIPLIER");
                yield return BoosterActivatedAnimation();
                WinningsTextAnimation(0, false);
                yield return StartMultiplierWheelGame(SocketManager.initUIData.paylines.symbols[FreeSpinData.freeSpinResponse.levelUp[i].level].payout, 0, true, i); //CAN BE CHANGED ACCORDING TO FREE SPIN
            }
            else{
                yield return TotalWinningsAnimation(payout, true, false);
                FreeSpinWinningsTextAnimation(payout);
                yield return new WaitForSeconds(2f);
                if(FreeSpinData.freespinType == "BLUE"){
                    BgController.SwitchBG(BackgroundController.BackgroundType.GoldenFR); //ENDING SIMPLE MULTIPLAYER GAME
                }
                else{
                    BgController.SwitchBG(BackgroundController.BackgroundType.PurpleFR); //ENDING SIMPLE MULTIPLAYER GAME
                }
            }
        }
    }

    
    private IEnumerator StartMultiplierWheelGame(int basePayout, int MultiplierIndex, bool FS = false, int i = 0){
        if(MultiplierIndex == 0) multiplierWinnings = 0;
        StartCoroutine(TotalWinningsAnimation(basePayout, false));
        NormalArrowImage.DOFade(1, 0.2f);
        yield return new WaitForSeconds(2f);
        BgController.RotateWheel(); //ROTATE WHEEL
        yield return new WaitForSeconds(2f); //WAITING FOR ROTATION ANIMATION
        
        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = true; //TURNING ON COLLIDER TO STOP THE ROTATIO
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();

        if(MultiplierIndex!=-1){
            if(!FS){
                stopper.stopAT = SocketManager.resultData.booster.multipliers[MultiplierIndex].ToString(); //TELLING THE STOPPER THE LOCATION TO STOP
            }
            else{
                stopper.stopAT = FreeSpinData.freeSpinResponse.booster[i].multipliers[MultiplierIndex].ToString();
            }
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

            SlotWinnings_Text.DOFade(0, 0); 
            BlastImageAnimation.StartAnimation(); //STARTING AN ANIMATION
            
            yield return new WaitUntil(()=> BlastImageAnimation.textureArray[^1] == BlastImageAnimation.rendererDelegate.sprite); //WAITIN FOR ANIMATION TO FINISH

            BlastImageAnimation.StopAnimation(); //STOPPING ANIMATION
            
            int winnings = int.Parse(ResultImage.GetComponent<Image>().sprite.name) * basePayout;
            multiplierWinnings += winnings;
            SlotWinnings_Text.text = winnings.ToString("f2");
            SlotWinnings_Text.DOFade(1, .3f);

            yield return new WaitForSeconds(2f);

            Vector3 tempPosition2 = SlotWinnings_Text.transform.localPosition;
            bool scalingStarted = false;
            yield return SlotWinnings_Text.transform.DOLocalMoveY(-367f, 0.5f)
            .OnUpdate(()=>{
                if(SlotWinnings_Text.transform.localPosition.y < 299f && scalingStarted == false){
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
            SlotWinnings_Text.transform.localPosition = tempPosition2;
        }
        else{ //ENDING EXHAUSTIVE MULTIPLAYER GAME
            TotalWin_text.text = "0";
            BgController.FadeOutChildren();
            // Win_Text_BG.DOFade(0.8f, .8f);
            yield return new WaitForSeconds(1f);
            if(!FS){
                if(multiplierWinnings!=SocketManager.playerdata.currentWining){
                    Debug.LogError("Error while checking if winnings match multiplier Winnings: "+ multiplierWinnings + "SocketManager.playerdata.currentWining: "+ SocketManager.playerdata.currentWining);
                }
                yield return TotalWinningsAnimation(multiplierWinnings, true, false);
            } 
            else{
                FreeSpinWinningsTextAnimation(multiplierWinnings);
                yield return TotalWinningsAnimation(multiplierWinnings, true, false);
            }
        }

        yield return new WaitForSeconds(.5f);

        if(FS){
            if(FreeSpinData.freeSpinResponse.booster[i].type == "EXHAUSTIVE" && MultiplierIndex!=-1){
                yield return new WaitForSeconds(1f);
                if(FreeSpinData.freeSpinResponse.booster[i].multipliers[MultiplierIndex] != FreeSpinData.freeSpinResponse.booster[i].multipliers[^1]){
                    yield return StartMultiplierWheelGame(basePayout, MultiplierIndex+1, true, i);
                    yield break;
                }
                else{
                    yield return StartMultiplierWheelGame(basePayout, -1, true, i);
                }
            }
            else if(FreeSpinData.freeSpinResponse.booster[i].type == "SIMPLE"){
                FreeSpinWinningsTextAnimation(multiplierWinnings);
                yield return TotalWinningsAnimation(multiplierWinnings, true, false);
            }
        }
        else{
            if(SocketManager.resultData.booster.type == "EXHAUSTIVE" && MultiplierIndex!=-1){
                yield return new WaitForSeconds(1f);
                if(SocketManager.resultData.booster.multipliers[MultiplierIndex] != SocketManager.resultData.booster.multipliers[^1]){
                    StartCoroutine(StartMultiplierWheelGame(basePayout, MultiplierIndex+1));
                    yield break;
                }
                else{
                    StartCoroutine(StartMultiplierWheelGame(basePayout, -1));
                    yield break;
                }
            }
            else if(SocketManager.resultData.booster.type == "SIMPLE"){
                yield return TotalWinningsAnimation(multiplierWinnings, true, false);
            }
        }
        
        if(!FS){
            BgController.SwitchBG(BackgroundController.BackgroundType.Base); //ENDING SIMPLE MULTIPLAYER GAME
            TopUIToggle(true);
            ToggleButtonGrp(true);
        }
        else{
            if(FreeSpinData.freespinType == "BLUE"){
                BgController.SwitchBG(BackgroundController.BackgroundType.GoldenFR); //ENDING SIMPLE MULTIPLAYER GAME
            }
            else{
                BgController.SwitchBG(BackgroundController.BackgroundType.PurpleFR); //ENDING SIMPLE MULTIPLAYER GAME
            }
        }
    }

    private void ResetNormalArrow(){
        Stopper stopper = NormalArrowImage.GetComponent<Stopper>();
        NormalArrowImage.GetComponent<BoxCollider2D>().enabled = false; //RESET THE STOPPER
        NormalArrowImage.DOFade(0, 0.2f);
        stopper.ImageTransform = null;
        stopper.stop = false;
        stopper.stopAT = "";
    }

    internal IEnumerator TotalWinningsAnimation(double amt, bool ShowTextAnimation = true, bool ToggleButtonsInTheEnd = true){
        if(ShowTextAnimation) WinningsTextAnimation(amt, ToggleButtonsInTheEnd);
        SlotWinnings_Text.text = amt.ToString("f2");
        if(SlotWinnings_Text.transform.localScale==Vector3.zero) SlotWinnings_Text.transform.localScale=Vector3.one;
        Win_Text_BG.DOFade(.8f, 0.5f);
        yield return SlotWinnings_Text.DOFade(1f, 0.5f);
    }

    private void CloseSlotWinningsUI(bool scaleAnim=false){
        if(scaleAnim){
            SlotWinnings_Text.transform.DOScale(0, 0.3f);
        }
        else{
            SlotWinnings_Text.DOFade(0f, 0.3f);
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

    private void FreeSpinWinningsTextAnimation(double amount){
        double.TryParse(TWCount_Text.text, out double TWCount);
        DOTween.To(() => TWCount, (val) => TWCount = val, amount + TWCount, 0.8f)
        .OnUpdate(() =>{
            if(TWCount_Text) TWCount_Text.text = TWCount.ToString("f2");
        });
        FreeSpinWinnings = (int)(amount + TWCount);
    }

    private void  BalanceDeduction()
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

    internal void CallCloseSocket()
    {
        SocketManager.CloseSocket();
    }

    internal void ToggleButtonGrp(bool toggle)
    {
        if(SlotStart_Button && !IsAutoSpin) SlotStart_Button.gameObject.SetActive(toggle);
        if (AutoSpin_Button && !IsAutoSpin) AutoSpin_Button.gameObject.SetActive(toggle);
        if (TotalBetPlus_Button && !IsAutoSpin) TotalBetPlus_Button.interactable = toggle;
        if (TotalBetMinus_Button && !IsAutoSpin) TotalBetMinus_Button.interactable = toggle;
    }

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
        EmptyResult = false;

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