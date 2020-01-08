using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;

public class Knight : ChessPiece {
    public override int[][] GetMoves() {
        List<int[]> moves = new List<int[]>();

        for (int d = 0; d < 4; d++) {
            for (int d2 = d + 1; d2 < 4; d2++) {
                if (d != d2) {
                    for (int i = -1; i < 2; i += 2) {
                        for (int i2 = -1; i2 < 2; i2 += 2) {

                            int[][] tryMoves = new int[][] { CalcMove(new int[4]), CalcMove(new int[4]) };

                            for (int m = 0; m < 2; m++) {
                                tryMoves[m][d] += (2-m)*i;
                                tryMoves[m][d2] += (1+m)*i2;
                            }

                            foreach (int[] move in tryMoves) {
                                if (Stuff.WithinBounds(move) && (board.Index(move) == null || board.Index(move).black != black)) {
                                    moves.Add(move);
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
