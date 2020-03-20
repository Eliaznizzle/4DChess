using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;
using System.Net.Sockets;
using System.Threading.Tasks;

public class Board : MonoBehaviour {
    private const float TILE_SIZE = 1f;
    private const float PLANE_OFFSET = 1f; //Changing from 1f does as of now not work

    public Manager manager;

    public bool gameStarted = false;
    public bool localPlayerBlack;
    public bool playerTurn;
    bool gameEnabled = true;

    public GameObject whiteKing;
    public GameObject whiteQueen;
    public GameObject whiteBishop;
    public GameObject whiteKnight;
    public GameObject whiteRook;
    public GameObject whitePawn;
    public GameObject blackKing;
    public GameObject blackQueen;
    public GameObject blackBishop;
    public GameObject blackKnight;
    public GameObject blackRook;
    public GameObject blackPawn;

    public AudioClip moveSound;
    public AudioClip promoteSound;

    public static GameObject[] piecePrefabs;

    List<GameObject> validMoveMarkers;
    GameObject validMoveMarkerPrefab;

    public GameObject tile;
    public Color darkTileColor;

    public GameObject moveIndicator;
    List<GameObject> moveIndicators = new List<GameObject>();

    private int[] hover = new int[4] { -1, -1, -1, -1 };
    public int[] grab = new int[4] { -1, -1, -1, -1 };

    public ChessPiece[,,,] pieces = new ChessPiece[4, 4, 4, 4];

    int up = 1;

    private void Start() {
        GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("volume");

        if (localPlayerBlack) { up = -1; playerTurn = false; } else { playerTurn = true; }

        piecePrefabs = new GameObject[] { whiteKing, whiteQueen, whiteBishop, whiteKnight, whiteRook, whitePawn, blackKing, blackQueen, blackBishop, blackKnight, blackRook, blackPawn };
        GenerateChessboard();
        SpawnPieces();

        //Index(new int[] { 3, 3, 3, 3 }).Die();
    }

    private void Update() {
        UpdateHover();
        //Stuff.LogArray(selection);

        if (Input.GetMouseButtonDown(0) && playerTurn && gameEnabled) {
            if (Stuff.WithinBounds(hover) && Index(hover) != null) {
                grab = hover;
                ChessPiece piece = Index(hover);
                if (piece.black == localPlayerBlack) {
                    piece.grab = true;
                    piece.rend.sortingOrder = 6;
                    SpawnIndicators(piece.GetMoves());
                } else {
                    grab = new int[] { -1, -1, -1, -1 };
                }
            }
        }

        if (Input.GetMouseButtonUp(0)) {
            if (grab[0] != -1) {
                ChessPiece piece = Index(grab);
                piece.grab = false;
                Debug.Log("got here");
                if (Stuff.WithinBounds(hover)) {
                    playerTurn = false;
                    if (piece.Move(hover) && !manager.singlePlayerTest) {
                        piece.GoHome();
                        if (piece.GetType() == typeof(Pawn) && piece.position[1] == piece.FinalRank && piece.position[3] == piece.FinalRank) {
                            Promotion promotion = Instantiate(manager.promotionMenuPrefab, manager.worldCanvas).GetComponent<Promotion>();
                            promotion.manager = manager;
                            promotion.piece = piece;
                            promotion.originalPosition = grab;
                        } else {
                            manager.PassTurn(grab, hover);
                        }
                    } else {
                        piece.GoHome();
                        if (piece.GetType() == typeof(Pawn) && piece.position[1] == piece.FinalRank && piece.position[3] == piece.FinalRank) {
                            Debug.Log("Promotion time");
                            Promotion promotion = Instantiate(manager.promotionMenuPrefab, manager.worldCanvas).GetComponent<Promotion>();
                            promotion.manager = manager;
                            promotion.piece = piece;
                        } else {
                            playerTurn = true;
                        }
                    }
                } else { piece.GoHome(); }
                grab = new int[] { -1, -1, -1, -1 };
                DestroyIndicators();
            }
        }
    }

    private void UpdateHover() {
        hover = WorldToBoard(Camera.main.ScreenToWorldPoint(Input.mousePosition));
    }

    private void GenerateChessboard() {
        for (int x = 0; x < 4; x++) {
            for (int y = 0; y < 4; y++) {
                for (int z = 0; z < 4; z++) {
                    for (int w = 0; w < 4; w++) {
                        GameObject obj = Instantiate(tile, BoardToWorld(new int[4] { x, y, z, w }), Quaternion.identity, transform);
                        if ((x + y + z + w) % 2 == 0) {
                            obj.GetComponent<SpriteRenderer>().color = darkTileColor;
                        }
                    }
                }
            }
        }
    }

