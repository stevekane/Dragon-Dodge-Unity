using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BoardAuthoring : MonoBehaviour {
  public List<GameObject> PlayablePositions;
  public List<GameObject> Tiles;
  public List<GameObject> Dragons;
  public List<GameObject> Wizards;
}

[Serializable]
public struct Board {
  public List<LayerElement<PlayablePosition>> PlayablePositions;
  public List<LayerElement<Tile<Element>>> Tiles;
  public List<LayerElement<Dragon>> Dragons;
  public List<LayerElement<Wizard>> Wizards;

  public void PopulatePlayablePositions(List<GameObject> positionMarkers) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      PlayablePositions.Add(new LayerElement<PlayablePosition>(positionMarkers[i].transform.position.FromWorldPosition(), default(PlayablePosition)));
    }
  }

  public void PlaceTiles(List<GameObject> positionMarkers, Tile<Element>[] tiles) {
    for (int i = 0; i < tiles.Length; i++) {
      Tiles.Add(new LayerElement<Tile<Element>>(positionMarkers[i].transform.position.FromWorldPosition(), tiles[i]));
    }
  }

  public void PlaceDragons(List<GameObject> positionMarkers) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      Dragons.Add(new LayerElement<Dragon>(positionMarkers[i].transform.position.FromWorldPosition(), default(Dragon)));
    }
  }

  public void PlaceWizards(List<GameObject> positionMarkers) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      var team = positionMarkers[i].GetComponent<TeamAuthoring>();

      Wizards.Add(new LayerElement<Wizard>(positionMarkers[i].transform.position.FromWorldPosition(), new Wizard { TeamIndex = team.Index }));
    }
  }

  public Board(BoardAuthoring board, TileSet tileSet) {
    PlayablePositions = new List<LayerElement<PlayablePosition>>(board.PlayablePositions.Count);
    Tiles = new List<LayerElement<Tile<Element>>>(board.Tiles.Count);
    Dragons = new List<LayerElement<Dragon>>(board.Dragons.Count);
    Wizards = new List<LayerElement<Wizard>>(board.Wizards.Count);

    PopulatePlayablePositions(board.PlayablePositions);
    PlaceTiles(board.Tiles, tileSet.Tiles);
    PlaceDragons(board.Dragons);
    PlaceWizards(board.Wizards);
  }
}