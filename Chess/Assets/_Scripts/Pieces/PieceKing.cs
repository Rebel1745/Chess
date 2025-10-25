using UnityEngine;

public class PieceKing : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        base.CalculateAvailableMoves(checkForChecks);

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