    private void SpawnPieces() {

        SpawnPiece(1, 0, 2, 0, 0, false);
        SpawnPiece(2, 0, 1, 0, 0, false);

        SpawnPiece(1, 3, 2, 3, 0, true);
        SpawnPiece(2, 3, 1, 3, 0, true);


        SpawnPiece(1, 0, 1, 0, 1, false);
        SpawnPiece(2, 0, 2, 0, 1, false);

        SpawnPiece(1, 3, 1, 3, 1, true);
        SpawnPiece(2, 3, 2, 3, 1, true);


        SpawnPiece(0, 0, 1, 0, 2, false);
        SpawnPiece(0, 0, 2, 0, 2, false);

        SpawnPiece(3, 0, 1, 0, 2, false);
        SpawnPiece(3, 0, 2, 0, 2, false);

        SpawnPiece(0, 3, 1, 3, 2, true);
        SpawnPiece(0, 3, 2, 3, 2, true);

        SpawnPiece(3, 3, 1, 3, 2, true);
        SpawnPiece(3, 3, 2, 3, 2, true);


        SpawnPiece(1, 0, 0, 0, 3, false);
        SpawnPiece(1, 0, 3, 0, 3, false);

        SpawnPiece(2, 0, 0, 0, 3, false);
        SpawnPiece(2, 0, 3, 0, 3, false);

        SpawnPiece(1, 3, 0, 3, 3, true);
        SpawnPiece(1, 3, 3, 3, 3, true);

        SpawnPiece(2, 3, 0, 3, 3, true);
        SpawnPiece(2, 3, 3, 3, 3, true);


        SpawnPiece(0, 0, 0, 0, 4, false);
        SpawnPiece(0, 0, 3, 0, 4, false);

        SpawnPiece(3, 0, 0, 0, 4, false);
        SpawnPiece(3, 0, 3, 0, 4, false);

        SpawnPiece(0, 3, 0, 3, 4, true);
        SpawnPiece(0, 3, 3, 3, 4, true);

        SpawnPiece(3, 3, 0, 3, 4, true);
        SpawnPiece(3, 3, 3, 3, 4, true);


        for (int x = 0; x < 4; x++) {
            for (int z = 0; z < 4; z++) {
                SpawnPiece(x, 1, z, 0, 5, false);
                SpawnPiece(x, 0, z, 1, 5, false);
                SpawnPiece(x, 1, z, 1, 5, false);

                SpawnPiece(x, 2, z, 3, 5, true);
                SpawnPiece(x, 3, z, 2, 5, true);
                SpawnPiece(x, 2, z, 2, 5, true);
            }
        }
    }

    public ChessPiece SpawnPiece(int x, int y, int z, int w, int typeIndex, bool black) {
        ChessPiece piece;
        int[] pos = new int[] { x, y, z, w };

        piece = Index(pos);
        if (piece != null) {
            Destroy(piece.gameObject);
        }

        if (!black) {
            piece = Instantiate(piecePrefabs[typeIndex]).GetComponent<ChessPiece>();
        } else {
            piece = Instantiate(piecePrefabs[typeIndex + 6]).GetComponent<ChessPiece>();
        }
        piece.board = this;
        pieces[x, y, z, w] = piece;
        piece.position = pos;
        return piece;
    }

    public int[] WorldToBoard(Vector2 world) {
        int[] board = new int[4] { (int)(world.x*up + 1.5 * PLANE_OFFSET + 8 - transform.position.x) / 5,
            (int)(world.y*up + 1.5 * PLANE_OFFSET + 8 - transform.position.y) / 5,
            0,
            0
        };
        return Stuff.AddPos(board, new int[4]{ 0,
             0,
             (int)(world.x*up + 1.5 * PLANE_OFFSET + 8 - transform.position.x - (PLANE_OFFSET + TILE_SIZE * 4) * board[0]),
             (int)(world.y*up + 1.5 * PLANE_OFFSET + 8 - transform.position.y - (PLANE_OFFSET + TILE_SIZE * 4) * board[1])});
    }

    public Vector2 BoardToWorld(int[] board) {
        return new Vector2(
                (board[0] * (4f + PLANE_OFFSET) + board[2] - 9f) * up,
                (board[1] * (4f + PLANE_OFFSET) + board[3] - 9f) * up
            );
    }

    public ChessPiece Index(int[] index) {
        print(string.Join(".", index));
        return pieces[index[0], index[1], index[2], index[3]];
    }

    public void SpawnIndicators(int[][] moves) {
        foreach (int[] move in moves) {
            moveIndicators.Add(Instantiate(moveIndicator, BoardToWorld(move), Quaternion.identity));
        }
    }

    public void DestroyIndicators() {
        foreach (GameObject indicator in moveIndicators) {
            Destroy(indicator);
        }
    }

}
