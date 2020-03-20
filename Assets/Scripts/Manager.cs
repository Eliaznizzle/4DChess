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
using System.Linq;

public class Manager : MonoBehaviour {
    public RectTransform screenCanvas;
    public RectTransform worldCanvas;

    public GameObject mainMenu;
    public GameObject gameUI;
    public GameObject popupMenu;
    public GameObject settingsMenu;
    public GameObject matchmakingWindow;

    public InputField nameInput;

    public GameObject gameEndMenuPrefab;
    public Text result;

    public GameObject promotionMenuPrefab;

    public GameObject DebugTextPrefab;
    List<GameObject> DebugLines = new List<GameObject>();

    public System.Random random;

    string url = "http://4dchess.club/server";
    int netID;
    string opName;

    bool matchmake;
    Task startMatchmaking;
    Task cancelMatchmaking;
    List<string[]> netData = new List<string[]>();


    public GameObject boardPrefab;
    public Board board;

    public SettingsController settings;

    public bool singlePlayerTest; //REMOVE THIS. IN FINAL BUILD THIS IS TO ALWAYS BE CONSIDERED FALSE
    void Start() {
        Print("Debug log initiated.");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-us");
        Initialize();
        UpdateName();
        if (singlePlayerTest) {
            StartGame(true);
        }

    }

    void Initialize() {
        settings.UpdateSettings();

        DontDestroyOnLoad(gameObject);

        mainMenu.SetActive(true);

        random = new System.Random();
    }

    public void Play() {
        if (!matchmake) {
            //If there is a matchmaking task that has yet not been killed, wait for it. It should die. If it does not, program will crash and I deserve that for messing up
            if (cancelMatchmaking != null) { cancelMatchmaking.Wait(); }
            string localPlayerName = PlayerPrefs.GetString("name");
            if (localPlayerName == "") { localPlayerName = "Guest"; }
            ToggleMatchmakingWindow(true);
            startMatchmaking = Task.Run(() => {
                netData.Add(new string[] { "matchmake" }.Concat(WebMatchmake(localPlayerName).Split('.')).ToArray());
            });

        }
    }

    private void Update() {
        if (Input.GetButtonDown("Cancel")) {
            WebCancelMatchmaking();
            Application.Quit();
        }

        if (netData.Count > 0) {
            HandleData(netData[0]);
        }
    }

    void StartGame(bool isHost) {
        Print("StartGame was called");
        mainMenu.SetActive(false);
        ToggleMatchmakingWindow(false);
        board = Instantiate(boardPrefab).GetComponent<Board>();
        gameUI.transform.Find("Playing against").GetComponent<Text>().text = "Playing against " + opName;
        gameUI.SetActive(true);
        if (!singlePlayerTest) {
            //Debug.Log("Connected to " + ((IPEndPoint)otherPlayer.RemoteEndPoint).Address);
            if (isHost) {
                board.localPlayerBlack = random.NextDouble() >= 0.5;
                if (!singlePlayerTest) {
                    WebSend((!board.localPlayerBlack).ToString());
                    Print("Get here mebe?");
                    /*
                    otherPlayer.Send(new byte[] { Convert.ToByte(!board.localPlayerBlack) });
                    */
                }
            } else {
                if (!singlePlayerTest) {
                    Print("Is player black?");
                    int i = 0;
                    while (true) {
                        i++;
                        Thread.Sleep(2000);
                        string isBlackString = WebReceive();
                        Print(isBlackString);
                        try {
                            board.localPlayerBlack = bool.Parse(isBlackString);
                            break;
                        } catch (Exception e) {
                            Print("Error! " + e.Message);
                        }
                        if (i > 10) {
                            mainMenu.SetActive(true);
                            Destroy(board);
                            Destroy(gameUI);
                            return;
                        }
                    }

                    /*byte[] data = new byte[4];
                    otherPlayer.Receive(data);
                    board.localPlayerBlack = Convert.ToBoolean(data[0]);*/
                }
            }

            Task.Run(() => {
                while (this) {
                    Thread.Sleep(2000);
                    netData.Add(WebReceive().Split('.'));
                }
            });
        } else { board.localPlayerBlack = false; }
        Debug.Log("Setting board manager");
        board.manager = this;
        board.gameStarted = true;
    }

    public void MainMenu() {
        SceneManager.LoadScene(0);
        Destroy(gameObject);
    }

    public void PassTurn(int[] piece, int[] move) {
        string data = $"move.{piece[0]}{piece[1]}{piece[2]}{piece[3]}.{move[0]}{move[1]}{move[2]}{move[3]}";
        Debug.Log($"Sending move {piece[0]}{piece[1]}{piece[2]}{piece[3]} to {move[0]}{move[1]}{move[2]}{move[3]}");
        WebSend(data);
        /*otherPlayer.Send(data);*/
    }

