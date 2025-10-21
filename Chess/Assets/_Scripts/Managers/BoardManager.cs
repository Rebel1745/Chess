using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }

    [SerializeField] private Transform _boardHolder;
    [SerializeField] private GameObject _boardBackgroundPrefab;
    [SerializeField] private GameObject _lightSquarePrefab;
    [SerializeField] private GameObject _darkSquarePrefab;
    private readonly string[] _fileNames = { "A", "B", "C", "D", "E", "F", "G", "H" };

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void CreateBoard()
    {
        GameObject squarePrefab;
        GameObject newSquareGO;
        int squareIndex;
        bool isLightSquare = true;

        // create board background
        Instantiate(_boardBackgroundPrefab, new Vector3(3.5f, 3.5f, 0f), Quaternion.identity, _boardHolder);

        // create squares
        for (int y = 0; y < 8; y++)
        {
            isLightSquare = !isLightSquare;

            for (int x = 0; x < 8; x++)
            {
                squareIndex = (y * 8) + x;
                squarePrefab = isLightSquare ? _lightSquarePrefab : _darkSquarePrefab;
                newSquareGO = Instantiate(squarePrefab, new Vector3(x, y, 0f), Quaternion.identity, _boardHolder);
                newSquareGO.name = "(" + x + ", " + y + ") Square ( " + squareIndex + ", " + (squareIndex % 2 == 0) + " ) " + _fileNames[x] + (y + 1);
                isLightSquare = !isLightSquare;
            }
        }

        GameManager.Instance.UpdateGameState(GameState.SetupPieces);
    }
}
