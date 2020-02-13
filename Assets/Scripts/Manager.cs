using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using UnityEngine.UI;
using System;
using System.Globalization;
using System.Threading;
using System.Text;
using UnityEngine.SceneManagement;

public class Manager : MonoBehaviour {
    public GameObject mainMenuPrefab;
    public GameObject mainMenu;
    public InputField iPInput;
    public InputField portInput;

    public RectTransform ScreenUIScaler;
    public RectTransform WorldUIScaler;

    public GameObject gameUIPrefab;
    public GameObject gameUI;

    public GameObject gameEndMenuPrefab;
    public Text result;

    public GameObject promotionMenuPrefab;

    public GameObject DebugTextPrefab;
    List<GameObject> DebugLines = new List<GameObject>();

    public Text myIP;
    public Text error;

    int port;

    public System.Random random;

    Socket otherPlayer;

    public GameObject boardPrefab;
    public Board board;

    List<(byte[], int)> netData = new List<(byte[], int)>();

    Task receive;

    public bool singlePlayerTest; //REMOVE THIS. IN FINAL BUILD THIS IS TO ALWAYS BE CONSIDERED FALSE

    Task host;
    void Start() {
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
        Initialize();
        if (singlePlayerTest) {
            StartGame(true);
        }
    }

    void Initialize() {
        DontDestroyOnLoad(gameObject);

        random = new System.Random();

        mainMenu = Instantiate(mainMenuPrefab);

        foreach (InputField ui in mainMenu.GetComponentsInChildren<InputField>()) {
            switch (ui.gameObject.name) {
                case "IP Input":
                    iPInput = ui;
                    break;
                case "Port Input":
                    portInput = ui;
                    break;
                default:
                    break;
            }
        }

        foreach (Text ui in mainMenu.GetComponentsInChildren<Text>()) {
            switch (ui.gameObject.name) {
                case "My IP":
                    myIP = ui;
                    break;
                case "Error":
                    error = ui;
                    break;
                default:
                    break;
            }
        }

        mainMenu.GetComponentInChildren<Button>().onClick.AddListener(Connect);

        Socket listener = new Socket(SocketType.Stream, ProtocolType.Tcp);
        port = random.Next(2000, 3000);
        host = Task.Run(() => {
            foreach (IPAddress address in Dns.GetHostEntry(Dns.GetHostName()).AddressList) {
                if (address.AddressFamily == AddressFamily.InterNetwork && (address.ToString().Split('.')[0] == "192" || address.ToString().Split('.')[0] == "10")) {
                    myIP.text = "Your ip: " + address + " Your port: " + port;
                    IPEndPoint ep = new IPEndPoint(address, port);
                    listener.Bind(ep);
                    break;
                }
            }
            listener.Listen(1);
            Debug.Log("Listening for connections...");

            otherPlayer = listener.Accept();

            StartGame(true);
        });
    }

    private void Update() {
        if (Input.GetButtonDown("Cancel")) {
            Application.Quit();
        }
        if (netData.Count > 0) {
            Print("We got data boys");
            string dataString = Encoding.ASCII.GetString(netData[0].Item1, 0, netData[0].Item2);
            Print(dataString);
            if (dataString.Length == 8) {
                Print("Data length is 8 - opponent made move.");
                int[] piecePos = new int[4];
                int[] move = new int[4];
                for (int i = 0; i < 4; i++) {
                    piecePos[i] = (int)Char.GetNumericValue(dataString[i]);
                }
                for (int i = 0; i < 4; i++) {
                    move[i] = (int)Char.GetNumericValue(dataString[i + 4]);
                }
                Print("Got here");
                board.Index(piecePos).Move(move);
                Print("Got here?");
                board.playerTurn = true;
                board.Index(move).GoHome();
            } else if (dataString.Length == 9) {
                Print("Data length is 9 - opponent made move and promoted a piece.");
                int[] piecePos = new int[4];
                int[] move = new int[4];
                for (int i = 0; i < 4; i++) {
                    piecePos[i] = (int)Char.GetNumericValue(dataString[i]);
                }
                for (int i = 0; i < 4; i++) {
                    move[i] = (int)Char.GetNumericValue(dataString[i + 4]);
                }
                ChessPiece piece = board.Index(piecePos);
                piece.Move(move);
                Print("Got here");
                board.SpawnPiece(move[0], move[1], move[2], move[3], (int)Char.GetNumericValue(dataString[8]), piece.black);
                board.playerTurn = true;
                Print("Got here too");
                board.Index(move).GoHome();
                Print("How about here?");
            } else {
                Print("wtf? data length is " + dataString.Length);
            }
            netData.RemoveAt(0);
        }
    }

