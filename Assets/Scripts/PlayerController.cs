using System.Collections.Generic; 
using UnityEngine;

public class PlayerController {
  public void ProcessInputs(in SampleGame game, in List<InputSnapshot> inputs, ref List<Action> Actions) {
    for (int i = 0; i < inputs.Count; i++) {
      ProcessInput(game, inputs[i], ref Actions);
    }
  }

  public void ProcessInput(in SampleGame game, in InputSnapshot input, ref List<Action> Actions) {
    var mouseDown = input.MouseButtonDown;
    var screenRay = game.MainCamera.ScreenPointToRay(input.MousePosition);
    var rayHitBoard = Physics.Raycast(screenRay, out RaycastHit hit);

    switch (game.CurrentState) {
      case SampleGame.State.Base: {
        if (input.RDown) {
          Actions.Add(new Action(Operation.BeginRotateTile));
        } else if (input.MDown) {
          Actions.Add(new Action(Operation.BeginMoveTile));
        } else if (input.PDown) {
          Actions.Add(new Action(Operation.BeginPlaceTile));
        } else if (input.WDown) {
          Actions.Add(new Action(Operation.BeginMoveWizard));
        } else if (input.DDown) {
          Actions.Add(new Action(Operation.BeginMoveDragon));
        }
      }
      break;

      case SampleGame.State.RotateTile: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int tileIndex)) {
            Actions.Add(new Action(Operation.RotateTile, tileIndex));
          }
        }
      }
      break;

      case SampleGame.State.PlaceTile: {
        if (mouseDown && rayHitBoard) {
          // check if this is a valid playable position
          if (game.Board.PlayablePositions.TryGetIndexForCell(hit.point.FromWorldPosition(), out int positionIndex)) {
            // check if this cell already contains a tile
            if (!game.Board.Tiles.HasMemberForCell(hit.point.FromWorldPosition())) {
              Actions.Add(new Action(Operation.PlaceTile, positionIndex));
            }
          }
        }
      }
      break;

      case SampleGame.State.MoveTile: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int index)) {
            Actions.Add(new Action(Operation.SelectTile, index));
          }
        }
      }
      break;

      case SampleGame.State.TileToMoveSelected: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.PlayablePositions.TryGetIndexForCell(hit.point.FromWorldPosition(), out int positionIndex)) {
            var selectedCell = game.Board.Tiles[game.SelectedTileIndex].Cell;
            var candidateCell = game.Board.PlayablePositions[positionIndex].Cell;
            
            if (!game.Board.Tiles.HasMemberForCell(candidateCell) && candidateCell.IsNeighborOf(selectedCell)) {
              Actions.Add(new Action(Operation.MoveTile, candidateCell));
            }
          }
        }
      }
      break;

      case SampleGame.State.DragonToMoveSelected: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int tileIndex)) {
            var selectedCell = game.Board.Dragons[game.SelectedPieceIndex].Cell;
            var candidateCell = game.Board.Tiles[tileIndex].Cell;

            if (candidateCell.IsNeighborOf(selectedCell) && !game.Board.Dragons.HasMemberForCell(candidateCell)) {
              Actions.Add(new Action(Operation.MoveDragon, candidateCell));
            }
          }
        }
      }
      break;

      case SampleGame.State.WizardToMoveSelected: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.Tiles.TryGetIndexForCell(hit.point.FromWorldPosition(), out int tileIndex)) {
            var selectedCell = game.Board.Wizards[game.SelectedPieceIndex].Cell;
            var candidateCell = game.Board.Tiles[tileIndex].Cell;

            if (candidateCell.IsNeighborOf(selectedCell) && !game.Board.Wizards.HasMemberForCell(candidateCell)) {
              Actions.Add(new Action(Operation.MoveWizard, candidateCell));
            }
          }
        }
      }
      break;

      case SampleGame.State.MoveDragon: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.Dragons.TryGetIndexForCell(hit.point.FromWorldPosition(), out int index)) {
            Actions.Add(new Action(Operation.SelectDragon, index));
          }
        }
      }
      break;

      case SampleGame.State.MoveWizard: {
        if (mouseDown && rayHitBoard) {
          if (game.Board.Wizards.TryGetIndexForCell(hit.point.FromWorldPosition(), out int index)) {
            Actions.Add(new Action(Operation.SelectWizard, index));
          }
        }
      }
      break;
    }
  }
}