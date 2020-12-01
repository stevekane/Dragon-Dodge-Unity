using UnityEngine;

public enum Operation {
  PassTime,
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
  public float Time;

  public Action(Operation operation, Vector2Int cell, int index, float time) {
    Operation = operation;
    Cell = cell;
    Index = index;
    Time = time;
  }

  public Action(Operation operation) : this(operation, default, default, default) {}
  public Action(Operation operation, Vector2Int cell) : this(operation, cell, default, default) {}
  public Action(Operation operation, int index) : this(operation, default, index, default) {}
  public Action(Operation operation, float time) : this(operation, default, default, time) {}
}