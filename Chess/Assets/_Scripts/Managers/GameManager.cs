using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameState State { get; private set; }
    public GameState PreviousState { get; private set; }

    private bool _isCurrentPlayerWhite = true;
    public bool IsCurrentPlayerWhite { get { return _isCurrentPlayerWhite; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        UpdateGameState(GameState.CreateBoard);
    }

    public void UpdateGameState(GameState newState, float delay = 0f)
    {
        PreviousState = State;
        State = newState;

        switch (newState)
        {
            case GameState.CreateBoard:
                CreateBoard();
                break;
            case GameState.SetupPieces:
                SetupPieces();
                break;
        }
    }

    private void CreateBoard()
    {
        BoardManager.Instance.CreateBoard();
    }

    private void SetupPieces()
    {
        PieceManager.Instance.LoadDefaultPosition();
    }
}

public enum GameState
{
    None,
    CreateBoard,
    SetupPieces
}
