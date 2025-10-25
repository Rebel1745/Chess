using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private Transform _boardHolder;
    [SerializeField] private GameObject _boardBackgroundPrefab;
    [SerializeField] private GameObject _lightSquarePrefab;
    [SerializeField] private GameObject _darkSquarePrefab;
    private readonly string[] _fileNames = { "A", "B", "C", "D", "E", "F", "G", "H" };
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

        // create board background
        Instantiate(_boardBackgroundPrefab, new Vector3(3.5f, 3.5f, 0f), Quaternion.identity, _boardHolder);

        // create squares
        for (int y = 0; y < 8; y++)
        {
            isLightSquare = !isLightSquare;

            for (int x = 0; x < 8; x++)
            {
                squarePrefab = isLightSquare ? _lightSquarePrefab : _darkSquarePrefab;
                newSquareGO = Instantiate(squarePrefab, new Vector3(x, y, 0f), Quaternion.identity, _boardHolder);
                newSquareGO.name = "Square " + _fileNames[x] + (y + 1);
                currentSquare = newSquareGO.GetComponent<Square>();
                currentSquare.SetupSquareDetails(x, y);
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
}
