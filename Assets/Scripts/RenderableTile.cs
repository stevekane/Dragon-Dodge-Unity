using UnityEngine;

public class RenderableTile : MonoBehaviour {
  public Animator Animator;

  public Material EarthMaterial;
  public Material FireMaterial;
  public Material AirMaterial;
  public Material WaterMaterial;

  public MeshRenderer NorthWedgeMeshRenderer;
  public MeshRenderer EastWedgeMeshRenderer;
  public MeshRenderer SouthWedgeMeshRenderer;
  public MeshRenderer WestWedgeMeshRenderer;

  Material MaterialForElement(in Element element) {
    switch (element) {
      case Element.Earth: return EarthMaterial;
      case Element.Fire:  return FireMaterial;
      case Element.Air:   return AirMaterial;
      case Element.Water: return WaterMaterial;
      default:            return default(Material);
    }
  }

  public void SetElements(in Tile<Element> tile) {
    NorthWedgeMeshRenderer.material = MaterialForElement(tile.North);
    EastWedgeMeshRenderer.material  = MaterialForElement(tile.East);
    SouthWedgeMeshRenderer.material = MaterialForElement(tile.South);
    WestWedgeMeshRenderer.material  = MaterialForElement(tile.West);
  }
}