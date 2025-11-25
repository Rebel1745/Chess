using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    protected List<AnalysisMoveDetails> _analysisMoves = new();
    public List<AnalysisMoveDetails> AnalysisMoves { get { return _analysisMoves; } }

    // animation stuff
    private bool _isPieceMoving = false;
    private Vector3 _moveToPosition;
    private float _moveSpeed = 20f;

    private void Update()
    {
        if (_isPieceMoving)
            MovePieceToPosition();
    }

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

        if (ToggleManager.Instance.IsAnalysisModeActivated)
            CalculateAnalysisMoves(checkForChecks);

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
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare,
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

            if (_availableMoves[i].EndSquare.PieceOnSquare != null)
                capturedPiece = _availableMoves[i].EndSquare.PieceOnSquare;
            else capturedPiece = null;

            // move to valid square
            SetPieceSquare(_availableMoves[i].EndSquare);
            _availableMoves[i].EndSquare.SetPieceOnSquare(this);
            currentSquare.SetPieceOnSquare(null);

            // check to see if on this new square the player would be in check
            // if they are that means that the piece was pinned and shouldn't be able to move
            if (PieceManager.Instance.CheckIfAnyPieceCanTakeKing(!_isWhite, capturedPiece))
                remove = true;

            // reset pieces and squares
            SetPieceSquare(currentSquare);
            _availableMoves[i].EndSquare.SetPieceOnSquare(capturedPiece);
            currentSquare.SetPieceOnSquare(this);

            if (remove) _availableMoves.RemoveAt(i);
        }
    }

    public virtual void CalculateAnalysisMoves(bool checkForChecks)
    {
        _analysisMoves.Clear();

        Square possibleMoveSquare;
        float lastXSign = Mathf.Sign(_basicMoves[0].x);
        float lastYSign = Mathf.Sign(_basicMoves[0].y);
        bool isCurrentDirectionXRay = false;

        foreach (Vector2Int move in _basicMoves)
        {
            // if the signs don't match, we are checking in a different direction, reset the ignore variable
            if (Mathf.Sign(move.x) != lastXSign || Mathf.Sign(move.y) != lastYSign)
                isCurrentDirectionXRay = false;

            possibleMoveSquare = BoardManager.Instance.GetSquare(_square.SquareX + move.x, _square.SquareY + move.y);

            // if there is no square, move on
            if (possibleMoveSquare == null) continue;

            // if we are to ignore x-ray moves and this is an x-ray, bail
            if (!ToggleManager.Instance.ShowXRayMoves && isCurrentDirectionXRay) continue;

            // if the piece is landing on a piece of the same colour, it is a protection move
            // following moves will be XRay moves
            if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite == _isWhite)
            {
                // we don't care about x-ray protection moves
                if (ToggleManager.Instance.ShowProtectionMoves && !isCurrentDirectionXRay)
                    _analysisMoves.Add(new AnalysisMoveDetails
                    {
                        StartSquare = _square,
                        EndSquare = possibleMoveSquare,
                        AnalysisMoveType = ANALYSIS_MOVE_TYPE.Protection,
                        IsXRayMove = isCurrentDirectionXRay
                    });

                isCurrentDirectionXRay = true;
                lastXSign = Mathf.Sign(move.x);
                lastYSign = Mathf.Sign(move.y);
            }
            // if we land on an oponent piece, we can't move past it
            else if (possibleMoveSquare.PieceOnSquare != null && possibleMoveSquare.PieceOnSquare.IsWhite != _isWhite)
            {
                if (ToggleManager.Instance.ShowCaptureMoves)
                    _analysisMoves.Add(new AnalysisMoveDetails
                    {
                        StartSquare = _square,
                        EndSquare = possibleMoveSquare,
                        AnalysisMoveType = ANALYSIS_MOVE_TYPE.Capture,
                        IsXRayMove = isCurrentDirectionXRay
                    });

                isCurrentDirectionXRay = true;
                lastXSign = Mathf.Sign(move.x);
                lastYSign = Mathf.Sign(move.y);
            }
            else
                _analysisMoves.Add(new AnalysisMoveDetails
                {
                    StartSquare = _square,
                    EndSquare = possibleMoveSquare,
                    AnalysisMoveType = ANALYSIS_MOVE_TYPE.Standard,
                    IsXRayMove = false
                });
        }

        if (checkForChecks)
            RemovePinnedPieceMovesFromAnalysisMoves();
    }

    public void RemovePinnedPieceMovesFromAnalysisMoves()
    {
        Square currentSquare;
        bool remove;
        Piece capturedPiece;

        // loop backwards through the list so elements can be removed as we go
        for (int i = _analysisMoves.Count - 1; i >= 0; i--)
        {
            remove = false;
            currentSquare = _square;

            if (_analysisMoves[i].EndSquare.PieceOnSquare != null)
                capturedPiece = _analysisMoves[i].EndSquare.PieceOnSquare;
            else capturedPiece = null;

            // move to valid square
            SetPieceSquare(_analysisMoves[i].EndSquare);
            _analysisMoves[i].EndSquare.SetPieceOnSquare(this);
            currentSquare.SetPieceOnSquare(null);

            // check to see if on this new square the player would be in check
            // if they are that means that the piece was pinned and shouldn't be able to move
            if (PieceManager.Instance.CheckIfAnyPieceCanTakeKing(!_isWhite, capturedPiece))
                remove = true;

            // reset pieces and squares
            SetPieceSquare(currentSquare);
            _analysisMoves[i].EndSquare.SetPieceOnSquare(capturedPiece);
            currentSquare.SetPieceOnSquare(this);

            if (remove) _analysisMoves.RemoveAt(i);
        }
    }

    public void ShowHideAvailableMoves(bool show)
    {
        if (_availableMoves.Count == 0) return;

        foreach (MoveDetails move in _availableMoves)
        {
            move.EndSquare.ShowHidePossibleMoveIndicator(show);
        }
    }

    public bool CheckIfValidMove(Square square, out MoveDetails moveDetails)
    {
        moveDetails = new MoveDetails();

        foreach (MoveDetails move in _availableMoves)
        {
            if (move.EndSquare == square)
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
            if (move.EndSquare == square)
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
            if (move.EndSquare.PieceOnSquare != null && move.EndSquare.PieceOnSquare.PieceType == PIECE_TYPE.King)
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

    public void AnimateToPosition(Vector3 position)
    {
        _moveToPosition = position;
        _isPieceMoving = true;
    }

    private void MovePieceToPosition()
    {
        transform.position = Vector3.MoveTowards(transform.position, _moveToPosition, _moveSpeed * Time.deltaTime);

        if (Vector3.Distance(_moveToPosition, transform.position) < 0.01)
        {
            transform.position = _moveToPosition;
            _isPieceMoving = false;
        }
    }

    public void ShowAnalysisArrows()
    {
        foreach (AnalysisMoveDetails move in _analysisMoves)
        {
            if (!ToggleManager.Instance.ShowXRayMoves && move.IsXRayMove) continue;

            ArrowManager.Instance.DrawArrow(move.StartSquare, move.EndSquare, move.AnalysisMoveType, true);
        }
    }

    public void RemoveAnalysisArrows()
    {
        foreach (AnalysisMoveDetails move in _analysisMoves)
        {
            ArrowManager.Instance.RemoveArrow(move.StartSquare, move.EndSquare);
        }
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