﻿using System.Collections.Generic;
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
    WizardToMoveSelected,
    RotatingTile,
    PlacingTile,
    MovingTile,
    MovingDragon,
    MovingWizard
  }

  [Header("Visualization")]
  public Color[] TileElementColors;

  [Header("Rendering")]
  public float InterpolationEpsilon = .001f;

  [Header("Camera")]
  public Camera MainCamera;

  [Header("Authoring")]
  public BoardAuthoring AuthoringBoard;
  public TileSet TileSet;
  public BoardRenderables BoardRenderables;

  [Header("Runtime")]
  public Board Board;
  public PlayerController PlayerController;
  public AIController AIController;

  [Header("State")]
  public List<InputSnapshot> Inputs = new List<InputSnapshot>();
  public List<Action> Actions = new List<Action>();
  public State CurrentState;
  public int SelectedTileIndex = -1;
  public int SelectedPieceIndex = -1;
  public bool IsPlayerTurn = true;

  public static Tile<Element> GenerateTile() {
    var tile =  new Tile<Element>();

    tile.North = (Element)UnityEngine.Random.Range(0, 4);
    tile.East = (Element)UnityEngine.Random.Range(0, 4);
    tile.South = (Element)UnityEngine.Random.Range(0, 4);
    tile.West = (Element)UnityEngine.Random.Range(0, 4);
    return tile;
  }

  public static void ProcessAction(SampleGame game, in Action action) {
    switch (action.Operation) {
      case Operation.PassTime: {

      }
      break;

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

        var selectedTile = game.Board.Tiles[game.SelectedTileIndex];

        selectedTile.Renderable.Animator.SetInteger("State", 2);
        for (var i = 0; i < game.Board.PlayablePositions.Count; i++) {
          var playablePosition = game.Board.PlayablePositions[i];
          var isNeighbor = playablePosition.Cell.IsNeighborOf(selectedTile.Cell);
          var isEmpty = !game.Board.Tiles.HasMemberForCell(playablePosition.Cell);

          playablePosition.Renderable.Animator.SetInteger("State", isNeighbor && isEmpty ? 1 : 0);
        }
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
        game.IsPlayerTurn = !game.IsPlayerTurn;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.PlaceTile: {
        var cell = game.Board.PlayablePositions[action.Index].Cell;
        var tile = GenerateTile();
        var renderable = RenderableTile.Instantiate(game.BoardRenderables.TilePrefab);
        var tileElement = new LayerElement<Tile<Element>, RenderableTile>(cell, tile, renderable);

        game.Board.Tiles.Add(tileElement);
        game.IsPlayerTurn = !game.IsPlayerTurn;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.MoveTile: {
        var affectedTile = game.Board.Tiles[game.SelectedTileIndex];

        affectedTile.Renderable.Animator.SetInteger("State", 0);
        for (var i = 0; i < game.Board.PlayablePositions.Count; i++) {
          game.Board.PlayablePositions[i].Renderable.Animator.SetInteger("State", 0);
        }

        if (game.Board.Dragons.TryGetIndexForCell(affectedTile.Cell, out int dragonIndex)) {
          game.Board.Dragons.SetCell(dragonIndex, action.Cell);
        }

        if (game.Board.Wizards.TryGetIndexForCell(affectedTile.Cell, out int wizardIndex)) {
          game.Board.Wizards.SetCell(wizardIndex, action.Cell);
        }

        game.Board.Tiles.SetCell(game.SelectedTileIndex, action.Cell);
        game.SelectedTileIndex = -1;
        game.IsPlayerTurn = !game.IsPlayerTurn;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.MoveDragon: {
        game.Board.Dragons.SetCell(game.SelectedPieceIndex, action.Cell);
        game.SelectedPieceIndex = -1;
        game.IsPlayerTurn = !game.IsPlayerTurn;
        game.CurrentState = State.Base;
      }
      break;

      case Operation.MoveWizard: {
        game.Board.Wizards.SetCell(game.SelectedPieceIndex, action.Cell);
        game.SelectedPieceIndex = -1;
        game.IsPlayerTurn = !game.IsPlayerTurn;
        game.CurrentState = State.Base;
      }
      break;

      default:
        Debug.LogError($"Un-handled Operation: {action.Operation}.");
      break;
    }
  }

  public static void UpdateRenderables(SampleGame game) {
    foreach (var e in game.Board.PlayablePositions) {
      e.Renderable.transform.position = Vector3.Lerp(e.Renderable.transform.position, e.Cell.ToWorldPosition(), game.InterpolationEpsilon);
    }

    foreach (var e in game.Board.Tiles) {
      var position = Vector3.Lerp(e.Renderable.transform.position, e.Cell.ToWorldPosition(), game.InterpolationEpsilon);
      var rotation = Quaternion.Slerp(e.Renderable.transform.rotation, Quaternion.AngleAxis(90f * (int)e.Element.CardinalRotation, Vector3.up), game.InterpolationEpsilon);

      e.Renderable.transform.SetPositionAndRotation(position, rotation);
    }

    foreach (var e in game.Board.Dragons) {
      e.Renderable.transform.position = Vector3.Lerp(e.Renderable.transform.position, e.Cell.ToWorldPosition(), game.InterpolationEpsilon);
    }

    foreach (var e in game.Board.Wizards) {
      e.Renderable.transform.position = Vector3.Lerp(e.Renderable.transform.position, e.Cell.ToWorldPosition(), game.InterpolationEpsilon);
    }
  }

  public static void LogAction(Action action) {
    Debug.Log($"{action.Operation}");
  }

  void Awake() {
    Board = new Board(AuthoringBoard, TileSet, BoardRenderables);
    PlayerController = new PlayerController();
    AIController = new AIController();
  }

  void Update() {
    Inputs.Clear();
    Inputs.Add(InputSnapshot.FromGlobalSingletons);
    Actions.Clear();
    if (IsPlayerTurn) {
      PlayerController.ProcessInputs(this, Inputs, ref Actions);
    } else {
      AIController.ProcessInputs(this, Inputs, ref Actions);
    }
    for (int i = 0; i < Actions.Count; i++) {
      ProcessAction(this, Actions[i]);
      LogAction(Actions[i]);
    }
    // Check collisions
    // Check winning conditions
    UpdateRenderables(this);
  }
}