using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;

public class Pawn : ChessPiece
{
    public override int[][] GetMoves() {

        int up;
        if (black) { up = -1; } else { up = 1; }

        List<int[]> moves = new List<int[]>();


        for (int d = 1; d < 5; d+=2) {
            int [] move = CalcMove(new int[4]);
            move[d] += up;
            if (Stuff.WithinBounds(move) && board.Index(move) == null) {
                moves.Add(move);
                if (!hasMoved && Stuff.WithinBounds(move) && board.Index(move) == null) {
                    move = CalcMove(new int[4]);
                    move[d] += 2 * up;
                    moves.Add(move);
                }
            }
        }

        List<int[]> attacks = new List<int[]>();

        for (int d = 1; d < 5; d+=2) {
            for (int d2 = 0; d2 < 4; d2+=2) {
                int[] baseMove = new int[] { 0, 0, 0, 0 };
                baseMove[d] = up;
                for (int k = -1; k < 3; k+=2) {
                    baseMove[d2] = k;
                    int[] move = CalcMove(baseMove);
                    if (Stuff.WithinBounds(move) && board.Index(move) != null && board.Index(move).black != this.black) {
                        moves.Add(move);
                    }
                }
            }
        }

        return moves.ToArray();
    }
}
