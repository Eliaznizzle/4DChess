using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Promotion : MonoBehaviour {
    public Manager manager;
    public ChessPiece piece;
    public int[] promotions;

    int index = 0;

    private void Start() {
        GameObject[] pieces = Board.piecePrefabs;

        GetComponent<RectTransform>().localPosition = (manager.board.BoardToWorld(piece.position) * (manager.canvas.pixelRect.height / Camera.main.orthographicSize) / 2) * 10;
    }

    public void Change() {
        piece = manager.board.SpawnPiece(piece.position[0], piece.position[1], piece.position[2], piece.position[3], index + 1, piece.black);
    }

    public void ChangeRight() {
        if (index < 3) { index++; } else index = 0;
        Change();
    }

    public void ChangeLeft() {
        if (index > 0) { index--; } else index = 3;
        Change();
    }

    public void Confirm() {
        if (!manager.singlePlayerTest) {
            //Pass turn with info about promotion
        } else {
            manager.board.playerTurn = true;
        }
        Destroy(gameObject);
    }
}
