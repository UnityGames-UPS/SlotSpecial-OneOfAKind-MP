using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json;
using Best.SocketIO;
using Best.SocketIO.Events;

public class SocketIOManager : MonoBehaviour
{
  [SerializeField] private GameObject RaycastBlocker;
  [Header("Controllers")]
  [SerializeField] private SlotBehaviour slotManager;
  [SerializeField] private UIManager uiManager;
  [Header("Test Token")]
  [SerializeField] private string testToken;
  internal GameData initialData = null;
  internal Features initFeatures = null;
  internal UiData initUIData = null;
  internal Root resultData = null;
  internal Player playerdata = null;
  internal bool isResultdone = false;
  [SerializeField] internal JSFunctCalls JSManager;
  protected string nameSpace = "playground";
  private Socket gameSocket;
  private SocketManager manager;
  protected string SocketURI = null;
  protected string TestSocketURI = "http://localhost:5000/";
  // protected string TestSocketURI = "https://game-crm-rtp-backend.onrender.com/";
  internal bool isLoaded = false;
  internal bool SetInit = false;
  private const int maxReconnectionAttempts = 6;
  private readonly TimeSpan reconnectionDelay = TimeSpan.FromSeconds(10);
  private bool hasEverConnected = false;
  private const int MaxReconnectAttempts = 5;
  private const float ReconnectDelaySeconds = 2f;
  private bool isConnected = false; //Back2 Start
  private float lastPongTime = 0f;
  private float pingInterval = 2f;
  private float pongTimeout = 3f;
  private bool waitingForPong = false;
  private int missedPongs = 0;
  private const int MaxMissedPongs = 5;
  private Coroutine PingRoutine; //Back2 end
  private void Awake()
  {
    SetInit = false;
  }

  private void Start()
  {
    OpenSocket();
  }

  void ReceiveAuthToken(string jsonData)
  {
    Debug.Log("Received data: " + jsonData);
    // Parse the JSON data
    var data = JsonUtility.FromJson<AuthTokenData>(jsonData);
    // Proceed with connecting to the server using myAuth and socketURL
    SocketURI = data.socketURL;
    myAuth = data.cookie;
    nameSpace = data.nameSpace;
  }

  string myAuth = null;

  private void OpenSocket()
  {
    //Create and setup SocketOptions
    SocketOptions options = new SocketOptions(); //Back2 Start
    options.AutoConnect = false;
    options.Reconnection = false;
    options.Timeout = TimeSpan.FromSeconds(3); //Back2 end
    options.ConnectWith = Best.SocketIO.Transports.TransportTypes.WebSocket;

#if UNITY_WEBGL && !UNITY_EDITOR
        JSManager.SendCustomMessage("authToken");
        StartCoroutine(WaitForAuthToken(options));
#else
    object authFunction(SocketManager manager, Socket socket)
    {
      return new
      {
        token = testToken
      };
    }
    options.Auth = authFunction;
    SetupSocketManager(options);
#endif
  }


  private IEnumerator WaitForAuthToken(SocketOptions options)
  {
    // Wait until myAuth is not null
    while (myAuth == null)
    {
      Debug.Log("My Auth is null");
      yield return null;
    }
    while (SocketURI == null)
    {
      Debug.Log("My Socket is null");
      yield return null;
    }
    Debug.Log("My Auth is not null");

    // Once myAuth is set, configure the authFunction
    Func<SocketManager, Socket, object> authFunction = (manager, socket) =>
    {
      return new
      {
        token = myAuth
      };
    };
    options.Auth = authFunction;

    Debug.Log("Auth function configured with token: " + myAuth);

    // Proceed with connecting to the server
    SetupSocketManager(options);
  }

  private void SetupSocketManager(SocketOptions options)
  {
    // Create and setup SocketManager
#if UNITY_EDITOR
    this.manager = new SocketManager(new Uri(TestSocketURI), options);
#else
        this.manager = new SocketManager(new Uri(SocketURI), options);
#endif
    if (string.IsNullOrEmpty(nameSpace) | string.IsNullOrWhiteSpace(nameSpace))
    {
      gameSocket = this.manager.Socket;
    }
    else
    {
      Debug.Log("Namespace used :" + nameSpace);
      gameSocket = this.manager.GetSocket("/" + nameSpace);
    }
    // Set subscriptions
    gameSocket.On<ConnectResponse>(SocketIOEventTypes.Connect, OnConnected);
    gameSocket.On(SocketIOEventTypes.Disconnect, OnDisconnected); //Back2 Start
    gameSocket.On<Error>(SocketIOEventTypes.Error, OnError);
    gameSocket.On<string>("game:init", OnListenEvent);
    gameSocket.On<string>("result", OnListenEvent);
    gameSocket.On<string>("pong", OnPongReceived); //Back2 Start

    manager.Open(); //Back2 Start
  }

