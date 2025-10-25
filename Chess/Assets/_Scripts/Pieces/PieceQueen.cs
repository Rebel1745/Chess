using UnityEngine;

public class PieceQueen : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        base.CalculateAvailableMoves(checkForChecks);

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
