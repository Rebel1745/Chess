using System;
using System.Text;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private Transform _boardHolder;
    [SerializeField] private GameObject _boardBackgroundPrefab;
    [SerializeField] private GameObject _squarePrefab;
    [SerializeField] private Color _lightSquareColour;
    [SerializeField] private Color _darkSquareColour;
    [SerializeField] private Color _moveHighlightColour;
    [SerializeField] private Color _moveHighlightOutlineColour;
    [SerializeField] private Color _squareHighlightColour;
    [SerializeField] private Color _squareHighlightOutlineColour;
    private readonly string[] _fileNames = { "a", "b", "c", "d", "e", "f", "g", "h" };
    private Square[,] _squares = new Square[8, 8];
    public Square[,] AllSquares { get { return _squares; } }

    // arrows
    private Square _startSquare;
    private bool _isDragging = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        PieceManager.Instance.OnMoveCompleted += PieceManager_OnMoveCompleted;
        InputManager.Instance.OnRightClickStarted += InputManager_OnRightClickStarted;
        InputManager.Instance.OnRightClickFinished += InputManager_OnRightClickFinished;
    }

    private void PieceManager_OnMoveCompleted(object sender, PieceManager.OnMoveCompletedArgs e)
    {
        GenerateBoardPositionFEN();
    }

    private void InputManager_OnRightClickStarted(object sender, InputManager.OnRightClickArgs e)
    {
        // arrow stuff
        if (e.CurrentSquare)
        {
            // we are right clicking on a square
            _startSquare = e.CurrentSquare;
            _isDragging = true;
        }
    }

    private void InputManager_OnRightClickFinished(object sender, InputManager.OnRightClickArgs e)
    {
        // if we have started on one square and moved to another square, draw a line between the two
        if (e.CurrentSquare && _isDragging && _startSquare != null && e.CurrentSquare != _startSquare)
        {
            ArrowManager.Instance.DrawArrow(_startSquare, e.CurrentSquare);
            _startSquare = null;
            _isDragging = false;
        }

        if (e.CurrentSquare == _startSquare)
            HighlightSquare(e.CurrentSquare);
    }

    public void CreateBoard()
    {
        Color squareColor;
        GameObject newSquareGO;
        bool isLightSquare = true;
        Square currentSquare;
        string code;

        // create board background
        Instantiate(_boardBackgroundPrefab, new Vector3(3.5f, 3.5f, 0f), Quaternion.identity, _boardHolder);

        // create squares
        for (int y = 0; y < 8; y++)
        {
            isLightSquare = !isLightSquare;

            for (int x = 0; x < 8; x++)
            {
                code = _fileNames[x] + (y + 1);
                squareColor = isLightSquare ? _lightSquareColour : _darkSquareColour;
                newSquareGO = Instantiate(_squarePrefab, new Vector3(x, y, 0f), Quaternion.identity, _boardHolder);
                newSquareGO.name = "Square " + code;
                currentSquare = newSquareGO.GetComponent<Square>();
                currentSquare.SetupSquareDetails(x, y, code, squareColor);
                _squares[x, y] = currentSquare;
                isLightSquare = !isLightSquare;
            }
        }

        GameManager.Instance.UpdateGameState(GameState.SetupPieces);
    }

    public void ResetAllSquares()
    {
        for (int rank = 0; rank < 8; rank++)
        {
            for (int file = 0; file < 8; file++)
            {
                _squares[rank, file].SetPieceOnSquare(null);
            }
        }
    }

    public Square GetSquare(int x, int y)
    {
        if (x > 7 || x < 0 || y > 7 || y < 0)
            return null;

        return _squares[x, y];
    }

    public Square GetSquareFromPGNCode(string code)
    {
        foreach (Square square in _squares)
        {
            if (square.SquarePGNCode.ToUpper().Equals(code.ToUpper()))
                return square;
        }

        return null;
    }

    public void GenerateBoardPositionFEN()
    {
        Square currentSquare;
        int emptyCount;
        StringBuilder fen = new();

        for (int rank = 7; rank >= 0; rank--)
        {
            emptyCount = 0;
            for (int file = 0; file < 8; file++)
            {
                currentSquare = GetSquare(file, rank);

                if (currentSquare.PieceOnSquare == null)
                    emptyCount++;
                else
                {
                    if (emptyCount != 0)
                    {
                        fen.Append(emptyCount);
                        emptyCount = 0;
                    }

                    fen.Append(currentSquare.PieceOnSquare.PieceCode);
                }
            }
            if (emptyCount != 0)
                fen.Append(emptyCount);
            if (rank > 0)
                fen.Append("/");
        }

        UIManager.Instance.UpdateFENText(fen.ToString());
    }

    public void ResetSquareColours()
    {
        for (int rank = 7; rank >= 0; rank--)
        {
            for (int file = 0; file < 8; file++)
            {
                _squares[rank, file].ResetSquareColour();
            }
        }
    }

    public void HighlightCurrentMove(MoveDetails move)
    {
        move.StartingSquare.SetSquareColour(_moveHighlightColour, _moveHighlightOutlineColour);
        move.MoveToSquare.SetSquareColour(_moveHighlightColour, _moveHighlightOutlineColour);
    }

    public void FlipBoard()
    {
        PieceManager.Instance.FlipPieces();
        CameraManager.Instance.FlipCamera();
    }

    private void HighlightSquare(Square square)
    {
        ResetSquareColours();
        square.SetSquareColour(_squareHighlightColour, _squareHighlightOutlineColour);
    }
}
