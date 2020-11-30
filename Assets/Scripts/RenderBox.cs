using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderBox : MonoBehaviour {
    public Vector3 Center;
    public Vector3 Size;
    public Color Color;

    public void OnDrawGizmos() {
        Gizmos.color = Color;
        Gizmos.DrawWireCube(transform.position + Center, Size);
    }
}