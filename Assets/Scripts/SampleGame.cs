using System.Collections.Generic;
using UnityEngine;

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

  public static Tile<Element> GenerateTile() {
    var tile =  new Tile<Element>();

    tile.North = (Element)UnityEngine.Random.Range(0, 4);
    tile.East = (Element)UnityEngine.Random.Range(0, 4);
    tile.South = (Element)UnityEngine.Random.Range(0, 4);
    tile.West = (Element)UnityEngine.Random.Range(0, 4);
    return tile;
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
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int tileIndex)) {
            game.Actions.Add(new Action(Operation.RotateTile, tileIndex));
          }
        }
      }
      break;

      case State.PlaceTile: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.PlayablePositions.TryGetIndexForCell(hit.point.FromWorldPosition(), out int positionIndex)) {
            if (!game.Board.Tiles.HasMemberForCell(hit.point.FromWorldPosition())) {
              game.Actions.Add(new Action(Operation.PlaceTile, positionIndex));
            }
          }
        }
      }
      break;

      case State.MoveTile: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int index)) {
            game.Actions.Add(new Action(Operation.SelectTile, index));
          }
        }
      }
      break;

      case State.TileToMoveSelected: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.PlayablePositions.TryGetIndexForCell(hit.point.FromWorldPosition(), out int positionIndex)) {
            var selectedCell = game.Board.Tiles[game.SelectedTileIndex].Cell;
            var candidateCell = game.Board.PlayablePositions[positionIndex].Cell;
            
            if (!game.Board.Tiles.HasMemberForCell(candidateCell) && candidateCell.IsNeighborOf(selectedCell)) {
              game.Actions.Add(new Action(Operation.MoveTile, candidateCell));
            }
          }
        }
      }
      break;

      case State.DragonToMoveSelected: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int tileIndex)) {
            var selectedCell = game.Board.Dragons[game.SelectedPieceIndex].Cell;
            var candidateCell = game.Board.Tiles[tileIndex].Cell;

            if (candidateCell.IsNeighborOf(selectedCell) && !game.Board.Dragons.HasMemberForCell(candidateCell)) {
              game.Actions.Add(new Action(Operation.MoveDragon, candidateCell));
            }
          }
        }
      }
      break;

      case State.WizardToMoveSelected: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int tileIndex)) {
            var selectedCell = game.Board.Wizards[game.SelectedPieceIndex].Cell;
            var candidateCell = game.Board.Tiles[tileIndex].Cell;

            if (candidateCell.IsNeighborOf(selectedCell) && !game.Board.Wizards.HasMemberForCell(candidateCell)) {
              game.Actions.Add(new Action(Operation.MoveWizard, candidateCell));
            }
          }
        }
      }
      break;

      case State.MoveDragon: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.Dragons.TryGetIndexForCell(hit.point.FromWorldPosition(), out int index)) {
            game.Actions.Add(new Action(Operation.SelectDragon, index));
          }
        }
      }
      break;

      case State.MoveWizard: {
        if (mouseDown && pickStruckBoard) {
          if (game.Board.Wizards.TryGetIndexForCell(hit.point.FromWorldPosition(), out int index)) {
            game.Actions.Add(new Action(Operation.SelectWizard, index));
          }
        }
      }
      break;
    }
  }

  public static void ProcessAction(SampleGame game, in Action action) {
    switch (action.Operation) {
      case Operation.BeginRotateTile: {
        game.CurrentState = State.RotateTile;
      }
      break;

      case Operation.BeginMoveTile: {
        game.CurrentState = State.MoveTile;
      }
      break;

      case Operation.BeginPlaceTile: {
        game.CurrentState = State.PlaceTile;
      }
      break;

      case Operation.BeginMoveWizard: {
        game.CurrentState = State.MoveWizard;
      }
      break;

      case Operation.BeginMoveDragon: {
        game.CurrentState = State.MoveDragon;
      }
      break;

      case Operation.SelectTile: {
        game.SelectedTileIndex = action.Index;
        game.CurrentState = State.TileToMoveSelected;
      }
      break;

      case Operation.SelectDragon:
        game.SelectedPieceIndex = action.Index;
        game.CurrentState = State.DragonToMoveSelected;
      break;

      case Operation.SelectWizard: {
        game.SelectedPieceIndex = action.Index;
        game.CurrentState = State.WizardToMoveSelected;
      }
      break;

      case Operation.RotateTile: {
        var tile = game.Board.Tiles[action.Index];
        var rotation = (CardinalRotation)(((int)tile.Element.CardinalRotation + 1) % 4);

        tile.Element.CardinalRotation = rotation;
        game.Board.Tiles[action.Index] = tile;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.PlaceTile: {
        var cell = game.Board.PlayablePositions[action.Index].Cell;
        var tile = GenerateTile();
        var tileElement = new LayerElement<Tile<Element>>(cell, tile);

        game.Board.Tiles.Add(tileElement);
        game.CurrentState = State.Base;
      }
      break;

      case Operation.MoveTile: {
        var affectedTile = game.Board.Tiles[game.SelectedTileIndex];

        if (game.Board.Dragons.TryGetIndexForCell(affectedTile.Cell, out int dragonIndex)) {
          game.Board.Dragons.SetCell(dragonIndex, action.Cell);
        }

        if (game.Board.Wizards.TryGetIndexForCell(affectedTile.Cell, out int wizardIndex)) {
          game.Board.Wizards.SetCell(wizardIndex, action.Cell);
        }

        game.Board.Tiles.SetCell(game.SelectedTileIndex, action.Cell);
        game.SelectedTileIndex = -1;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.MoveDragon: {
        game.Board.Dragons.SetCell(game.SelectedPieceIndex, action.Cell);
        game.SelectedPieceIndex = -1;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.MoveWizard: {
        game.Board.Wizards.SetCell(game.SelectedPieceIndex, action.Cell);
        game.SelectedPieceIndex = -1;
        game.CurrentState = State.Base;
      }
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
      var count = Board.FindEmptyNeighborCells(Board.Tiles[SelectedTileIndex].Cell, ref indices);

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