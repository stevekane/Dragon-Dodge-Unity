using System.Collections.Generic;
using UnityEngine;

public class AIController {
  static List<int> ShuffledIndexCache = new List<int>();

  static Action[] BaseActions = new Action[] { 
    new Action(Operation.BeginRotateTile),
    new Action(Operation.BeginMoveTile),
    new Action(Operation.BeginPlaceTile),
    // new Action(Operation.BeginMoveWizard),
    // new Action(Operation.BeginMoveDragon)
  };

  public static void PopulateIndexArray(int count, ref List<int> indexArray) {
    indexArray.Clear();
    for (int i = 0; i < count; i++) {
      indexArray.Add(i);
    }
  }

  public static void ShuffleInPlace<T>(ref List<T> xs, int seed = 1) where T : struct {  
    var n = xs.Count;  

    Random.InitState(seed);
    while (n > 1) {
      n--;  
      int k = Random.Range(0, n);
      T value = xs[k];  
      xs[k] = xs[n];
      xs[n] = value;  
    }  
  }

  static bool TryFindRandomEmptyPlayablePosition(in Board board, out int index) {
    PopulateIndexArray(board.PlayablePositions.Count, ref ShuffledIndexCache);
    ShuffleInPlace(ref ShuffledIndexCache, seed: 1);
    for (int i = 0; i < ShuffledIndexCache.Count; i++) {
      var candidateIndex = ShuffledIndexCache[i];
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
    PopulateIndexArray(board.Tiles.Count, ref ShuffledIndexCache);
    ShuffleInPlace(ref ShuffledIndexCache, seed: 1);
    for (int i = 0; i < ShuffledIndexCache.Count; i++) {
      var candidateIndex = ShuffledIndexCache[i];

      // TODO: This extension method is really awkward... make this more obvious
      if (candidateIndex.TileHasMove(board)) {
        index = candidateIndex;
        return true;
      }
    }
    index = -1;
    return false;
  }

  static void NeighborPlayablePositions(in Board board, in Vector2Int cell, ref List<int> neighbors) {
    neighbors.Clear();
    for (var i = 0; i < board.PlayablePositions.Count; i++) {
      if (board.PlayablePositions[i].Cell.IsNeighborOf(cell)) {
        neighbors.Add(i);
      }
    }
  }

  static bool TryFindRandomEmptyNeighborCell(in Board board, in int tileIndex, out Vector2Int cell) {
    NeighborPlayablePositions(board, board.Tiles[tileIndex].Cell, ref ShuffledIndexCache);
    ShuffleInPlace(ref ShuffledIndexCache);
    for (int i = 0; i < ShuffledIndexCache.Count; i++) {
      var candidateIndex = ShuffledIndexCache[i];
      var candidateCell = board.PlayablePositions[candidateIndex].Cell;

      if (!board.Tiles.HasMemberForCell(candidateCell)) {
        cell = candidateCell;
        return true;
      }
    }
    cell = default;
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

    if (TimeRemainingTillNextAction > 0)
      return;

    TimeRemainingTillNextAction = 2f;
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

      case SampleGame.State.DragonToMoveSelected: {
      }
      break;

      case SampleGame.State.WizardToMoveSelected: {
      }
      break;

      case SampleGame.State.MoveDragon: {
      }
      break;

      case SampleGame.State.MoveWizard: {
      }
      break;
    }
  }
}