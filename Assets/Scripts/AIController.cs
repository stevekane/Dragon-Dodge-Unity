using System.Collections.Generic;
using UnityEngine;

public class AIController {
  static List<int> Indices = new List<int>();

  static Action[] BaseActions = new Action[] { 
    new Action(Operation.BeginRotateTile),
    new Action(Operation.BeginMoveTile),
    new Action(Operation.BeginPlaceTile),
    new Action(Operation.BeginMoveWizard),
    new Action(Operation.BeginMoveDragon)
  };

  static void PopulateIndexArray(int count, ref List<int> indexArray) {
    indexArray.Clear();
    for (int i = 0; i < count; i++) {
      indexArray.Add(i);
    }
  }

  static void Neighbors<T, R>(in List<LayerElement<T, R>> layer, in Vector2Int cell, ref List<int> neighbors) {
    neighbors.Clear();
    for (var i = 0; i < layer.Count; i++) {
      if (layer[i].Cell.IsNeighborOf(cell)) {
        neighbors.Add(i);
      }
    }
  }

  static void ShuffleInPlace<T>(ref List<T> xs) where T : struct {  
    var n = xs.Count;  

    while (n > 1) {
      n--;  
      int k = Random.Range(0, n + 1);
      T value = xs[k];  
      xs[k] = xs[n];
      xs[n] = value;  
    }  
  }

  static bool TryFindRandomEmptyPlayablePosition(in Board board, out int index) {
    PopulateIndexArray(board.PlayablePositions.Count, ref Indices);
    ShuffleInPlace(ref Indices);
    for (int i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];
      var candidateCell = board.PlayablePositions[candidateIndex].Cell;

      if (!board.Tiles.HasMemberForCell(candidateCell)) {
        index = candidateIndex;
        return true;
      }
    }
    index = -1;
    return false;
  }

  static bool TryFindRandomMovableTile(in Board board, out int index) {
    PopulateIndexArray(board.Tiles.Count, ref Indices);
    ShuffleInPlace(ref Indices);
    for (int i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];

      if (board.TileHasMove(candidateIndex)) {
        index = candidateIndex;
        return true;
      }
    }
    index = -1;
    return false;
  }

  static bool TryFindRandomEmptyNeighborCell(in Board board, in int tileIndex, out Vector2Int cell) {
    Neighbors(board.PlayablePositions, board.Tiles[tileIndex].Cell, ref Indices);
    ShuffleInPlace(ref Indices);
    for (int i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];
      var candidateCell = board.PlayablePositions[candidateIndex].Cell;

      if (!board.Tiles.HasMemberForCell(candidateCell)) {
        cell = candidateCell;
        return true;
      }
    }
    cell = default;
    return false;
  }

  static bool TryFindRandomValidCellForDragonMove(in Board board, in int dragonIndex, out Vector2Int cell) {
    Neighbors(board.Tiles, board.Dragons[dragonIndex].Cell, ref Indices);
    ShuffleInPlace(ref Indices);
    for (int i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];
      var candidateCell = board.Tiles[candidateIndex].Cell;
      var doesNotAlreadyHaveADragon = !board.Dragons.HasMemberForCell(candidateCell);

      if (doesNotAlreadyHaveADragon) {
        cell = candidateCell;
        return true;
      }
    }
    cell = default;
    return false;
  }

  static bool TryFindRandomValidCellForWizardMove(in Board board, in int wizardIndex, out Vector2Int cell) {
    Neighbors(board.Tiles, board.Wizards[wizardIndex].Cell, ref Indices);
    ShuffleInPlace(ref Indices);
    for (int i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];
      var candidateCell = board.Tiles[candidateIndex].Cell;
      var doesNotAlreadyHaveADragon = !board.Dragons.HasMemberForCell(candidateCell);
      var doesNotAlreadyHaveAWizard = !board.Wizards.HasMemberForCell(candidateCell);

      if (doesNotAlreadyHaveADragon && doesNotAlreadyHaveAWizard) {
        cell = candidateCell;
        return true;
      }
    }
    cell = default;
    return false;
  }

  static bool TryFindRandomMovableWizard(in Board board, out int index) {
    PopulateIndexArray(board.Wizards.Count, ref Indices);
    ShuffleInPlace(ref Indices);
    for (var i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];

      if (board.WizardHasMove(candidateIndex)) {
        index = candidateIndex;
        return true;
      }
    }
    index = -1;
    return false;
  }

  static bool TryFindRandomMovableDragon(in Board board, out int index) {
    PopulateIndexArray(board.Dragons.Count, ref Indices);
    ShuffleInPlace(ref Indices);
    for (var i = 0; i < Indices.Count; i++) {
      var candidateIndex = Indices[i];

      if (board.DragonHasMove(candidateIndex)) {
        index = candidateIndex;
        return true;
      }
    }
    index = -1;
    return false;
  }

  static T RandomElementFrom<T>(T[] xs) {
    return xs[Random.Range(0, xs.Length)];
  }

  float TimeRemainingTillNextAction = 0f;

  public void ProcessInputs(in SampleGame game, in List<InputSnapshot> inputs, ref List<Action> Actions) {
    for (int i = 0; i < inputs.Count; i++) {
      ProcessInput(game, inputs[i], ref Actions);
    }
  }

  public void ProcessInput(in SampleGame game, in InputSnapshot input, ref List<Action> Actions) {
    TimeRemainingTillNextAction -= input.DeltaTime;

    if (TimeRemainingTillNextAction > 0) {
      return;
    } else {
      TimeRemainingTillNextAction = .5f;
    }

    switch (game.CurrentState) {
      case SampleGame.State.Base: {
        Actions.Add(RandomElementFrom(BaseActions));
      }
      break;

      case SampleGame.State.RotateTile: {
        Actions.Add(new Action(Operation.RotateTile, Random.Range(0, game.Board.Tiles.Count)));
      }
      break;

      case SampleGame.State.PlaceTile: {
        if (TryFindRandomEmptyPlayablePosition(game.Board, out int index)) {
          Actions.Add(new Action(Operation.PlaceTile, index));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;

      case SampleGame.State.MoveTile: {
        if (TryFindRandomMovableTile(game.Board, out int index)) {
          Actions.Add(new Action(Operation.SelectTile, index));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;

      case SampleGame.State.TileToMoveSelected: {
        if (TryFindRandomEmptyNeighborCell(game.Board, game.SelectedTileIndex, out Vector2Int cell)) {
          Actions.Add(new Action(Operation.MoveTile, cell));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;

      case SampleGame.State.MoveDragon: {
        if (TryFindRandomMovableDragon(game.Board, out int index)) {
          Actions.Add(new Action(Operation.SelectDragon, index));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;

      case SampleGame.State.MoveWizard: {
        if (TryFindRandomMovableWizard(game.Board, out int index)) {
          Actions.Add(new Action(Operation.SelectWizard, index));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;

      case SampleGame.State.DragonToMoveSelected: {
        if (TryFindRandomValidCellForDragonMove(game.Board, game.SelectedPieceIndex, out Vector2Int cell)) {
          Actions.Add(new Action(Operation.MoveDragon, cell));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;

      case SampleGame.State.WizardToMoveSelected: {
        if (TryFindRandomValidCellForWizardMove(game.Board, game.SelectedPieceIndex, out Vector2Int cell)) {
          Actions.Add(new Action(Operation.MoveWizard, cell));
        } else {
          Actions.Add(RandomElementFrom(BaseActions));
        }
      }
      break;
    }
  }
}