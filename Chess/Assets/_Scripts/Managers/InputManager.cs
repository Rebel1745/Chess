using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public PlayerInput _inputActions;

    private bool _inputEnabled = true;
    [SerializeField] private LayerMask _whatIsPiece;
    [SerializeField] private LayerMask _whatIsSquare;
    [SerializeField] private LayerMask _whatIsPromotionPiece;
    private Vector2 _mousePosition;
    public bool _isMovingPiece = false;
    public bool _isAwaitingMove = false;
    public Piece _selectedPiece;
    public Square _selectedPieceStartSquare;
    private PIECE_TYPE _promotionPieceType;

    public Square _currentSquare;
    public Square CurrentSquare { get { return _currentSquare; } }

    public Piece _currentPiece;
    public Piece CurrentPiece { get { return _currentPiece; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        _inputActions = new PlayerInput();
        _inputActions.Enable();

        _inputActions.Game.Click.started += OnClickStarted;
        _inputActions.Game.Click.canceled += OnClickCanceled;
    }

    private void Update()
    {
        if (!_inputEnabled) return;

        GetMousePosition();

        if (GameManager.Instance.State == GameState.WaitingForMove)
        {
            CheckForMouseOverPiece();
            CheckForMouseOverSquare();
        }

        if (GameManager.Instance.State == GameState.WaitingForPromotion)
            CheckForMouseOverPromotionPiece();

        if (_isMovingPiece)
            UpdateMovePiece();
    }

    #region Get current piece / square
    private void GetMousePosition()
    {
        _mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
    }

    private void CheckForMouseOverPiece()
    {
        Collider2D hit = Physics2D.OverlapPoint(_mousePosition, _whatIsPiece);

        if (hit)
            _currentPiece = hit.GetComponent<Piece>();
        else _currentPiece = null;
    }

    public void ResetCurrentPiece()
    {
        _currentPiece = null;
        _selectedPiece = null;
    }

    private void CheckForMouseOverPromotionPiece()
    {
        Collider2D hit = Physics2D.OverlapPoint(_mousePosition, _whatIsPromotionPiece);

        if (hit)
            _promotionPieceType = hit.GetComponent<PromotionPiece>().PieceType;
        else _promotionPieceType = PIECE_TYPE.None;
    }

    private void CheckForMouseOverSquare()
    {
        Collider2D hit = Physics2D.OverlapPoint(_mousePosition, _whatIsSquare);

        if (hit)
            _currentSquare = hit.GetComponent<Square>();
        else _currentSquare = null;
    }
    #endregion

    #region Move Piece
    private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (GameManager.Instance.State == GameState.WaitingForPromotion)
        {
            if (_promotionPieceType == PIECE_TYPE.None) return;

            PieceManager.Instance.SelectPromotionPiece(new MoveDetails
            {
                PromotionPieceType = _promotionPieceType
            });
        }

        if (GameManager.Instance.State != GameState.WaitingForMove) return;

        if (_currentPiece)
        {
            if (_currentPiece.IsWhite != GameManager.Instance.IsCurrentPlayerWhite) return;

            // if there is already a piece seleced, hide their available moves
            if (_selectedPiece) _selectedPiece.ShowHideAvailableMoves(false);

            _selectedPiece = _currentPiece;
            _selectedPiece.CalculateAvailableMoves(true);
            _selectedPiece.ShowHideAvailableMoves(true);
            _isMovingPiece = true;
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        if (!_selectedPiece) return;

        if (_selectedPiece.Square == _currentSquare)
        {
            _isMovingPiece = false;
            _selectedPiece.transform.position = _currentSquare.transform.position;
            return;
        }

        if (_currentSquare)
        {
            // check if the current square is part of the available move set of the piece
            if (_selectedPiece.CheckIfValidMove(_currentSquare, out MoveDetails move))
                MovePiece(move);
            else
            {
                ResetPiecePosition(_selectedPiece, _selectedPiece.Square);
            }
        }
    }

    public void MovePiece(MoveDetails move)
    {
        // if there is a piece on the square, capture it
        if (move.MoveToSquare.PieceOnSquare != null)
            PieceManager.Instance.TakePiece(move.MoveToSquare.PieceOnSquare);
        else if (move.RemovePieceEnPassant != null)
        {
            move.RemovePieceEnPassant.Square.SetPieceOnSquare(null);
            PieceManager.Instance.TakePiece(move.RemovePieceEnPassant);
        }

        // remove the piece from the current square
        move.PieceToMove.Square.SetPieceOnSquare(null);
        // set the pieces position to the new squares position
        move.PieceToMove.transform.position = move.MoveToSquare.transform.position;
        move.PieceToMove.ShowHideAvailableMoves(false);
        // set the new square of the piece
        move.PieceToMove.SetPieceSquare(move.MoveToSquare);
        _selectedPiece = null;
        // set the piece as the new piece on the square
        move.MoveToSquare.SetPieceOnSquare(move.PieceToMove);

        if (move.ActivatesEnPassant)
            move.PieceToMove.SetPossibleEnPassant(true);

        move.PieceToMove.SetIsFirstMove(false);

        // if we are castling, we need to move the rook as well
        if (move.SecondPieceToMove != null && move.SecondMoveToSquare != null)
        {
            // remove the piece from the current square
            move.SecondPieceToMove.Square.SetPieceOnSquare(null);
            // set the pieces position to the new squares position
            move.SecondPieceToMove.transform.position = move.SecondMoveToSquare.transform.position;
            move.SecondPieceToMove.ShowHideAvailableMoves(false);
            // set the new square of the piece
            move.SecondPieceToMove.SetPieceSquare(move.SecondMoveToSquare);
            // set the piece as the new piece on the square
            move.SecondMoveToSquare.SetPieceOnSquare(move.SecondPieceToMove);

            move.SecondPieceToMove.SetIsFirstMove(false);
        }

        // this should only be used moving through the PGN moves
        if (move.PromotionPieceType != PIECE_TYPE.None)
        {
            PieceManager.Instance.SetAutomaticPromotion(move);
        }

        _currentSquare = null;
        _isMovingPiece = false;

        // if we didn't en passant this move, set any available en passantable pieces back to not en passantable
        PieceManager.Instance.SetPiecesAsNotEnPassantable(!GameManager.Instance.IsCurrentPlayerWhite);

        if (move.IsPromotion)
            PieceManager.Instance.ShowPromotionPieces(move);
        else
        {
            PieceManager.Instance.UpdateAllPieceMoves();
            PieceMoved();
        }
    }

    public void PieceMoved()
    {
        BoardManager.Instance.GenerateBoardPositionFEN();
        // check for check
        if (PieceManager.Instance.CheckIfAnyPieceCanTakeKing(GameManager.Instance.IsCurrentPlayerWhite))
        {
            if (PieceManager.Instance.CheckForMate())
            {
                // check mate baby
                Debug.Log("CheckMate");
                GameManager.Instance.UpdateGameState(GameState.GameOver);
            }
            else
            {
                //Debug.Log("Check");
                GameManager.Instance.UpdateGameState(GameState.NextTurn);
            }
        }
        else
            GameManager.Instance.UpdateGameState(GameState.NextTurn);
    }

    private void ResetPiecePosition(Piece piece, Square square)
    {
        piece.transform.position = square.transform.position;
        piece.ShowHideAvailableMoves(false);
        _selectedPiece = null;
        _currentSquare = null;
        _isMovingPiece = false;
    }

    private void UpdateMovePiece()
    {
        _selectedPiece.transform.position = _mousePosition;
    }
    #endregion
}
