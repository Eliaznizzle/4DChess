using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Promotion : MonoBehaviour {
    public Manager manager;
    public ChessPiece piece;
    public int[] promotions;

    public int[] originalPosition;

    int index = 0;

    private void Start() {
        GameObject[] pieces = Board.piecePrefabs;

        /*GetComponent<RectTransform>().localPosition = new Vector2(
            piece.transform.position.x * (manager.canvas.pixelRect.width / (Camera.main.orthographicSize * Camera.main.aspect * 2)),
            piece.transform.position.y * (manager.canvas.pixelRect.height / (Camera.main.orthographicSize * 2))
            ) * 10;*/
        transform.position = piece.transform.position;
        UpdatePiece();
    }

    public void UpdatePiece() {
        piece = manager.board.SpawnPiece(piece.position[0], piece.position[1], piece.position[2], piece.position[3], index + 1, piece.black);
    }

    public void ChangeRight() {
        if (index < 3) { index++; } else index = 0;
        UpdatePiece();
    }

    public void ChangeLeft() {
        if (index > 0) { index--; } else index = 3;
        UpdatePiece();
    }

    public void Confirm() {
        if (!manager.singlePlayerTest) {
            //Pass turn with info about promotion
            manager.PassTurnPromotion(originalPosition, piece.position, index+1);
        } else {
            manager.board.playerTurn = true;
        }
        Destroy(gameObject);
    }
}
