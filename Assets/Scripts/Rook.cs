using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;

public class Rook : ChessPiece {
    public override int[][] GetMoves() {
        List<int[]> moves = new List<int[]>();

        for (int d = 0; d < 4; d++) {
            for (int i = -1; i < 2; i += 2) {
                int[] move = new int[4];
                while (true) {
                    move[d] += i;
                    int[] move2 = CalcMove(move);
                    if (Stuff.WithinBounds(move2)) {
                        if (board.Index(move2) == null) {
                            moves.Add(move2);
                        } else if (board.Index(move2).black != black) {
                            moves.Add(move2);
                            break;
                        } else {
                            break;
                        }
                    } else {
                        break;
                    }
                }
            }
        }

        return moves.ToArray();
    }
}
