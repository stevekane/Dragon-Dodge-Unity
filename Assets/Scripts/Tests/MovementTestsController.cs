using System;
using UnityEngine;

public class MovementTestsController : MonoBehaviour {
  public RenderableWizard Wizard;

  static KeyCode[] Keys       = new KeyCode[] { KeyCode.W,        KeyCode.S,       KeyCode.D,     KeyCode.A };
  static Vector3[] Directions = new Vector3[] { Vector3.forward, -Vector3.forward, Vector3.right, -Vector3.right };

  static bool TryGetKeyDown(KeyCode[] keys, out int index) {
    index = Array.FindIndex(keys, 0, keys.Length, Input.GetKeyDown);
    return index >= 0;
  }

  public void Update() {
    if (TryGetKeyDown(Keys, out int index)) {
      Wizard.SetNewPath(Wizard.transform.position + Directions[index]);
    }
    Wizard.Tick(Time.deltaTime);
  }
}