using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;

public class Bishop : ChessPiece
{
    public override int[][] GetMoves() {
        List<int[]> moves = new List<int[]>();

        for (int d = 0; d < 4; d++) {
            for (int d2 = d + 1; d2 < 4; d2++) {
                if (d != d2) {
                    for (int i = -1; i < 2; i += 2) {
                        for (int i2 = -1; i2 < 2; i2 += 2) {
                            int[] move = new int[4];
                            while (true) {
                                move[d] += i;
                                move[d2] += i2;
                                int[] move2 = CalcMove(move);
                                if (Stuff.WithinBounds(move2) && (board.Index(move2) == null || board.Index(move2).black != black)) {
                                    moves.Add(move2);
                                } else {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        return moves.ToArray();
    }
}
