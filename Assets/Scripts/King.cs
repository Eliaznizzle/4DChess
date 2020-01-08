using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Chess;

public class King : ChessPiece
{
    public override int[][] GetMoves() {
        List<int[]> moves = new List<int[]>();

        for (int x = -1; x < 2; x++) {
            for (int y = -1; y < 2; y++) {
                for (int z = -1; z < 2; z++) {
                    for (int w = -1; w < 2; w++) {
                        int[] move = CalcMove(new int[] { x, y, z, w });
                        if (Stuff.WithinBounds(move) && (board.Index(move) == null || board.Index(move).black != black)) {
                            moves.Add(move);
                        }
                    }
                }
            }
        }

        return moves.ToArray();
    }

    public override void Die() {
        base.Die();
        board.manager.EndGame(black != board.localPlayerBlack);
    }
}
