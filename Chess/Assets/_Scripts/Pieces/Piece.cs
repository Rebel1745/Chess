using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private Vector2Int[] _basicMoves;
    protected bool _isWhite;
    protected Square _square;
    public bool IsWhite { get { return _isWhite; } }
    protected bool _isFirstMove = true;

    protected List<Square> _availableMoves = new();

    public void SetupPiece(Square square, bool isWhite)
    {
        _isWhite = isWhite;
        _square = square;
    }

    public virtual void CalculateAvailableMoves()
    {
        _availableMoves.Clear();

        int startX = _square.SquareX;
        int startY = _square.SquareY;
        Square possibleMoveSquare;

        foreach (Vector2Int move in _basicMoves)
        {
            possibleMoveSquare = BoardManager.Instance.GetSquare(startX + move.x, startY + move.y);
            Debug.Log($"{possibleMoveSquare == null}");

            // if there is no square, move on
            if (possibleMoveSquare == null) continue;

            // if the piece is landing on a piece of the same colour, we can't go there, move on
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite == _isWhite) continue;

            _availableMoves.Add(possibleMoveSquare);
        }
        Debug.Log(_availableMoves.Count);
    }

    public void ShowHideAvailableMoves(bool show)
    {
        if (_availableMoves.Count == 0) return;

        foreach (Square square in _availableMoves)
        {
            square.ShowHidePossibleMoveIndicator(show);
        }
    }
}
