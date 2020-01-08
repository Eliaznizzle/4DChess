using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chess {
    public class Stuff {
        public static int[] AddPos(int[] one, int[] two) {
            int[] sum = new int[4];
            for (int i = 0; i < 4; i++) {
                sum[i] = one[i] + two[i];
            }
            return sum;
        }

        public static int[] Copy(int[] array) {
            int[] copy = new int[4];
            for (int i = 0; i < 4; i++) {
                copy[i] = array[i];
            }
            return copy;
        }

        public static void LogArray(int[] array) {
            Debug.Log($"{array[0]}, {array[1]}, {array[2]}, {array[3]}");
        }

        public static bool WithinBounds(int[] pos) {
            for (int i = 0; i < pos.Length; i++) {
                if (pos[i] > 3 || pos[i] < 0) {
                    return false;
                }
            }
            return true;
        }
    }
}
