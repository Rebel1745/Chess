using UnityEngine;

public class Square : MonoBehaviour
{
    private int _squareX;
    public int SquareX { get { return _squareX; } }
    private int _squareY;
    public int SquareY { get { return _squareY; } }
    [SerializeField] private GameObject _possibleMoveIndicator;

    public void SetupSquareDetails(int x, int y)
    {
        _squareX = x;
        _squareY = y;
    }

    public void ShowHidePossibleMoveIndicator(bool show)
    {
        _possibleMoveIndicator.SetActive(show);
    }
}
