using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
public class BackgroundController : MonoBehaviour
{
  [Header("Background Images")]
  [SerializeField] private Image NR_BG_Image;
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
  [SerializeField] private CanvasGroup FreeSpinCircle_CG;
  [Header("Sprites")]
  [SerializeField] private Sprite[] MultiplierSprites;
  [SerializeField] private Sprite[] LevelSprites;
  [SerializeField] private Sprite BlueJoker_Sprite;
  [SerializeField] private Sprite GreenJoker_Sprite;
  [SerializeField] private Sprite RedJoker_Sprite;
  [SerializeField] private Sprite Empty_Sprite;
  [Header("Rotation Tween Duration")]
  [SerializeField] private float NRTweenDuration = 30;
  [SerializeField] private float FRTweenDuration = 5;
  [SerializeField] private Transform startsImageAnim_Transform;
  [SerializeField] private AudioController audioController;
  private Tween NR_RotateTween;
  private Tween BlueFR_RotateTween, GoldenFR_RotateTween, OrangeFR_RotateTween, GreenFR_RotateTween, PurpleFR_RotateTween, wheelRoutine;
  private Tween GlowRotation1;
  private Tween GlowRotation2;

  public enum BackgroundType
  {
    Base,
    BlueFR,
    GreenFR,
    GoldenFR,
    OrangeFR,
    PurpleFR,
    FreeSpin,
  }
  private Dictionary<BackgroundType, (CanvasGroup CG, CanvasGroup CircleCG)> backgrounds;
  private BackgroundType currentBG;

  private void Awake()
  {
    backgrounds = new Dictionary<BackgroundType, (CanvasGroup, CanvasGroup)> {
            { BackgroundType.Base, (NRBG_CG, NRBGCircle_CG) },
            { BackgroundType.BlueFR, (BlueFR_CG, BlueFRCircle_CG) },
            { BackgroundType.GreenFR, (GreenFR_CG, GreenFRCircle_CG) },
            { BackgroundType.GoldenFR, (GoldenFR_CG, GoldenFRCircle_CG) },
            { BackgroundType.OrangeFR, (OrangeFR_CG, OrangeFRCircle_CG) },
            { BackgroundType.PurpleFR, (PurpleFR_CG, PurpleFRCircle_CG) },
            { BackgroundType.FreeSpin, (NRBG_CG, FreeSpinCircle_CG) }
        };
    startsImageAnim_Transform.DORotate(new Vector3(0, 0, 360), 300, RotateMode.FastBeyond360).SetEase(Ease.Linear);
  }
  private void Start()
  {
    currentBG = BackgroundType.Base;
    RotateBG();
  }

