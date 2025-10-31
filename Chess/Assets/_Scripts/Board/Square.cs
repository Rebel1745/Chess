using UnityEngine;

public class Square : MonoBehaviour
{
    private int _squareX;
    public int SquareX { get { return _squareX; } }
    private int _squareY;
    public int SquareY { get { return _squareY; } }
    private string _squarePGNCode;
    public string SquarePGNCode { get { return _squarePGNCode; } }
    private Piece _pieceOnSquare;
    public Piece PieceOnSquare { get { return _pieceOnSquare; } }

    [SerializeField] private GameObject _possibleMoveIndicator;

    public void SetupSquareDetails(int x, int y, string code)
    {
        _squareX = x;
        _squareY = y;
        _squarePGNCode = code;
    }

    public void SetPieceOnSquare(Piece piece)
    {
        _pieceOnSquare = piece;
    }

    public void ShowHidePossibleMoveIndicator(bool show)
    {
        _possibleMoveIndicator.SetActive(show);
    }
}