    public void PassTurnPromotion(int[] piece, int[] move, int index) {
        string data = $"promote.{piece[0]}{piece[1]}{piece[2]}{piece[3]}.{move[0]}{move[1]}{move[2]}{move[3]}.{index}";
        Debug.Log($"Sending move {piece[0]}{piece[1]}{piece[2]}{piece[3]} to {move[0]}{move[1]}{move[2]}{move[3]} with promotion to {index}");
        WebSend(data);
        /*otherPlayer.Send(data);*/
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

    public void HandleData(string[] data) {
        netData.Remove(data);
        Print(string.Join(".", data));
        if (data[0] == "") { return; }

        if (data[0] == "move") {
            int[] piecePos = Array.ConvertAll(data[1].ToCharArray(), c => (int)Char.GetNumericValue(c));
            int[] move = Array.ConvertAll(data[2].ToCharArray(), c => (int)Char.GetNumericValue(c));
            board.Index(piecePos).Move(move);
            board.playerTurn = true;
            board.Index(move).GoHome();
        } else if (data[0] == "promote") {
            int[] piecePos = Array.ConvertAll(data[1].ToCharArray(), c => (int)Char.GetNumericValue(c));
            int[] move = Array.ConvertAll(data[2].ToCharArray(), c => (int)Char.GetNumericValue(c));
            Print("assigned arrays");
            ChessPiece piece = board.Index(piecePos);
            Print("Got piece");
            piece.Move(move);
            Print("Moved");
            board.SpawnPiece(move[0], move[1], move[2], move[3], int.Parse(data[3]), piece.black);
            board.playerTurn = true;
            Print("Got here too");
            board.Index(move).GoHome();
            Print("How about here?");
        } else if (data[0] == "name") {
            if (data[0] != "") {
                matchmake = false;
                opName = data[0];
                StartGame(true);
            }
        } else if (data[0] == "matchmake") {
            if (data.Length == 2) {
                //Waiting for match
                netID = int.Parse(data[1]);
                Debug.Log(netID);
                Task.Run(() => {
                    while (matchmake && this) {
                        Thread.Sleep(2000);
                        data = WebReceive().Split('.');
                        netData.Add(data);
                    }
                    WebCancelMatchmaking();
                });
            } else if (data.Length == 3) {
                //Found match
                netID = int.Parse(data[1]);
                opName = data[2];
                WebSend("name." + PlayerPrefs.GetString("name"));
                Debug.Log(opName);
                StartGame(false);
            } else {
                throw new Exception("Matchmaking data (" + string.Join(".", data) + ")had an invalid length");
            }
        } else {
            Print("wtf? data length is " + data.Length);
        }
    }

    /*
    public (byte[], int) ReceiveData() {
        byte[] data = new byte[1024];
        int length = otherPlayer.Receive(data);
        Debug.Log("Received data");
        return (data, length);
    }*/

    string WebMatchmake(string localPlayerName) {
        matchmake = true;
        return new WebClient().DownloadString(url + "?intent=matchmake&name=" + localPlayerName);
    }

    string WebCancelMatchmaking() {
        matchmake = false;
        return new WebClient().DownloadString(url + "?intent=cancel&id=" + netID);
    }

    string WebSend(string data) {
        Debug.Log("Sending " + data);
        return new WebClient().DownloadString(url + "?intent=send&id=" + netID + "&data=" + data);
    }

    string WebReceive() {
        return new WebClient().DownloadString(url + "?intent=receive&id=" + netID);
    }

    public void UpdateName() {
        nameInput.text = PlayerPrefs.GetString("name");
    }

    public void ChangeName() {
        PlayerPrefs.SetString("name", nameInput.text);
    }

    public void ToggleMenu(bool active) {
        popupMenu.SetActive(active);
    }

    public void ToggleSettings(bool active) {
        settingsMenu.SetActive(active);
    }

    public void ToggleMatchmakingWindow(bool active) {
        matchmakingWindow.SetActive(active);
    }

    public void CancelMatchmaking() {

        cancelMatchmaking = Task.Run(() => {
            WebCancelMatchmaking();
        });
        matchmake = false;
        ToggleMatchmakingWindow(false);
    }

    public void Print(string text) {

        Debug.Log(text);
        foreach (GameObject line in DebugLines) {
            line.GetComponent<RectTransform>().localPosition += new Vector3(0, -15, 0);
        }

        GameObject newLine = Instantiate(DebugTextPrefab, screenCanvas);
        newLine.GetComponent<Text>().text = Time.time + ": " + text;
        DebugLines.Add(newLine);
    }

}
