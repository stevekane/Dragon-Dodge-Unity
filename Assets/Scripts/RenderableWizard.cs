using UnityEngine;

public class RenderableWizard : MonoBehaviour {
  public Animator Animator;

  public Material Team1Material;
  public Material Team2Material;

  public SkinnedMeshRenderer MeshRenderer;

  public Vector3 Heading;
  public Vector3 Destination;

  public void SetTeam(in int teamIndex) {
    MeshRenderer.material = teamIndex % 2 == 0 ? Team1Material : Team2Material;
  }
}