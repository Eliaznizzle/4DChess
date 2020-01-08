using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;
public abstract class ChessPiece : MonoBehaviour
{
    public Board board;

    public bool hasMoved = false;
    public int[] position = new int[] { -1, -1, -1, -1 };
    public bool black;

    public bool grab;

    private bool fling = false; //If true, piece should be flinged then this should be set to false

    void Start() {
        if (gameObject.name.Contains("Black")) {
            black = true;
        }
    }

    void Update() {
        if (grab) {
            transform.position = (Vector2)Camera.main.ScreenToWorldPoint(Input.mousePosition);
        }
    }

    private void FixedUpdate() {
        if (fling) {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            int mod = Random.Range(0, 1) * 2 - 1;
            rb.AddForce(new Vector2(Random.Range(250, 500) * mod, board.manager.random.Next(30, 50)));
            rb.AddTorque(-30 * mod, ForceMode2D.Force);
            fling = false;
        }
    }

    public abstract int[][] GetMoves();

    public bool Move(int[] target) {
        if (IsMoveValid(target)) {
            ChessPiece cap = board.pieces[target[0], target[1], target[2], target[3]];
            if (cap != null) { cap.Die(); }
            board.pieces[position[0], position[1], position[2], position[3]] = null;
            position = target;
            board.pieces[target[0], target[1], target[2], target[3]] = this;
            hasMoved = true;
            return true;
        }
        else { return false; }
    }

    public void GoHome() {
        GetComponent<SpriteRenderer>().sortingOrder = 0;
        transform.position = board.BoardToWorld(position);
    }

    public bool IsMoveValid(int[] move) {
        foreach (int[] move2 in GetMoves()) {
            for (int i = 0; i < 4; i++) {
                if (move[i] != move2[i]) {
                    break;
                } else if (i == 3) {
                    return true;
                }
            }
        }
        return false;
    }
    public int[] CalcMove(int[] move) {
        int[] sum = new int[4];
        for (int i = 0; i < 4; i++) {
            sum[i] = position[i] + move[i];
        }
        return sum;
    }

    public virtual void Die() {
        position = new int[] { -1, -1, -1, -1 };
        GetComponent<SpriteRenderer>().sortingOrder = 2;
        gameObject.AddComponent(typeof(Rigidbody2D));
        gameObject.AddComponent(typeof(PolygonCollider2D));
        fling = true;
    }
}
