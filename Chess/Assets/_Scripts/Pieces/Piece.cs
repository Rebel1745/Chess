using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    [SerializeField] protected PIECE_TYPE _pieceType;
    public PIECE_TYPE PieceType { get { return _pieceType; } }
    [SerializeField] protected Vector2Int[] _basicMoves;
    protected string _pieceCode;
    public string PieceCode { get { return _pieceCode; } }
    protected bool _isWhite;
    private bool _isPromotedPiece;
    public bool IsPromotedPiece { get { return _isPromotedPiece; } }
    protected Square _square;
    public Square Square { get { return _square; } }
    private Square _initialSquare;
    public Square InitialSquare { get { return _initialSquare; } }
    public bool IsWhite { get { return _isWhite; } }
    protected bool _isFirstMove = true;
    public bool IsFirstMove { get { return _isFirstMove; } }
    private bool _canBeEnPassanted = false;
    public bool IsCanBeEnPassanted { get { return _canBeEnPassanted; } }
    protected Piece[] _piecesWithTheSameMove;
    protected string _detailedPieceCode; // used when multiple pieces can move to the same square

    protected List<MoveDetails> _availableMoves = new();
    public List<MoveDetails> AvailableMoves { get { return _availableMoves; } }
    public int AvailableMoveCount { get { return _availableMoves.Count; } }

    public void SetupPiece(string pieceCode, Square square, bool isWhite, bool isPromotedPiece = false)
    {
        _pieceCode = pieceCode;
        _isWhite = isWhite;
        _square = square;
        _initialSquare = square;
        _isPromotedPiece = isPromotedPiece;
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
            {
                _piecesWithTheSameMove = PieceManager.Instance.GetPiecesByMove(possibleMoveSquare, _isWhite, _pieceType);

                if (_piecesWithTheSameMove.Length >= 1)
                    _detailedPieceCode = GetDetailedPieceCode();
                else
                    _detailedPieceCode = _pieceCode.ToUpper();

                _availableMoves.Add(new MoveDetails
                {
                    MoveNumber = -1,
                    isWhite = _isWhite,
                    PieceToMove = this,
                    MoveToSquare = possibleMoveSquare,
                    PGNCode = _detailedPieceCode + (possibleMoveSquare.PieceOnSquare == null ? "" : "x") + possibleMoveSquare.SquarePGNCode
                });
            }

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

    protected string GetDetailedPieceCode()
    {
        List<Piece> sameRankPieces = new();
        List<Piece> sameFilePieces = new();

        // first check to see if they are on different files
        foreach (Piece piece in _piecesWithTheSameMove)
        {
            // ignore our piece
            if (piece.Square.SquarePGNCode == _square.SquarePGNCode) continue;

            if (piece.Square.SquarePGNCode[..1] == _square.SquarePGNCode[..1]) sameRankPieces.Add(piece);
        }

        // if there are no pieces on the same rank as our piece, return the piece code and the rank
        if (sameRankPieces.Count == 0)
            return _pieceCode.ToUpper() + _square.SquarePGNCode[..1];

        // check to see if there are on different ranks
        foreach (Piece piece in _piecesWithTheSameMove)
        {
            // ignore our piece
            if (piece.Square.SquarePGNCode == _square.SquarePGNCode) continue;

            if (piece.Square.SquarePGNCode.Substring(1, 1) == _square.SquarePGNCode.Substring(1, 1)) sameFilePieces.Add(piece);
        }

        // if there are no pieces on the same file as our piece, return the piece code and the file
        if (sameFilePieces.Count == 0)
            return _pieceCode.ToUpper() + _square.SquarePGNCode.Substring(1, 1);
        else
            // otherwise return the piece code and the full code of the square it is on
            return _pieceCode.ToUpper() + _square.SquarePGNCode;
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