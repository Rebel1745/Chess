using UnityEngine;

public class PieceBishop : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        base.CalculateAvailableMoves(checkForChecks);

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
