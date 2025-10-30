using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] private PIECE_TYPE _pieceType;
    public PIECE_TYPE PieceType { get { return _pieceType; } }
    [SerializeField] protected Vector2Int[] _basicMoves;
    private string _pieceCode;
    public string PieceCode { get { return _pieceCode; } }
    protected bool _isWhite;
    protected Square _square;
    public Square Square { get { return _square; } }
    public bool IsWhite { get { return _isWhite; } }
    protected bool _isFirstMove = true;
    public bool IsFirstMove { get { return _isFirstMove; } }
    private bool _canBeEnPassanted = false;
    public bool IsCanBeEnPassanted { get { return _canBeEnPassanted; } }

    protected List<MoveDetails> _availableMoves = new();
    public int AvailableMoveCount { get { return _availableMoves.Count; } }

    public void SetupPiece(string pieceCode, Square square, bool isWhite)
    {
        _pieceCode = pieceCode;
        _isWhite = isWhite;
        _square = square;
    }

    public void SetIsFirstMove(bool isFirstMove)
    {
        _isFirstMove = isFirstMove;
    }

    public void SetPieceSquare(Square square)
    {
        _square = square;
    }

    public void SetPossibleEnPassant(bool isPossibleEnPassant)
    {
        _canBeEnPassanted = isPossibleEnPassant;
    }

    public virtual void CalculateAvailableMoves(bool checkForChecks)
    {
        _availableMoves.Clear();

        int startX = _square.SquareX;
        int startY = _square.SquareY;
        Square possibleMoveSquare;
        bool ignoreCurrentDirection = false;
        float lastXSign = Mathf.Sign(_basicMoves[0].x);
        float lastYSign = Mathf.Sign(_basicMoves[0].y);

        foreach (Vector2Int move in _basicMoves)
        {
            // if the signs don't match, we are checking in a different direction, reset the ignore variable
            if (Mathf.Sign(move.x) != lastXSign || Mathf.Sign(move.y) != lastYSign)
                ignoreCurrentDirection = false;

            possibleMoveSquare = BoardManager.Instance.GetSquare(startX + move.x, startY + move.y);

            // if there is no square, move on
            if (possibleMoveSquare == null) continue;

            // if the piece is landing on a piece of the same colour, we can't go there, move on
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite == _isWhite)
            {
                ignoreCurrentDirection = true;
                lastXSign = Mathf.Sign(move.x);
                lastYSign = Mathf.Sign(move.y);
                continue;
            }

            if (!ignoreCurrentDirection)
                _availableMoves.Add(new MoveDetails
                {
                    PieceToMove = this,
                    MoveToSquare = possibleMoveSquare
                });

            // if we land on an oponent piece, we can't move past it
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
            {
                ignoreCurrentDirection = true;
                lastXSign = Mathf.Sign(move.x);
                lastYSign = Mathf.Sign(move.y);
            }
        }
    }

    public void RemovePinnedPieceMovesFromAvailableMoves()
    {
        Square currentSquare;
        bool remove;
        Piece capturedPiece;

        // loop backwards through the list so elements can be removed as we go
        for (int i = _availableMoves.Count - 1; i >= 0; i--)
        {
            remove = false;
            currentSquare = _square;

            if (_availableMoves[i].MoveToSquare.PieceOnSquare != null)
                capturedPiece = _availableMoves[i].MoveToSquare.PieceOnSquare;
            else capturedPiece = null;

            // move to valid square
            SetPieceSquare(_availableMoves[i].MoveToSquare);
            _availableMoves[i].MoveToSquare.SetPieceOnSquare(this);
            currentSquare.SetPieceOnSquare(null);

            // check to see if on this new square the player would be in check
            // if they are that means that the piece was pinned and shouldn't be able to move
            if (PieceManager.Instance.CheckIfAnyPieceCanTakeKing(!_isWhite, capturedPiece))
                remove = true;

            // reset pieces and squares
            SetPieceSquare(currentSquare);
            _availableMoves[i].MoveToSquare.SetPieceOnSquare(capturedPiece);
            currentSquare.SetPieceOnSquare(this);

            if (remove) _availableMoves.RemoveAt(i);
        }
    }

    public void ShowHideAvailableMoves(bool show)
    {
        if (_availableMoves.Count == 0) return;

        foreach (MoveDetails move in _availableMoves)
        {
            move.MoveToSquare.ShowHidePossibleMoveIndicator(show);
        }
    }

    public bool CheckIfValidMove(Square square, out MoveDetails moveDetails)
    {
        moveDetails = new MoveDetails();

        foreach (MoveDetails move in _availableMoves)
        {
            if (move.MoveToSquare == square)
            {
                moveDetails = move;
                return true;
            }
        }

        return false;
    }

    public bool CheckIfValidMove(Square square)
    {

        foreach (MoveDetails move in _availableMoves)
        {
            if (move.MoveToSquare == square)
            {
                return true;
            }
        }

        return false;
    }

    // if the piece can take the king, that means that the king would be in check
    public bool CheckIfPieceCanTakeKing()
    {
        foreach (MoveDetails move in _availableMoves)
        {
            if (move.MoveToSquare.PieceOnSquare != null && move.MoveToSquare.PieceOnSquare.PieceType == PIECE_TYPE.King)
                return true;
        }

        return false;
    }
}

public enum PIECE_TYPE
{
    None,
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}