using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquareDetails : MonoBehaviour
{
    [SerializeField] private Image _squareImage;
    [SerializeField] private TMP_Text _squareText;
    [SerializeField] private Transform _whiteAttackingPiecesHolder;
    [SerializeField] private Transform _blackAttackingPiecesHolder;
    [SerializeField] private GameObject _whitePawnUIIcon;
    [SerializeField] private GameObject _whiteKnightUIIcon;
    [SerializeField] private GameObject _whiteBishopUIIcon;
    [SerializeField] private GameObject _whiteRookUIIcon;
    [SerializeField] private GameObject _whiteQueenUIIcon;
    [SerializeField] private GameObject _whiteKingUIIcon;
    [SerializeField] private GameObject _blackPawnUIIcon;
    [SerializeField] private GameObject _blackKnightUIIcon;
    [SerializeField] private GameObject _blackBishopUIIcon;
    [SerializeField] private GameObject _blackRookUIIcon;
    [SerializeField] private GameObject _blackQueenUIIcon;
    [SerializeField] private GameObject _blackKingUIIcon;
    public Piece[] WhitePieces;
    public Piece[] BlackPieces;

    public void SetSquareDetails(Square square)
    {
        _squareImage.color = square.DefaultSquareColour;
        _squareText.text = square.SquarePGNCode;

        BoardManager.Instance.GetPiecesAttackingSquare(square, out Piece[] whitePieces, out Piece[] blackPieces);

        // DEBUG
        WhitePieces = whitePieces;
        BlackPieces = blackPieces;
        //END DEBUG

        ShowPieceIcons(true, whitePieces, _whiteAttackingPiecesHolder);
        ShowPieceIcons(false, blackPieces, _blackAttackingPiecesHolder);
    }

    private void ShowPieceIcons(bool isWhite, Piece[] pieces, Transform pieceHolder)
    {
        int pawnCount = 0, knightCount = 0, bishopCount = 0, rookCount = 0, queenCount = 0, kingCount = 0;
        GameObject pieceIcon;

        foreach (Piece piece in pieces)
        {
            switch (piece.PieceType)
            {
                case PIECE_TYPE.Pawn:
                    pawnCount++;
                    break;
                case PIECE_TYPE.Knight:
                    knightCount++;
                    break;
                case PIECE_TYPE.Bishop:
                    bishopCount++;
                    break;
                case PIECE_TYPE.Rook:
                    rookCount++;
                    break;
                case PIECE_TYPE.Queen:
                    queenCount++;
                    break;
                case PIECE_TYPE.King:
                    kingCount++;
                    break;
            }
        }

        for (int i = 0; i < pieceHolder.childCount; i++)
        {
            Destroy(pieceHolder.GetChild(i).gameObject);
        }

        pieceIcon = isWhite ? _whitePawnUIIcon : _blackPawnUIIcon;

        for (int i = 0; i < pawnCount; i++)
        {
            Instantiate(pieceIcon, pieceHolder);
        }

        pieceIcon = isWhite ? _whiteKnightUIIcon : _blackKnightUIIcon;

        for (int i = 0; i < knightCount; i++)
        {
            Instantiate(pieceIcon, pieceHolder);
        }

        pieceIcon = isWhite ? _whiteBishopUIIcon : _blackBishopUIIcon;

        for (int i = 0; i < bishopCount; i++)
        {
            Instantiate(pieceIcon, pieceHolder);
        }

        pieceIcon = isWhite ? _whiteRookUIIcon : _blackRookUIIcon;

        for (int i = 0; i < rookCount; i++)
        {
            Instantiate(pieceIcon, pieceHolder);
        }

        pieceIcon = isWhite ? _whiteQueenUIIcon : _blackQueenUIIcon;

        for (int i = 0; i < queenCount; i++)
        {
            Instantiate(pieceIcon, pieceHolder);
        }

        pieceIcon = isWhite ? _whiteKingUIIcon : _blackKingUIIcon;

        for (int i = 0; i < kingCount; i++)
        {
            Instantiate(pieceIcon, pieceHolder);
        }
    }
}
