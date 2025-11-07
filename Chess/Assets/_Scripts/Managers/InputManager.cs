using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    private PlayerInput _inputActions;

    private bool _inputEnabled = true;
    [SerializeField] private LayerMask _whatIsPiece;
    [SerializeField] private LayerMask _whatIsSquare;
    [SerializeField] private LayerMask _whatIsPromotionPiece;
    private Vector2 _mousePosition;
    public Vector2 MousePostion { get { return _mousePosition; } }
    private PIECE_TYPE _promotionPieceType;

    private Square _currentSquare;
    private Piece _currentPiece;

    public class OnClickArgs : EventArgs
    {
        public Square CurrentSquare;
        public Piece CurrentPiece;
        public PIECE_TYPE PromotionPieceType = PIECE_TYPE.None;
    }
    public event EventHandler<OnClickArgs> OnClickStarted;
    public event EventHandler<OnClickArgs> OnClickFinished;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void OnEnable()
    {
        _inputActions = new PlayerInput();
        _inputActions.Enable();

        _inputActions.Game.Click.started += OnClick;
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
    private void OnClick(InputAction.CallbackContext context)
    {
        OnClickStarted?.Invoke(this, new OnClickArgs()
        {
            CurrentPiece = _currentPiece,
            CurrentSquare = _currentSquare,
            PromotionPieceType = _promotionPieceType
        });
    }

    private void OnClickCanceled(InputAction.CallbackContext context)
    {
        OnClickFinished?.Invoke(this, new OnClickArgs()
        {
            CurrentPiece = _currentPiece,
            CurrentSquare = _currentSquare,
            PromotionPieceType = _promotionPieceType
        });
    }
    #endregion
}
