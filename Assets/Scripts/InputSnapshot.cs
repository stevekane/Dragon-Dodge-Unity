using System;
using UnityEngine;

[Serializable]
public struct InputSnapshot {
  public Vector3 MousePosition;
  public bool MouseButtonDown;
  public float DeltaTime;
  public float Time;
  public bool WDown;
  public bool DDown;
  public bool MDown;
  public bool PDown;
  public bool RDown;

  public static InputSnapshot FromGlobalSingletons {
    get {
      return new InputSnapshot {
        MousePosition = Input.mousePosition,
        MouseButtonDown = Input.GetMouseButtonDown(0),
        DeltaTime = UnityEngine.Time.deltaTime,
        Time = UnityEngine.Time.time,
        WDown = Input.GetKeyDown(KeyCode.W),
        DDown = Input.GetKeyDown(KeyCode.D),
        MDown = Input.GetKeyDown(KeyCode.M),
        PDown = Input.GetKeyDown(KeyCode.P),
        RDown = Input.GetKeyDown(KeyCode.R)
      };
    }
  }
}
