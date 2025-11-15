using TMPro;
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
    private Color _defaultSquareColour;
    public Color DefaultSquareColour { get { return _defaultSquareColour; } }
    private Color _previousSquareColour;
    private Color _previousOutlineColour;
    private bool _isHighlighted;
    public bool IsHighlighted { get { return _isHighlighted; } }

    [SerializeField] private SpriteRenderer _squareSprite;
    [SerializeField] private SpriteRenderer _squareOutlineSprite;
    [SerializeField] private GameObject _possibleMoveIndicator;
    [SerializeField] private TMP_Text _rankText;
    [SerializeField] private TMP_Text _fileText;

    public void SetupSquareDetails(int x, int y, string code, Color colour, Color otherColor)
    {
        _squareX = x;
        _squareY = y;
        _squarePGNCode = code;
        _defaultSquareColour = colour;

        if (code.Substring(0, 1).ToLower() == "a")
        {
            _rankText.text = code.Substring(1, 1);
            _rankText.color = otherColor;
            _rankText.gameObject.SetActive(true);
        }
        else _rankText.gameObject.SetActive(false);

        if (code.Substring(1, 1) == "1")
        {
            _fileText.text = code.Substring(0, 1).ToLower();
            _fileText.color = otherColor;
            _fileText.gameObject.SetActive(true);
        }
        else _fileText.gameObject.SetActive(false);

        SetSquareColour(colour, colour);
    }

    public void SetPieceOnSquare(Piece piece)
    {
        _pieceOnSquare = piece;
    }

    public void ShowHidePossibleMoveIndicator(bool show)
    {
        _possibleMoveIndicator.SetActive(show);
    }

    public void SetSquareColour(Color squareColour, Color outlineColour, bool isHighlighting = false)
    {
        if (isHighlighting)
        {
            if (_isHighlighted)
            {
                _squareSprite.color = _previousSquareColour;
                _squareOutlineSprite.color = _previousOutlineColour;
                _isHighlighted = false;
            }
            else
            {
                _isHighlighted = true;
                _previousSquareColour = _squareSprite.color;
                _previousOutlineColour = _squareOutlineSprite.color;

                _squareSprite.color = squareColour;
                _squareOutlineSprite.color = outlineColour;
            }
        }
        else
        {
            _squareSprite.color = squareColour;
            _squareOutlineSprite.color = outlineColour;
        }
    }

    public void SetSquareColour(Color color)
    {
        _squareSprite.color = color;
    }

    public void SetSquareOutlineColour(Color color)
    {
        _squareOutlineSprite.color = color;
    }

    public void ResetSquareColour()
    {
        _squareSprite.color = _defaultSquareColour;
        _squareOutlineSprite.color = _defaultSquareColour;
    }
}
