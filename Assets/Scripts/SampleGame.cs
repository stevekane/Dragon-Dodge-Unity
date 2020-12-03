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
    WizardToMoveSelected,
    RotatingTile,
    PlacingTile,
    MovingTile,
    MovingDragon,
    MovingWizard,
    GameOver
  }

  [Header("Camera")]
  public Camera MainCamera;

  [Header("Authoring")]
  public BoardAuthoring AuthoringBoard;
  public TileSet TileSet;
  public BoardRenderables BoardRenderables;
  public int InitialRandomSeed = 1;
  public float InterpolationEpsilon = .001f;

  [Header("Animation Testing ONLY")]
  [Range(-1, 1)]
  public float DirectionHeadingDotProduct = 1;
  [Range(0, 1)]
  public float NormalizedMovementSpeed = 0;

  [Header("Runtime State")]
  public Board Board;
  public PlayerController PlayerController;
  public AIController AIController;
  public State CurrentState;
  public int SelectedTileIndex = -1;
  public int SelectedPieceIndex = -1;
  public bool IsPlayerTurn = true;
  public List<InputSnapshot> Inputs = new List<InputSnapshot>();
  public List<Action> Actions = new List<Action>();

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

        tileElement.Renderable.transform.position = cell.ToWorldPosition() - Vector3.up;
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
          game.Board.Wizards[wizardIndex].Renderable.SetNewPath(action.Cell.ToWorldPosition());
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
        var wizard = game.Board.Wizards[game.SelectedPieceIndex];
        var destination = action.Cell.ToWorldPosition();

        wizard.Cell = action.Cell;
        wizard.Renderable.SetNewPath(destination);
        game.Board.Wizards[game.SelectedPieceIndex] = wizard;
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

  public static void HandleCollisions(SampleGame game) {
    for (var i = game.Board.Wizards.Count - 1; i >= 0; i--) {
      for (var j = 0; j < game.Board.Dragons.Count; j++) {
        if (game.Board.Wizards[i].Cell.Equals(game.Board.Dragons[j].Cell)) {
          GameObject.Destroy(game.Board.Wizards[i].Renderable.gameObject);
          game.Board.Wizards.RemoveAt(i);
        }
      }
    }
  }

  public static void CheckWinningConditions(SampleGame game) {
    bool team1Alive = false;
    bool team2Alive = false;

    for (var i = 0; i < game.Board.Wizards.Count; i++) {
      if (game.Board.Wizards[i].Element.TeamIndex % 2 == 0) {
        team1Alive = true;
      } else {
        team2Alive = true;
      }
    }

    if (!team1Alive || !team2Alive) {
      game.CurrentState = State.GameOver;
    }
  }

  public static void UpdateRenderables(SampleGame game) {
    foreach (var e in game.Board.PlayablePositions) {
      e.Renderable.transform.position = Vector3.Lerp(e.Renderable.transform.position, e.Cell.ToWorldPosition(), game.InterpolationEpsilon);
    }

    foreach (var e in game.Board.Tiles) {
      var position = Vector3.Lerp(e.Renderable.transform.position, e.Cell.ToWorldPosition(), game.InterpolationEpsilon);
      var rotation = Quaternion.Slerp(e.Renderable.transform.rotation, Quaternion.AngleAxis(90f * (int)e.Element.CardinalRotation, Vector3.up), game.InterpolationEpsilon);

      e.Renderable.SetElements(e.Element);
      e.Renderable.transform.SetPositionAndRotation(position, rotation);
    }

    foreach (var e in game.Board.Dragons) {
      var destination = e.Cell.ToWorldPosition();
      var position = e.Renderable.transform.position;
      var direction = Vector3.Normalize(destination - position);

      if (direction.magnitude != 0) {
        var rotation = e.Renderable.transform.rotation;
        var desiredRotation = Quaternion.LookRotation(direction, Vector3.up);

        e.Renderable.transform.rotation = Quaternion.Slerp(rotation, desiredRotation, game.InterpolationEpsilon);
      }
      e.Renderable.transform.position = Vector3.Lerp(position, destination, game.InterpolationEpsilon);
    }

    foreach (var e in game.Board.Wizards) {
      e.Renderable.Tick(Time.deltaTime);
      e.Renderable.SetTeam(e.Element.TeamIndex);
    }
  }

  public static void LogAction(Action action) {
    Debug.Log($"{action.Operation}");
  }

  void Awake() {
    Random.InitState(InitialRandomSeed);
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
    HandleCollisions(this);
    CheckWinningConditions(this);
    UpdateRenderables(this);
  }
}