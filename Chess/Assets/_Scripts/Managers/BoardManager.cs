using System.Text;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private Transform _boardHolder;
    [SerializeField] private GameObject _boardBackgroundPrefab;
    [SerializeField] private GameObject _lightSquarePrefab;
    [SerializeField] private GameObject _darkSquarePrefab;
    private readonly string[] _fileNames = { "a", "b", "c", "d", "e", "f", "g", "h" };
    private Square[,] _squares = new Square[8, 8];

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CreateBoard()
    {
        GameObject squarePrefab;
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
                squarePrefab = isLightSquare ? _lightSquarePrefab : _darkSquarePrefab;
                newSquareGO = Instantiate(squarePrefab, new Vector3(x, y, 0f), Quaternion.identity, _boardHolder);
                newSquareGO.name = "Square " + code;
                currentSquare = newSquareGO.GetComponent<Square>();
                currentSquare.SetupSquareDetails(x, y, code);
                _squares[x, y] = currentSquare;
                isLightSquare = !isLightSquare;
            }
        }

        GameManager.Instance.UpdateGameState(GameState.SetupPieces);
    }

    public Square GetSquare(int x, int y)
    {
        if (x > 7 || x < 0 || y > 7 || y < 0)
            return null;

        return _squares[x, y];
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

    public Square GetSquareFromPGNCode(string code)
    {
        foreach (Square square in _squares)
        {
            if (square.SquarePGNCode.ToUpper().Equals(code.ToUpper()))
                return square;
        }

        return null;
    }
}
