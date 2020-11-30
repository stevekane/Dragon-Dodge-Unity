using System;
using System.Collections.Generic;
using UnityEngine;

public enum Element { Earth, Fire, Air, Water }

public enum CardinalRotation { North, East, South, West }

[Serializable]
public struct LayerElement<T> {
  public Vector2Int Cell;
  public T Element;

  public LayerElement(Vector2Int cell, T element) {
    Cell = cell;
    Element = element;
  }
}

[Serializable]
public struct Tile<T>  {
  public CardinalRotation CardinalRotation;
  public T North;
  public T East;
  public T South;
  public T West;
}

[Serializable]
public struct PlayablePosition {}

[Serializable]
public struct Dragon {}

[Serializable]
public struct Wizard {
  public int TeamIndex;
}

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
      var cell = positionMarkers[i].transform.position.FromWorldPosition();
      var position = default(PlayablePosition);

      PlayablePositions.Add(new LayerElement<PlayablePosition>(cell, position));
    }
  }

  public void PlaceTiles(List<GameObject> positionMarkers, Tile<Element>[] tiles) {
    for (int i = 0; i < tiles.Length; i++) {
      var cell = positionMarkers[i].transform.position.FromWorldPosition();
      var tile = tiles[i];

      Tiles.Add(new LayerElement<Tile<Element>>(cell, tile));
    }
  }

  public void PlaceDragons(List<GameObject> positionMarkers) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      var cell = positionMarkers[i].transform.position.FromWorldPosition();
      var dragon = default(Dragon);

      Dragons.Add(new LayerElement<Dragon>(cell, dragon));
    }
  }

  public void PlaceWizards(List<GameObject> positionMarkers) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      var team = positionMarkers[i].GetComponent<TeamAuthoring>();
      var cell = positionMarkers[i].transform.position.FromWorldPosition();
      var wizard = new Wizard { TeamIndex = team.Index };

      Wizards.Add(new LayerElement<Wizard>(cell, wizard));
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