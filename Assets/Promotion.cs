using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Promotion : MonoBehaviour
{
    public Manager manager;
    public ChessPiece piece;



    private void Start() {
        GetComponent<RectTransform>().localPosition = (manager.board.BoardToWorld(piece.position)*(manager.canvas.pixelRect.height/Camera.main.orthographicSize)/2)*10;
    }

    public void ChangeRight() {

    }
}
