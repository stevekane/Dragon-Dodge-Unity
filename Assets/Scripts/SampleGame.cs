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

public static class Extensions {
  public static Vector2Int FromWorldPosition(this Vector3 v) {
    return new Vector2Int(Mathf.RoundToInt(v.x),  Mathf.RoundToInt(v.z));
  }

  public static Vector3 ToWorldPosition(this Vector2Int v) { 
    return new Vector3(v.x, 0, v.y);
  }
}

public class SampleGame : MonoBehaviour {
  public enum State { 
    Base, 
    RotateTile, 
    MoveTile, 
    PlaceTile, 
    MoveDragon, 
    MoveWizard,
    TileToMoveSelected, 
    DragonToMoveSelected,
    WizardToMoveSelected
  }

  [Header("Visualization")]
  public Color[] TileElementColors;

  [Header("Camera")]
  public Camera MainCamera;

  [Header("Authoring")]
  public BoardAuthoring AuthoringBoard;
  public TileSet TileSet;

  [Header("Runtime")]
  public Board Board;

  [Header("State")]
  public List<Action> Actions = new List<Action>();
  public State CurrentState;
  public int SelectedTileIndex = -1;
  public int SelectedPieceIndex = -1;

  public static bool TryGetIndexForCell<T>(List<LayerElement<T>> layer, in Vector2Int cell, out int index) {
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell == cell) {
        index = i;
        return true;
      }
    }
    index = -1;
    return false;
  }

  public static bool HasMemberForCell<T>(List<LayerElement<T>> layer, in Vector2Int cell) {
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell == cell) {
        return true;
      }
    }
    return false;
  }

  public static bool NeighborOf(in Vector2Int cell, in Vector2Int candidate) {
    var north = (candidate.x == cell.x && candidate.y == cell.y + 1);
    var south = (candidate.x == cell.x && candidate.y == cell.y - 1);
    var east = (candidate.y == cell.y && candidate.x == cell.x + 1);
    var west = (candidate.y == cell.y && candidate.x == cell.x - 1);
    
    return north || south || east || west;
  }

  public static int FindEmptyNeighborCells(in Board board, in Vector2Int cell, ref List<Vector2Int> indices) {
    var count = 0;
    for (var i = 0; i < board.PlayablePositions.Count; i++) {
      var candidateCell = board.PlayablePositions[i].Cell;

      if (NeighborOf(cell, candidateCell) && !HasMemberForCell(board.Tiles, candidateCell)) {
        count++;
        indices.Add(candidateCell);
      }
    }
    return count;
  }

  public static Tile<Element> GenerateTile() {
    var tile =  new Tile<Element>();

    tile.North = (Element)UnityEngine.Random.Range(0, 4);
    tile.East = (Element)UnityEngine.Random.Range(0, 4);
    tile.South = (Element)UnityEngine.Random.Range(0, 4);
    tile.West = (Element)UnityEngine.Random.Range(0, 4);
    return tile;
  }

  public static void BeginRotateTile(SampleGame game) {
    game.CurrentState = State.RotateTile;
  }

  public static void BeginMoveTile(SampleGame game) {
    game.CurrentState = State.MoveTile;
  }

  public static void BeginPlaceTile(SampleGame game) {
    game.CurrentState = State.PlaceTile;
  }

  public static void BeginMoveDragon(SampleGame game) {
    game.CurrentState = State.MoveDragon;
  }

  public static void BeginMoveWizard(SampleGame game) {
    game.CurrentState = State.MoveWizard;
  }

  public static void RotateTile(SampleGame game, int tileIndex) {
    var tile = game.Board.Tiles[tileIndex];

    tile.Element.CardinalRotation = (CardinalRotation)(((int)tile.Element.CardinalRotation + 1) % 4);
    game.Board.Tiles[tileIndex] = tile;
    game.CurrentState = State.Base;
  }

  public static void PlaceTile(SampleGame game, int positionIndex) {
    var tileElement = new LayerElement<Tile<Element>> { 
      Cell = game.Board.PlayablePositions[positionIndex].Cell,
      Element = GenerateTile()
    };

    game.Board.Tiles.Add(tileElement);
    game.CurrentState = State.Base;
  }

  public static void SelectTile(SampleGame game, int tileIndex) {
    game.SelectedTileIndex = tileIndex;
    game.CurrentState = State.TileToMoveSelected;
  }

  public static void SelectDragon(SampleGame game, int dragonIndex) {
    game.SelectedPieceIndex = dragonIndex;
    game.CurrentState = State.DragonToMoveSelected;
  }

  public static void SelectWizard(SampleGame game, int wizardIndex) {
    game.SelectedPieceIndex = wizardIndex;
    game.CurrentState = State.WizardToMoveSelected;
  }

  public static void SetCell<T>(List<LayerElement<T>> layer, in int index, in Vector2Int cell) {
    var layerElement = layer[index];

    layerElement.Cell = cell;
    layer[index] = layerElement;
  }

  public static void MoveTile(SampleGame game, Vector2Int cell) {
    var affectedTile = game.Board.Tiles[game.SelectedTileIndex];

    if (TryGetIndexForCell(game.Board.Dragons, affectedTile.Cell, out int dragonIndex)) {
      SetCell(game.Board.Dragons, dragonIndex, cell);
    }

    if (TryGetIndexForCell(game.Board.Wizards, affectedTile.Cell, out int wizardIndex)) {
      SetCell(game.Board.Wizards, wizardIndex, cell);
    }

    SetCell(game.Board.Tiles, game.SelectedTileIndex, cell);
    game.SelectedTileIndex = -1;
    game.CurrentState = State.Base;
  }

  public static void MoveDragon(SampleGame game, Vector2Int cell) {
    SetCell(game.Board.Dragons, game.SelectedPieceIndex, cell);
    game.SelectedPieceIndex = -1;
    game.CurrentState = State.Base;
  }

  public static void MoveWizard(SampleGame game, Vector2Int cell) {
    SetCell(game.Board.Wizards, game.SelectedPieceIndex, cell);
    game.SelectedPieceIndex = -1;
    game.CurrentState = State.Base;
  }

  public static void ConvertInputToActions(SampleGame game) {
    var mouseDown = Input.GetMouseButtonDown(0);
    var pickStruckBoard = Physics.Raycast(game.MainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit);

    switch (game.CurrentState) {
      case State.Base: {
        if (Input.GetKeyDown(KeyCode.R)) {
          game.Actions.Add(new Action(Operation.BeginRotateTile));
        } else if (Input.GetKeyDown(KeyCode.M)) {
          game.Actions.Add(new Action(Operation.BeginMoveTile));
        } else if (Input.GetKeyDown(KeyCode.P)) {
          game.Actions.Add(new Action(Operation.BeginPlaceTile));
        } else if (Input.GetKeyDown(KeyCode.W)) {
          game.Actions.Add(new Action(Operation.BeginMoveWizard));
        } else if (Input.GetKeyDown(KeyCode.D)) {
          game.Actions.Add(new Action(Operation.BeginMoveDragon));
        }
      }
      break;

      case State.RotateTile: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.Tiles, hit.point.FromWorldPosition(), out int tileIndex)) {
            game.Actions.Add(new Action(Operation.RotateTile, tileIndex));
          }
        }
      }
      break;

      case State.PlaceTile: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.PlayablePositions, hit.point.FromWorldPosition(), out int positionIndex)) {
            if (!HasMemberForCell(game.Board.Tiles, hit.point.FromWorldPosition())) {
              game.Actions.Add(new Action(Operation.PlaceTile, positionIndex));
            }
          }
        }
      }
      break;

      case State.MoveTile: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.Tiles, hit.point.FromWorldPosition(), out int index)) {
            game.Actions.Add(new Action(Operation.SelectTile, index));
          }
        }
      }
      break;

      case State.TileToMoveSelected: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.PlayablePositions, hit.point.FromWorldPosition(), out int positionIndex)) {
            var selectedCell = game.Board.Tiles[game.SelectedTileIndex].Cell;
            var candidateCell = game.Board.PlayablePositions[positionIndex].Cell;
            
            if (!HasMemberForCell(game.Board.Tiles, candidateCell) && NeighborOf(selectedCell, candidateCell)) {
              game.Actions.Add(new Action(Operation.MoveTile, candidateCell));
            }
          }
        }
      }
      break;

      case State.DragonToMoveSelected: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.Tiles, hit.point.FromWorldPosition(), out int tileIndex)) {
            var selectedCell = game.Board.Dragons[game.SelectedPieceIndex].Cell;
            var candidateCell = game.Board.Tiles[tileIndex].Cell;

            if (NeighborOf(selectedCell, candidateCell) && !HasMemberForCell(game.Board.Dragons, candidateCell)) {
              game.Actions.Add(new Action(Operation.MoveDragon, candidateCell));
            }
          }
        }
      }
      break;

      case State.WizardToMoveSelected: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.Tiles, hit.point.FromWorldPosition(), out int tileIndex)) {
            var selectedCell = game.Board.Wizards[game.SelectedPieceIndex].Cell;
            var candidateCell = game.Board.Tiles[tileIndex].Cell;

            if (NeighborOf(selectedCell, candidateCell) && !HasMemberForCell(game.Board.Wizards, candidateCell)) {
              game.Actions.Add(new Action(Operation.MoveWizard, candidateCell));
            }
          }
        }
      }
      break;

      case State.MoveDragon: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.Dragons, hit.point.FromWorldPosition(), out int index)) {
            game.Actions.Add(new Action(Operation.SelectDragon, index));
          }
        }
      }
      break;

      case State.MoveWizard: {
        if (mouseDown && pickStruckBoard) {
          if (TryGetIndexForCell(game.Board.Wizards, hit.point.FromWorldPosition(), out int index)) {
            game.Actions.Add(new Action(Operation.SelectWizard, index));
          }
        }
      }
      break;
    }
  }

  public static void ProcessAction(SampleGame game, in Action action) {
    switch (action.Operation) {
      case Operation.BeginRotateTile: 
        BeginRotateTile(game);
      break;

      case Operation.BeginMoveTile: 
        BeginMoveTile(game);
      break;

      case Operation.BeginPlaceTile: 
        BeginPlaceTile(game);
      break;

      case Operation.BeginMoveWizard: 
        BeginMoveWizard(game);
      break;

      case Operation.BeginMoveDragon: 
        BeginMoveDragon(game);
      break;

      case Operation.SelectTile:
        SelectTile(game, action.Index);
      break;

      case Operation.SelectDragon:
        SelectDragon(game, action.Index);
      break;

      case Operation.SelectWizard:
        SelectWizard(game, action.Index);
      break;

      case Operation.RotateTile: 
        RotateTile(game, action.Index);
      break;

      case Operation.PlaceTile:
        PlaceTile(game, action.Index);
      break;

      case Operation.MoveTile:
        MoveTile(game, action.Cell);
      break;

      case Operation.MoveDragon:
        MoveDragon(game, action.Cell);
      break;

      case Operation.MoveWizard:
        MoveWizard(game, action.Cell);
      break;

      default:
        Debug.LogError($"Un-handled Operation: {action.Operation}.");
      break;
    }
  }

  public static void LogAction(Action action) {
    Debug.Log($"{action.Operation} on frame {Time.frameCount}");
  }

  void OnDrawGizmos() {
    if (!Application.isPlaying)
        return;
    
    Gizmos.color = Color.black;
    foreach (var e in Board.PlayablePositions) {
        Gizmos.DrawWireCube(e.Cell.ToWorldPosition(), new Vector3(.9f, 0, .9f));
    }

    Gizmos.color = Color.white;
    foreach (var e in Board.Tiles) {
      var index = (int)e.Element.CardinalRotation;
      var center = e.Cell.ToWorldPosition() + .1f * Vector3.up;

      Gizmos.color = TileElementColors[(int)e.Element.North];
      Gizmos.DrawLine(center, center + Quaternion.AngleAxis(90f * index, Vector3.up) * (.4f * Vector3.forward));
      Gizmos.color = TileElementColors[(int)e.Element.East];
      Gizmos.DrawLine(center, center + Quaternion.AngleAxis(90f * index, Vector3.up) * (.4f * Vector3.right));
      Gizmos.color = TileElementColors[(int)e.Element.South];
      Gizmos.DrawLine(center, center + Quaternion.AngleAxis(90f * index, Vector3.up) * (.4f * -Vector3.forward));
      Gizmos.color = TileElementColors[(int)e.Element.West];
      Gizmos.DrawLine(center, center + Quaternion.AngleAxis(90f * index, Vector3.up) * (.4f * -Vector3.right));
    }

    Gizmos.color = Color.red;
    foreach (var e in Board.Dragons) {
      Gizmos.DrawWireCube(e.Cell.ToWorldPosition() + .5f * Vector3.up, new Vector3(.5f, 1f, .5f));
    }

    foreach (var e in Board.Wizards) {
      Gizmos.color = e.Element.TeamIndex % 2 == 0 ? Color.green : Color.blue;
      Gizmos.DrawWireCube(e.Cell.ToWorldPosition() + .25f * Vector3.up, new Vector3(.25f, .5f, .25f));
    }

    if (SelectedTileIndex >= 0) {
      Gizmos.color = Color.yellow;
      Gizmos.DrawWireCube(Board.Tiles[SelectedTileIndex].Cell.ToWorldPosition() + .2f * Vector3.up, new Vector3(.9f, 0, .9f));

      var indices = new List<Vector2Int>();
      var count = FindEmptyNeighborCells(Board, Board.Tiles[SelectedTileIndex].Cell, ref indices);

      Gizmos.color = Color.magenta;
      foreach (var c in indices) {
        Gizmos.DrawWireCube(c.ToWorldPosition() + .2f * Vector3.up, new Vector3(.9f, 0, .9f));
      }
    }
  }

  void Awake() {
    Board = new Board(AuthoringBoard, TileSet);
    Destroy(AuthoringBoard.gameObject);
  }

  void Update() {
    Actions.Clear();
    ConvertInputToActions(this);
    for (int i = 0; i < Actions.Count; i++) {
      ProcessAction(this, Actions[i]);
      // ProcessActionForRenderer(this, Actions[i]);
      LogAction(Actions[i]);
    }
  }
}