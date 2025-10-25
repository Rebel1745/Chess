using UnityEngine;

public class PieceKnight : Piece
{
    public override void CalculateAvailableMoves(bool checkForChecks)
    {
        base.CalculateAvailableMoves(checkForChecks);

        if (checkForChecks)
            RemovePinnedPieceMovesFromAvailableMoves();
    }
}
