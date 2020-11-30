using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
  public static Vector2Int FromWorldPosition(this Vector3 v) {
    return new Vector2Int(Mathf.RoundToInt(v.x),  Mathf.RoundToInt(v.z));
  }

  public static Vector3 ToWorldPosition(this Vector2Int v) { 
    return new Vector3(v.x, 0, v.y);
  }

  public static bool TryGetIndexForCell<T>(this List<LayerElement<T>> layer, in Vector2Int cell, out int index) {
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell == cell) {
        index = i;
        return true;
      }
    }
    index = -1;
    return false;
  }

  public static bool HasMemberForCell<T>(this List<LayerElement<T>> layer, in Vector2Int cell) {
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell == cell) {
        return true;
      }
    }
    return false;
  }

  public static bool IsNeighborOf(this in Vector2Int candidate, in Vector2Int cell) {
    var north = (candidate.x == cell.x && candidate.y == cell.y + 1);
    var south = (candidate.x == cell.x && candidate.y == cell.y - 1);
    var east = (candidate.y == cell.y && candidate.x == cell.x + 1);
    var west = (candidate.y == cell.y && candidate.x == cell.x - 1);
    
    return north || south || east || west;
  }

  public static int FindEmptyNeighborCells(this in Board board, in Vector2Int cell, ref List<Vector2Int> indices) {
    var count = 0;
    for (var i = 0; i < board.PlayablePositions.Count; i++) {
      var candidateCell = board.PlayablePositions[i].Cell;

      if (cell.IsNeighborOf(candidateCell) && !HasMemberForCell(board.Tiles, candidateCell)) {
        count++;
        indices.Add(candidateCell);
      }
    }
    return count;
  }

  public static void SetCell<T>(this List<LayerElement<T>> layer, in int index, in Vector2Int cell) {
    var layerElement = layer[index];

    layerElement.Cell = cell;
    layer[index] = layerElement;
  }
}