using System.Collections.Generic; 
using UnityEngine;

public class PlayerController {
  public void ProcessInputs(in SampleGame game, in List<InputSnapshot> inputs, ref List<Action> Actions) {
    for (int i = 0; i < inputs.Count; i++) {
      ProcessInput(game, inputs[i], ref Actions);
    }
  }

  public void ProcessInput(in SampleGame game, in InputSnapshot input, ref List<Action> Actions) {
    var screenRay = game.MainCamera.ScreenPointToRay(input.MousePosition);
    var rayHitBoard = Physics.Raycast(screenRay, out RaycastHit hit);
    var didPick = input.MouseButtonDown && rayHitBoard;
    var hitCell = hit.point.FromWorldPosition();

    switch (game.CurrentState) {
      case SampleGame.State.Base: {
        if (input.RDown) {
          Actions.Add(new Action(Operation.BeginRotateTile));
        } else if (input.MDown) {
          Actions.Add(new Action(Operation.BeginMoveTile));
        } else if (input.PDown && game.Board.ContainsEmptyPositions()) {
          Actions.Add(new Action(Operation.BeginPlaceTile));
        } else if (input.WDown) {
          Actions.Add(new Action(Operation.BeginMoveWizard));
        } else if (input.DDown) {
          Actions.Add(new Action(Operation.BeginMoveDragon));
        }
      }
      break;

      case SampleGame.State.RotateTile: {
        if (didPick && game.Board.Tiles.TryGetIndexForCell(hitCell, out int tileIndex)) {
          Actions.Add(new Action(Operation.RotateTile, tileIndex));
        }
      }
      break;

      case SampleGame.State.PlaceTile: {
        if (didPick && game.Board.PlayablePositions.TryGetIndexForCell(hitCell, out int index)) {
          if (!game.Board.Tiles.HasMemberForCell(hitCell)) {
            Actions.Add(new Action(Operation.PlaceTile, index));
          }
        }
      }
      break;

      case SampleGame.State.MoveTile: {
        if (didPick && game.Board.Tiles.TryGetIndexForCell(hitCell, out int index)) {
          if (game.Board.TileHasMove(index)) {
            Actions.Add(new Action(Operation.SelectTile, index));
          }
        }
      }
      break;

      case SampleGame.State.TileToMoveSelected: {
        if (didPick && game.Board.PlayablePositions.TryGetIndexForCell(hitCell, out int index)) {
          var candidateCell = game.Board.PlayablePositions[index].Cell;
          var selectedCell = game.Board.Tiles[game.SelectedTileIndex].Cell;
          var isNeighbor = candidateCell.IsNeighborOf(selectedCell);
          var isEmpty = !game.Board.Tiles.HasMemberForCell(candidateCell);
          
          if (isNeighbor && isEmpty) {
            Actions.Add(new Action(Operation.MoveTile, candidateCell));
          }
        }
      }
      break;

      case SampleGame.State.DragonToMoveSelected: {
        if (didPick && game.Board.Tiles.TryGetIndexForCell(hitCell, out int index)) {
          var candidateCell = game.Board.Tiles[index].Cell;
          var selectedCell = game.Board.Dragons[game.SelectedPieceIndex].Cell;
          var isNeighbor = candidateCell.IsNeighborOf(selectedCell);
          var doesNotAlreadyHaveADragon = !game.Board.Dragons.HasMemberForCell(candidateCell);

          if (isNeighbor && doesNotAlreadyHaveADragon) {
            Actions.Add(new Action(Operation.MoveDragon, candidateCell));
          }
        }
      }
      break;

      case SampleGame.State.WizardToMoveSelected: {
        if (didPick && game.Board.Tiles.TryGetIndexForCell(hitCell, out int index)) {
          var candidateCell = game.Board.Tiles[index].Cell;
          var selectedCell = game.Board.Wizards[game.SelectedPieceIndex].Cell;
          var isNeighbor = candidateCell.IsNeighborOf(selectedCell);
          var doesNotAlreadyHaveAWizard = !game.Board.Wizards.HasMemberForCell(candidateCell);

          if (isNeighbor && doesNotAlreadyHaveAWizard) {
            Actions.Add(new Action(Operation.MoveWizard, candidateCell));
          }
        }
      }
      break;

      case SampleGame.State.MoveDragon: {
        if (didPick && game.Board.Dragons.TryGetIndexForCell(hitCell, out int index)) {
          Actions.Add(new Action(Operation.SelectDragon, index));
        }
      }
      break;

      case SampleGame.State.MoveWizard: {
        if (didPick && game.Board.Wizards.TryGetIndexForCell(hitCell, out int index)) {
          Actions.Add(new Action(Operation.SelectWizard, index));
        }
      }
      break;
    }
  }
}