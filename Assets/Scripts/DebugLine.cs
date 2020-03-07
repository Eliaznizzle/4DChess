using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugLine : MonoBehaviour
{
    public int lifetime = 10;
    float age;
    // Update is called once per frame
    void Update()
    {
        if (age >= lifetime) {
            Destroy(gameObject);
        } else {
            age += Time.deltaTime;
        }
    }
}
