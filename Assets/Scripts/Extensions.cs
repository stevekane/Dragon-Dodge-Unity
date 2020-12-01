using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
  public static Vector2Int FromWorldPosition(this Vector3 v) {
    return new Vector2Int(Mathf.RoundToInt(v.x),  Mathf.RoundToInt(v.z));
  }

  public static Vector3 ToWorldPosition(this Vector2Int v) { 
    return new Vector3(v.x, 0, v.y);
  }

  public static bool TryGetIndexForCell<T, R>(
  this List<LayerElement<T, R>> layer, 
  in Vector2Int cell, 
  out int index) {
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell == cell) {
        index = i;
        return true;
      }
    }
    index = -1;
    return false;
  }

  public static bool HasMemberForCell<T, R>(
  this List<LayerElement<T, R>> layer, 
  in Vector2Int cell) {
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell == cell) {
        return true;
      }
    }
    return false;
  }

  public static bool IsNeighborOf(
  this in Vector2Int candidate, 
  in Vector2Int cell) {
    var north = (candidate.x == cell.x && candidate.y == cell.y + 1);
    var south = (candidate.x == cell.x && candidate.y == cell.y - 1);
    var east = (candidate.y == cell.y && candidate.x == cell.x + 1);
    var west = (candidate.y == cell.y && candidate.x == cell.x - 1);
    
    return north || south || east || west;
  }

  public static bool TileHasMove(
  this int tileIndex,
  in Board board) {
    var tileCell = board.Tiles[tileIndex].Cell;

    for (var i = 0; i < board.PlayablePositions.Count; i++) {
      var candidateCell = board.PlayablePositions[i].Cell;;
      var isNeighbor = candidateCell.IsNeighborOf(tileCell);
      var isEmpty = !board.Tiles.HasMemberForCell(candidateCell);

      if (isNeighbor && isEmpty) {
        return true;
      }
    }
    return false;
  }

  public static void SetCell<T, R>(
  this List<LayerElement<T, R>> layer, 
  in int index, 
  in Vector2Int cell) {
    var layerElement = layer[index];

    layerElement.Cell = cell;
    layer[index] = layerElement;
  }
}