  void OnConnected(ConnectResponse resp) //Back2 Start
  {
    Debug.Log("✅ Connected to server.");

    if (hasEverConnected)
    {
      uiManager.CheckAndClosePopups();
    }

    isConnected = true;
    hasEverConnected = true;
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
    SendPing();
  } //Back2 end

  private void OnDisconnected() //Back2 Start
  {
    Debug.LogWarning("⚠️ Disconnected from server.");
    isConnected = false;
    ResetPingRoutine();
    uiManager.DisconnectionPopup();
  }

  private void OnPongReceived(string data) //Back2 Start
  {
    // Debug.Log("✅ Received pong from server.");
    waitingForPong = false;
    missedPongs = 0;
    lastPongTime = Time.time;
    // Debug.Log($"⏱️ Updated last pong time: {lastPongTime}");
    // Debug.Log($"📦 Pong payload: {data}");
  } //Back2 end

  private void OnError(Error err)
  {
    Debug.LogError("Socket Error Message: " + err);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("error");
#endif
  }

  private void OnListenEvent(string data)
  {
    ParseResponse(data);
  }

  private void SendPing() //Back2 Start
  {
    ResetPingRoutine();
    PingRoutine = StartCoroutine(PingCheck());
  }

  void ResetPingRoutine()
  {
    if (PingRoutine != null)
    {
      StopCoroutine(PingRoutine);
    }
    PingRoutine = null;
  }

  private IEnumerator PingCheck()
  {
    while (true)
    {
      // Debug.Log($"🟡 PingCheck | waitingForPong: {waitingForPong}, missedPongs: {missedPongs}, timeSinceLastPong: {Time.time - lastPongTime}");

      if (missedPongs == 0)
      {
        uiManager.CheckAndClosePopups();
      }

      // If waiting for pong, and timeout passed
      if (waitingForPong)
      {
        if (missedPongs == 2)
        {
          uiManager.ReconnectionPopup();
        }
        missedPongs++;
        Debug.LogWarning($"⚠️ Pong missed #{missedPongs}/{MaxMissedPongs}");

        if (missedPongs >= MaxMissedPongs)
        {
          Debug.LogError("❌ Unable to connect to server — 5 consecutive pongs missed.");
          isConnected = false;
          uiManager.DisconnectionPopup();
          yield break;
        }
      }

      // Send next ping
      waitingForPong = true;
      lastPongTime = Time.time;
      // Debug.Log("📤 Sending ping...");
      SendDataWithNamespace("ping");
      yield return new WaitForSeconds(pingInterval);
    }
  } //Back2 end

  private void SendDataWithNamespace(string eventName, string json = null)
  {
    // Send the message
    if (gameSocket != null && gameSocket.IsOpen)
    {
      if (json != null)
      {
        gameSocket.Emit(eventName, json);
        Debug.Log("JSON data sent: " + json);
      }
      else
      {
        gameSocket.Emit(eventName);
      }
    }
    else
    {
      Debug.LogWarning("Socket is not connected.");
    }
  }
  void CloseGame()
  {
    Debug.Log("Unity: Closing Game");
    StartCoroutine(CloseSocket());
  }
  internal IEnumerator CloseSocket() //Back2 Start
  {
    RaycastBlocker.SetActive(true);
    ResetPingRoutine();

    Debug.Log("Closing Socket");

    manager?.Close();
    manager = null;

    Debug.Log("Waiting for socket to close");

    yield return new WaitForSeconds(0.5f);

    Debug.Log("Socket Closed");

#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnExit"); //Telling the react platform user wants to quit and go back to homepage
#endif
  } //Back2 end

  private void ParseResponse(string jsonObject)
  {
    Debug.Log(jsonObject);
    Root myData = JsonConvert.DeserializeObject<Root>(jsonObject);

    string id = myData.id;
    playerdata = myData.player;

    switch (id)
    {
      case "initData":
        {
          initialData = myData.gameData;
          initUIData = myData.uiData;
          initFeatures = myData.features;
          if (!SetInit)
          {
            PopulateSlotSocket();
            SetInit = true;
          }
          else
          {
            RefreshUI();
          }
          break;
        }
      case "ResultData":
        {
          resultData = myData;
          isResultdone = true;
          break;
        }
    }
  }

  private void RefreshUI()
  {
    uiManager.InitialiseUIData(initUIData.paylines);
  }

  private void PopulateSlotSocket()
  {
    slotManager.ShuffleSlot();

    slotManager.SetInitialUI();

    isLoaded = true;
    RaycastBlocker.SetActive(false);
#if UNITY_WEBGL && !UNITY_EDITOR
    JSManager.SendCustomMessage("OnEnter");
#endif
  }

  internal void AccumulateResult(int currBet)
  {
    isResultdone = false;
    MessageData message = new();
    message.type = "SPIN";
    message.payload.betIndex = currBet;

    // Serialize message data to JSON
    string json = JsonUtility.ToJson(message);
    SendDataWithNamespace("request", json);
  }
}

