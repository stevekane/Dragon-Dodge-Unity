using UnityEngine;

[CreateAssetMenu(menuName="TileSet")]
public class TileSet : ScriptableObject {
  public Tile<Element>[] Tiles;
}