  internal void SwitchBG(BackgroundType bgType, List<int> values = null, string type = null)
  {
    if (bgType != BackgroundType.OrangeFR && bgType != BackgroundType.GreenFR && bgType != BackgroundType.Base && bgType != BackgroundType.FreeSpin) { audioController.PlayWLAudio("BGChange"); }
    BackgroundType temp = currentBG;
    currentBG = bgType;

    foreach (var kvp in backgrounds)
    {
      if (kvp.Key != bgType)
      {
        if (kvp.Value.CG != backgrounds[bgType].CG) kvp.Value.CG.DOFade(0, 1f);
        kvp.Value.CircleCG.DOFade(0, 1f);
        if (GlowRotation1 != null || GlowRotation2 != null)
        {
          GlowRotation1.Kill();
          GlowRotation2.Kill();
        }
      }
    }

    if (backgrounds[temp].CG != backgrounds[currentBG].CG) DOVirtual.DelayedCall(1f, () => StopRotation(temp.ToString()));

    if (bgType == BackgroundType.Base)
    {
      NRBG_CG.DOFade(1, 1f);
      NRBGCircle_CG.DOFade(1, 1f);
      RotateBG();
    }
    else if (bgType == BackgroundType.FreeSpin)
    {
      FreeSpinCircle_CG.DOFade(1, 1f);
      int count = backgrounds[bgType].CircleCG.transform.childCount;
      List<int> purpleCandidates = new List<int> { 0, 9, 10, 11 };
      
      // Select one random element from the purple candidates
      int purpleIndex = purpleCandidates[UnityEngine.Random.Range(0, purpleCandidates.Count)];
      backgrounds[bgType].CircleCG.transform.GetChild(purpleIndex).name = "PURPLE";

      // Create a list of remaining indices, excluding the chosen purpleIndex
      // List<int> remainingIndices = Enumerable.Range(0, count).Where(i => i != purpleIndex).ToList();
      List<int> remainingIndices = new();
      for (int i = 0; i < count; i++)
      {
        if (!purpleCandidates.Contains(i))
        {
          remainingIndices.Add(i);
        }
      }

      // Select one random element from the remaining indices for blue
      int blueIndex = remainingIndices[UnityEngine.Random.Range(0, remainingIndices.Count)];
      backgrounds[bgType].CircleCG.transform.GetChild(blueIndex).name = "BLUE";


      // Set all other elements to "EMPTY"
      for (int i = 0; i < count; i++)
      {
        if (i != purpleIndex && i != blueIndex)
        {
          backgrounds[bgType].CircleCG.transform.GetChild(i).name = "EMPTY";
        }
      }
    }
    else
    {
      var (cg, circleCg) = backgrounds[bgType];
      if (values != null || type == "JOKER1" || type == "JOKER2" || type == "JOKER3") PopulateWheel(circleCg.transform, values, type);
      // anim.StartAnimation();
      RotateFastBG(cg.transform.GetChild(0).GetComponent<Image>(), bgType.ToString());
      cg.DOFade(1, 1f);
      circleCg.DOFade(1, 1f);
    }

    DOVirtual.DelayedCall(1f, () =>
    {
      if (temp != BackgroundType.Base)
      {
        int childCount = backgrounds[temp].CircleCG.transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
          Image image = backgrounds[temp].CircleCG.transform.GetChild(i).GetComponent<Image>();
          image.name = "Image(" + i + ")";
          image.sprite = Empty_Sprite;
          image.DOFade(1, 0);
        }
      }
    });

  }

  internal void FadeOutChildren()
  {
    int childCount = backgrounds[currentBG].CircleCG.transform.childCount;
    for (int i = 0; i < childCount; i++)
    {
      Image image = backgrounds[currentBG].CircleCG.transform.GetChild(i).GetComponent<Image>();
      image.DOFade(0, 0.4f);
    }
  }

  private void PopulateWheel(Transform CircleTransform, List<int> values, string type)
  {
    int childCount = CircleTransform.childCount;
    List<int> availableIndices = new List<int>();

    // Create a list of all available indices on the wheel
    for (int i = 0; i < childCount; i++) availableIndices.Add(i);

    System.Random random = new System.Random();

    Sprite[] sprites = null;
    Sprite jokerSprite = null;
    string jokerColor = "";

    if (type == "MULTIPLIER")
    {
      sprites = MultiplierSprites;
    }
    else if (type == "LEVEL")
    {
      sprites = LevelSprites;
    }
    else if (type.StartsWith("JOKER"))
    {
      int jokerCount = 0;
      if (type == "JOKER1")
      {
        jokerCount = 5;
        jokerSprite = BlueJoker_Sprite;
        jokerColor = "Blue";
      }
      else if (type == "JOKER2")
      {
        jokerCount = 4;
        jokerSprite = GreenJoker_Sprite;
        jokerColor = "Green";
      }
      else if (type == "JOKER3")
      {
        jokerCount = 3;
        jokerSprite = RedJoker_Sprite;
        jokerColor = "Red";
      }

      // Select a random starting index
      int startIndex = random.Next(childCount);
      bool useOddIndices = startIndex % 2 != 0;

      // Collect all odd or even indices based on startIndex
      List<int> possibleIndices = new List<int>();
      for (int i = useOddIndices ? 1 : 0; i < childCount; i += 2)
      {
        possibleIndices.Add(i);
      }

      // Randomly place the specified number of Jokers
      for (int i = 0; i < jokerCount && possibleIndices.Count > 0; i++)
      {
        int randomIndex = possibleIndices[random.Next(possibleIndices.Count)];
        possibleIndices.Remove(randomIndex);

        // Set the Joker at the selected index
        Image childImage = CircleTransform.GetChild(randomIndex).GetComponent<Image>();
        if (childImage != null)
        {
          childImage.rectTransform.sizeDelta = new Vector3(350f, 311f);
          childImage.sprite = jokerSprite;
          childImage.name = jokerColor; // Set the Joker's name to its color
        }

        // Remove this index from available indices to avoid reassigning
        availableIndices.Remove(randomIndex);
      }

      // Set remaining non-Joker indices to "Empty" with an empty sprite
      foreach (int index in availableIndices)
      {
        Image childImage = CircleTransform.GetChild(index).GetComponent<Image>();
        if (childImage.gameObject.name != jokerColor)
        {
          childImage.sprite = Empty_Sprite; // Set to empty sprite
          childImage.name = "Empty";       // Name as "Empty"
        }
      }

      return;  // Exit after placing Joker sprites
    }

    // Place each value at a random position on the wheel
    foreach (int value in values)
    {
      int randomIndex = availableIndices[random.Next(availableIndices.Count)];
      availableIndices.Remove(randomIndex);

      Sprite targetSprite = Array.Find(sprites, sprite => sprite.name == value.ToString());
      if (targetSprite != null)
      {
        Image childImage = CircleTransform.GetChild(randomIndex).GetComponent<Image>();
        if (childImage != null)
        {
          childImage.rectTransform.sizeDelta = new Vector3(110f, 110f);
          childImage.sprite = targetSprite;
          childImage.name = targetSprite.name;
        }
      }
      else
      {
        Debug.LogWarning($"Sprite for value {value} not found in sprites array.");
      }
    }

    // Fill remaining indices with random MultiplierSprites
    foreach (int index in availableIndices)
    {
      Sprite randomSprite = sprites[random.Next(sprites.Length)];
      Image childImage = CircleTransform.GetChild(index).GetComponent<Image>();
      if (childImage != null)
      {
        childImage.rectTransform.sizeDelta = new Vector3(110f, 110f);
        childImage.sprite = randomSprite;
        childImage.name = randomSprite.name;
      }
    }
  }

  private void RotateBG()
  {
    float z = NR_BG_Image.transform.eulerAngles.z;
    z -= 360;
    NR_RotateTween = NR_BG_Image.transform.DORotate(new Vector3(0, 0, z), NRTweenDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear)
    .SetLoops(-1, LoopType.Incremental);
  }

  private void RotateFastBG(Image image, string s)
  {
    float z = image.transform.eulerAngles.z;
    z -= 360;
    Tween tween = image.transform.DORotate(new Vector3(0, 0, z), FRTweenDuration, RotateMode.FastBeyond360).SetEase(Ease.Linear)
    .SetLoops(-1, LoopType.Incremental);

    // Initialize both children scales
    image.transform.GetChild(0).localScale = new Vector3(0.1f, 0.1f, 0.1f);
    image.transform.GetChild(1).localScale = new Vector3(0.1f, 0.1f, 0.1f);

    // Start scaling the first child image
    Tween scaleTween1 = image.transform.GetChild(0).DOScale(1.8f, 1f)
        .SetEase(Ease.Linear)
        .SetLoops(-1, LoopType.Restart);

    // Start scaling the second child image independently
    Tween scaleTween2 = image.transform.GetChild(1).DOScale(1.8f, 1f)
        .SetEase(Ease.Linear)
        .SetDelay(0.5f)
        .SetLoops(-1, LoopType.Restart);

    // Assign tweens to fields for control if needed
    GlowRotation1 = scaleTween1;
    GlowRotation2 = scaleTween2;

    switch (s)
    {
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


  internal void RotateWheel()
  {
    Transform Wheel_Transform = backgrounds[currentBG].CircleCG.transform;
    float z = Wheel_Transform.eulerAngles.z;
    z -= 360;
    Wheel_Transform.DORotate(new Vector3(0, 0, z), 1f, RotateMode.FastBeyond360).OnComplete(() =>
    {
      wheelRoutine = Wheel_Transform.DORotate(new Vector3(0, 0, Wheel_Transform.eulerAngles.z - 360), .5f, RotateMode.FastBeyond360)
      .SetEase(Ease.Linear)
      .SetLoops(-1, LoopType.Incremental);
    })
    .SetEase(Ease.InBack, 2.5f);
    audioController.PlaySpinButtonAudio();
  }

  internal void StopWheel()
  {
    audioController.StopWLAaudio();
    float currentZRotation = backgrounds[currentBG].CircleCG.transform.eulerAngles.z;     // Get the current Z rotation of the wheel

    wheelRoutine.Kill();
    backgrounds[currentBG].CircleCG.transform.DORotate(new Vector3(0, 0, currentZRotation - 360), 0.8f, RotateMode.FastBeyond360)
      .SetEase(Ease.OutCubic);


    // // Calculate the nearest multiple of 30
    // float targetZRotation = Mathf.Round(currentZRotation / 30) * 30;

    // // Rotate the wheel to the nearest 30-degree value over a short duration
    // backgrounds[currentBG].CircleCG.transform
    // .DORotate(new Vector3(0, 0, targetZRotation), .4f)
    // .SetEase(Ease.OutBack, 2f); // Adjust ease for a smooth stop animation

  }

  private void StopRotation(string s)
  {
    // Debug.Log("Stopping " + s + " rotation");
    switch (s)
    {
      case "Base":
        NR_RotateTween?.Kill();
        NRBGCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;

      case "BlueFR":
        wheelRoutine.Kill();
        BlueFR_RotateTween?.Kill();
        BlueFRCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;

      case "GreenFR":
        wheelRoutine.Kill();
        GreenFR_RotateTween?.Kill();
        GreenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;

      case "GoldenFR":
        wheelRoutine.Kill();
        GoldenFR_RotateTween?.Kill();
        GoldenFRCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;

      case "OrangeFR":
        wheelRoutine.Kill();
        OrangeFR_RotateTween?.Kill();
        OrangeFRCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;

      case "PurpleFR":
        wheelRoutine.Kill();
        PurpleFR_RotateTween?.Kill();
        PurpleFRCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;
      case "FreeSpin":
        wheelRoutine.Kill();
        NR_RotateTween?.Kill();
        FreeSpinCircle_CG.transform.localEulerAngles = Vector3.zero;
        break;
    }
  }
}


