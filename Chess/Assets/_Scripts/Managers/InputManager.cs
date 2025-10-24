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
    public Vector2 _mousePosition;
    public bool _isMovingPiece = false;
    public bool _isAwaitingMove = false;
    public Piece _selectedPiece;
    public Square _selectedPieceStartSquare;

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
        CheckForMouseOverPiece();
        CheckForMouseOverSquare();

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
        if (_currentPiece)
        {
            if (_currentPiece.IsWhite != GameManager.Instance.IsCurrentPlayerWhite) return;

            // if there is already a piece seleced, hide their available moves
            if (_selectedPiece) _selectedPiece.ShowHideAvailableMoves(false);

            //if (_selectedPiece != _currentPiece)
            //{
            // set the selected piece and show its move
            _selectedPiece = _currentPiece;
            _selectedPiece.CalculateAvailableMoves();
            _selectedPiece.ShowHideAvailableMoves(true);
            _isMovingPiece = true;
            //}
            /*else
            {
                // if this is the second time we click the piece, hide the moves
                _selectedPiece.ShowHideAvailableMoves(false);
                _selectedPiece = null;
            }*/
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
            // check if the current square is part of the available moves set of the piece
            if (_selectedPiece.CheckIfValidMove(_currentSquare))
                MovePiece(_selectedPiece, _currentSquare, true);
            else
            {
                MovePiece(_selectedPiece, _selectedPiece.Square, false);
            }
        }
    }

    private void MovePiece(Piece piece, Square square, bool isValid)
    {
        piece.Square.SetPieceOnSquare(null);
        piece.transform.position = square.transform.position;
        piece.ShowHideAvailableMoves(false);
        piece.SetPieceSquare(square);
        _selectedPiece = null;
        square.SetPieceOnSquare(piece);
        _currentSquare = null;
        _isMovingPiece = false;

        // the move was valid (i.e an actual move rather then resetting the piece to the origingal position)
        if (isValid)
            piece.SetIsFirstMove(false);
    }

    /*private void OnClickStarted(InputAction.CallbackContext context)
    {
        if (_currentPiece == null && !_isAwaitingMove) return;

        _selectedPiece?.ShowHideAvailableMoves(false);

        if (_currentPiece != null && _currentSquare != null)
        {
            _currentPiece.CalculateAvailableMoves();
            _currentPiece.ShowHideAvailableMoves(true);
        }

        if (_isAwaitingMove && _selectedPiece != null)
        {
            _selectedPiece.transform.position = _currentSquare.transform.position;
            _selectedPiece = null;
            _selectedPieceStartSquare = null;
        }
        else
        {
            _isMovingPiece = true;
            _selectedPiece = _currentPiece;
            _selectedPieceStartSquare = _currentSquare;
        }
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        if (_currentPiece == null || _selectedPiece == null) return;

        _isMovingPiece = false;

        if (_selectedPiece.CheckIfValidMove(_currentSquare))
        {
            _selectedPiece.transform.position = _currentSquare.transform.position;

            if (_selectedPieceStartSquare == _currentSquare)
                _isAwaitingMove = true;
            else
            {
                _selectedPiece.ShowHideAvailableMoves(false);
                _isAwaitingMove = false;
                _selectedPiece = _currentPiece;
                _selectedPieceStartSquare = _currentSquare;
            }
        }
        else
        {
            _selectedPiece.transform.position = _selectedPiece.Square.transform.position;
        }
    }*/

    private void UpdateMovePiece()
    {
        _selectedPiece.transform.position = _mousePosition;
    }
    #endregion
}