[Serializable]
public class MessageData
{
  public string type;
  public Data payload = new();
}

[Serializable]
public class Data
{
  public int betIndex;
  public string Event;
  public List<int> index;
  public int option;
}

[Serializable]
public class AuthTokenData
{
  public string cookie;
  public string socketURL;
  public string nameSpace = "";
}

[Serializable]
public class BoosterResponse
{
  public string type { get; set; }
  public List<int> multipliers { get; set; }
}

[Serializable]
public class Booster
{
  public bool isEnabledSimple { get; set; }
  public bool isEnabledExhaustive { get; set; }
  public string type { get; set; }
  public List<int> typeProbs { get; set; }
  public List<int> multiplier { get; set; }
  public List<double> multiplierProbs { get; set; }
}

[Serializable]
public class Features
{
  public ScatterPurple scatterPurple { get; set; }
  public ScatterBlue scatterBlue { get; set; }
  public Booster booster { get; set; }
  public LevelUp levelUp { get; set; }
  public Joker joker { get; set; }
}

[Serializable]
public class GameData
{
  public List<double> bets { get; set; }
}

[Serializable]
public class Joker
{
  public bool enabled { get; set; }
  public List<double> payout { get; set; }
  public List<int> blueRound { get; set; }
  public List<int> greenRound { get; set; }
  public List<int> redRound { get; set; }
}
[Serializable]
public class JokerResponse
{
  public bool isTriggered { get; set; }
  public List<double> payout { get; set; }
  public int blueRound { get; set; }
  public int greenRound { get; set; }
  public int redRound { get; set; }
}
[Serializable]
public class LevelUp
{
  public bool enabled { get; set; }
  public List<int> level { get; set; }
  public List<double> levelProbs { get; set; }
}

[SerializeField]
public class LevelUpResponse
{
  public bool isLevelUp { get; set; }
  public int level { get; set; }
}

[Serializable]
public class Paylines
{
  public List<Symbol> symbols { get; set; }
}

[Serializable]
public class Player
{
  public double balance { get; set; }
}

[Serializable]
public class Root
{
  public string id { get; set; }
  public GameData gameData { get; set; }
  public Features features { get; set; }
  public UiData uiData { get; set; }
  public Player player { get; set; }

  public bool success { get; set; }
  public List<List<string>> matrix { get; set; }
  public Payload payload { get; set; }
}

[Serializable]
public class Payload
{
  public double totalWin { get; set; }
  public double totalPayout { get; set; }
  public string freeSpinType { get; set; }
  public BoosterResponse boosterResponse { get; set; }
  public LevelUpResponse levelUpResponse { get; set; }
  public JokerResponse jokerResponse { get; set; }
  public ScatterBlueResponse scatterBlueResponse { get; set; }
  public ScatterPurpleResponse scatterPurpleResponse { get; set; }
  public int overallFreeSpins { get; set; }
}

[Serializable]
public class ScatterPurpleResponse
{
  public bool isTriggered { get; set; }
  public List<List<int>> topSymbols { get; set; }
  public List<int> symbols { get; set; }
  public double payout { get; set; }
  public List<LevelUpResponse> levelUp { get; set; }
  public List<BoosterResponse> booster { get; set; }
  public List<int> reTriggered { get; set; }
  public List<int> count { get; set; }
}

[Serializable]
public class ScatterBlueResponse
{
  public bool isTriggered { get; set; }
  public List<int> symbols { get; set; }
  public double payout { get; set; }
  public List<LevelUpResponse> levelUp { get; set; }
  public List<BoosterResponse> booster { get; set; }
  public List<int> count { get; set; }
}

[Serializable]
public class ScatterBlue
{
  public bool enabled { get; set; }
  public List<double> symbolsProbs { get; set; }
  public List<int> featureProbs { get; set; }
}

[Serializable]
public class ScatterPurple
{
  public bool enabled { get; set; }
  public List<int> topSymbolProbs { get; set; }
  public List<double> symbolsProbs { get; set; }
  public List<int> featureProbs { get; set; }
}

[Serializable]
public class Symbol
{
  public int id { get; set; }
  public string name { get; set; }
  public bool isSpecial { get; set; }
  public double payout { get; set; }
  public int freeSpinCount { get; set; }
  public string description { get; set; }
}

[Serializable]
public class UiData
{
  public Paylines paylines { get; set; }
}

public class FreeSpinData
{
  public string type;
  public bool isTriggered { get; set; }
  public List<int> symbols { get; set; }
  public double payout { get; set; }
  public List<LevelUpResponse> levelUp { get; set; }
  public List<int> reTriggered { get; set; }
  public List<BoosterResponse> booster { get; set; }
  public List<int> count { get; set; }
  public List<List<int>> topSymbols { get; set; }
}
