using System;
using System.Collections.Generic;
using UnityEngine;

public class ToggleManager : MonoBehaviour
{
    public static ToggleManager Instance { get; private set; }

    private bool _showSafeSquares = false;
    [SerializeField] private Color _safeSquareColour;
    private bool _showDangerousSquares = false;
    [SerializeField] private Color _dangerousSquareColour;
    private List<Piece> _allPieces;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        PieceManager.Instance.OnMoveCompleted += PieceManager_OnMoveCompleted;
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        _allPieces = PieceManager.Instance.AllPieces;
    }

    private void PieceManager_OnMoveCompleted(object sender, PieceManager.OnMoveCompletedArgs e)
    {
        _allPieces = PieceManager.Instance.AllPieces;
        if (_showSafeSquares) ShowSafeSquares();
        if (_showDangerousSquares) ShowDangerousSquares();
    }

    public void ShowHideSafeSquares(bool show)
    {
        _showSafeSquares = show;

        if (show) ShowSafeSquares();
        else BoardManager.Instance.ResetSquareColours();
    }

    public void ShowHideDangerousSquares(bool show)
    {
        _showDangerousSquares = show;

        if (show) ShowDangerousSquares();
        else BoardManager.Instance.ResetSquareColours();
    }

    private void ShowSafeSquares()
    {
        // foreach (Piece piece in _allPieces)
        // {
        //     if (piece.AvailableMoveCount == 0) continue;

        //     foreach (MoveDetails move in piece.AvailableMoves)
        //     {
        //         if (move.MoveType == MOVE_TYPE.StandardMove) continue;

        //         move.MoveToSquare.SetSquareColour(_safeSquareColour);
        //     }
        // }
    }

    private void ShowDangerousSquares()
    {

    }
}
