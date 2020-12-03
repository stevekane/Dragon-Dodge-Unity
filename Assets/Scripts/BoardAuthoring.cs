using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public enum Element { Earth, Fire, Air, Water }

public enum CardinalRotation { North, East, South, West }

[Serializable]
public struct LayerElement<T, R> {
  public Vector2Int Cell;
  public T Element;
  public R Renderable;

  public LayerElement(Vector2Int cell, T element, R renderable) {
    Cell = cell;
    Element = element;
    Renderable = renderable;
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
  public List<LayerElement<PlayablePosition, RenderablePlayablePosition>> PlayablePositions;
  public List<LayerElement<Tile<Element>, RenderableTile>> Tiles;
  public List<LayerElement<Dragon, RenderableDragon>> Dragons;
  public List<LayerElement<Wizard, RenderableWizard>> Wizards;

  public void PopulatePlayablePositions(List<GameObject> positionMarkers, RenderablePlayablePosition prefab) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      var transform = positionMarkers[i].transform;
      var cell = transform.position.FromWorldPosition();
      var position = default(PlayablePosition);
      var renderable = RenderablePlayablePosition.Instantiate(prefab, transform.position, transform.rotation);

      renderable.gameObject.SetActive(false);
      PlayablePositions.Add(new LayerElement<PlayablePosition, RenderablePlayablePosition>(cell, position, renderable));
    }
  }

  public void PlaceTiles(List<GameObject> positionMarkers, Tile<Element>[] tiles, RenderableTile prefab) {
    for (int i = 0; i < tiles.Length; i++) {
      var transform = positionMarkers[i].transform;
      var initialPosition = transform.position - Vector3.up * Random.Range(0, 5f);
      var cell = transform.position.FromWorldPosition();
      var tile = tiles[i];
      var renderable = RenderableTile.Instantiate(prefab, initialPosition, transform.rotation);

      Tiles.Add(new LayerElement<Tile<Element>, RenderableTile>(cell, tile, renderable));
    }
  }

  public void PlaceDragons(List<GameObject> positionMarkers, RenderableDragon prefab) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      var transform = positionMarkers[i].transform;
      var initialPosition = transform.position + Vector3.up * 10f;
      var cell = transform.position.FromWorldPosition();
      var dragon = default(Dragon);
      var renderable = RenderableDragon.Instantiate(prefab, initialPosition, transform.rotation);

      Dragons.Add(new LayerElement<Dragon, RenderableDragon>(cell, dragon, renderable));
    }
  }

  public void PlaceWizards(List<GameObject> positionMarkers, RenderableWizard prefab) {
    for (int i = 0; i < positionMarkers.Count; i++) {
      var transform = positionMarkers[i].transform;
      var initialPosition = transform.position + Vector3.up * 10f;
      var team = positionMarkers[i].GetComponent<TeamAuthoring>();
      var cell = transform.position.FromWorldPosition();
      var wizard = new Wizard { TeamIndex = team.Index };
      var renderable = RenderableWizard.Instantiate(prefab, initialPosition, transform.rotation);

      Wizards.Add(new LayerElement<Wizard, RenderableWizard>(cell, wizard, renderable));
    }
  }

  public Board(BoardAuthoring board, TileSet tileSet, BoardRenderables boardRenderables) {
    PlayablePositions = new List<LayerElement<PlayablePosition, RenderablePlayablePosition>>(board.PlayablePositions.Count);
    Tiles = new List<LayerElement<Tile<Element>, RenderableTile>>(board.Tiles.Count);
    Dragons = new List<LayerElement<Dragon, RenderableDragon>>(board.Dragons.Count);
    Wizards = new List<LayerElement<Wizard, RenderableWizard>>(board.Wizards.Count);

    PopulatePlayablePositions(board.PlayablePositions, boardRenderables.PlayablePositionPrefab);
    PlaceTiles(board.Tiles, tileSet.Tiles, boardRenderables.TilePrefab);
    PlaceDragons(board.Dragons, boardRenderables.DragonPrefab);
    PlaceWizards(board.Wizards, boardRenderables.WizardPrefab);
  }
}