using UnityEngine;

public class PieceRook : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        base.CalculateAvailableMoves(checkForChecks);

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
