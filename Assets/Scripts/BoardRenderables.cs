using UnityEngine;

[CreateAssetMenu(menuName="BoardRenderables")]
public class BoardRenderables : ScriptableObject {
  public RenderablePlayablePosition PlayablePositionPrefab;
  public RenderableTile TilePrefab;
  public RenderableDragon DragonPrefab;
  public RenderableWizard WizardPrefab;
}