    public void Connect() {


        IPAddress iP;
        int targetPort;
        try {
            iP = IPAddress.Parse(iPInput.textComponent.text);
            targetPort = int.Parse(portInput.text);
        } catch (Exception e) { error.text = e.Message; return; }
        for (int i = 0; i < 1; i++) {
            try {
                otherPlayer = new Socket(SocketType.Stream, ProtocolType.Tcp);
                otherPlayer.Connect(new IPEndPoint(iP, targetPort));
                if (otherPlayer.Connected) {
                    break;
                }
            } catch (Exception e) { error.text = e.Message; }
        }

        if (otherPlayer.Connected) {
            StartGame(false);
        }
    }

    void StartGame(bool isHost) {
        mainMenu.SetActive(false);
        board = Instantiate(boardPrefab).GetComponent<Board>();
        gameUI = Instantiate(gameUIPrefab);
        if (!singlePlayerTest) {
            //Debug.Log("Connected to " + ((IPEndPoint)otherPlayer.RemoteEndPoint).Address);
            if (isHost) {
                board.localPlayerBlack = random.NextDouble() >= 0.5;
                if (!singlePlayerTest) {
                    otherPlayer.Send(new byte[] { Convert.ToByte(!board.localPlayerBlack) });
                }
            } else {
                byte[] data = new byte[4];
                if (!singlePlayerTest) {
                    otherPlayer.Receive(data);
                }
                board.localPlayerBlack = Convert.ToBoolean(data[0]);
            }
        } else { board.localPlayerBlack = false; }
        board.manager = this;
        receive = Task.Run(() => {
            while (this) {
                netData.Add(ReceiveData());
            }
        });
        board.gameStarted = true;
    }

    public void MainMenu() {
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    public void PassTurn(int[] piece, int[] move) {
        byte[] data = Encoding.ASCII.GetBytes($"{piece[0]}{piece[1]}{piece[2]}{piece[3]}{move[0]}{move[1]}{move[2]}{move[3]}");
        Debug.Log($"Sending move {piece[0]}{piece[1]}{piece[2]}{piece[3]} to {move[0]}{move[1]}{move[2]}{move[3]}");
        otherPlayer.Send(data);
    }

    public void PassTurnPromotion(int[] piece, int[] move, int index) {
        byte[] data = Encoding.ASCII.GetBytes($"{piece[0]}{piece[1]}{piece[2]}{piece[3]}{move[0]}{move[1]}{move[2]}{move[3]}{index}");
        Debug.Log($"Sending move {piece[0]}{piece[1]}{piece[2]}{piece[3]} to {move[0]}{move[1]}{move[2]}{move[3]} with promotion to {index}");
        otherPlayer.Send(data);
    }

    public void EndGame(bool win) {
        GameObject gameEndMenu = Instantiate(gameEndMenuPrefab);
        result = gameEndMenu.GetComponentInChildren<Text>();
        Button mainMenuButton = gameEndMenu.GetComponentInChildren<Button>();
        mainMenuButton.onClick.AddListener(MainMenu);
        if (win) {
            result.text = "You won!";
        } else {
            result.text = "You lost...";
        }
    }

    public (byte[], int) ReceiveData() {
        byte[] data = new byte[1024];
        int length = otherPlayer.Receive(data);
        Debug.Log("Received data");
        return (data, length);
    }

    public void Print(string text) {
        Debug.Log(text);
        foreach (GameObject line in DebugLines) {
            line.GetComponent<RectTransform>().localPosition += new Vector3(0, -150, 0);
        }

        GameObject newLine = Instantiate(DebugTextPrefab, ScreenUIScaler);
        newLine.GetComponent<Text>().text = Time.time + ": " + text;
        DebugLines.Add(newLine);
    }

}
