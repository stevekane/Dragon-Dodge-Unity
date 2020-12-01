using UnityEngine;

public enum Operation {
  BeginRotateTile,
  BeginMoveTile,
  BeginPlaceTile,
  BeginMoveWizard,
  BeginMoveDragon,
  SelectTile,
  SelectDragon,
  SelectWizard,
  RotateTile,
  PlaceTile,
  MoveTile,
  MoveDragon,
  MoveWizard,
  MovingTile,
  RotatingTile,
  PlacingTile,
  MovingDragon,
  MovingWizard
}

public struct Action {
  public Operation Operation;
  public Vector2Int Cell;
  public int Index;

  public Action(Operation operation, Vector2Int cell, int index) {
    Operation = operation;
    Cell = cell;
    Index = index;
  }

  public Action(Operation operation) : this(operation, default, default) {}
  public Action(Operation operation, Vector2Int cell) : this(operation, cell, default) {}
  public Action(Operation operation, int index) : this(operation, default, index) {}
}