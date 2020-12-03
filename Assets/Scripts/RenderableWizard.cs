using UnityEngine;

public class RenderableWizard : MonoBehaviour {
  public Animator Animator;

  public Material Team1Material;
  public Material Team2Material;

  public MeshRenderer WizardMeshRenderer;

  public void SetTeam(in int teamIndex) {
    WizardMeshRenderer.material = teamIndex % 2 == 0 ? Team1Material : Team2Material;
  }
}