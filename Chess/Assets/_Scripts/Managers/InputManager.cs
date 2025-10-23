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

        _inputActions.Game.Click.started += StartMovePiece;
        _inputActions.Game.Click.canceled += EndMovePiece;
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
    private void StartMovePiece(InputAction.CallbackContext context)
    {
        if (_currentPiece == null && !_isAwaitingMove) return;

        if (_selectedPiece != null)
        {
            _selectedPiece.ShowHideAvailableMoves(false);
        }

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

    private void UpdateMovePiece()
    {
        _selectedPiece.transform.position = _mousePosition;
    }

    private void EndMovePiece(InputAction.CallbackContext context)
    {
        if (_currentPiece == null || _selectedPiece == null) return;

        _isMovingPiece = false;

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
    #endregion
}
