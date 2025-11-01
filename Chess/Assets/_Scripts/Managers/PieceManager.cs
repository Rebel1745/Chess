using System.Collections.Generic;
using UnityEngine;

public class PieceManager : MonoBehaviour
{
    public static PieceManager Instance { get; private set; }

    [SerializeField] private Transform _pieceHolder;
    // piece game objects
    [SerializeField] private GameObject _whiteKing;
    [SerializeField] private GameObject _whiteQueen;
    [SerializeField] private GameObject _whiteRook;
    [SerializeField] private GameObject _whiteBishop;
    [SerializeField] private GameObject _whiteKnight;
    [SerializeField] private GameObject _whitePawn;
    [SerializeField] private GameObject _blackKing;
    [SerializeField] private GameObject _blackQueen;
    [SerializeField] private GameObject _blackRook;
    [SerializeField] private GameObject _blackBishop;
    [SerializeField] private GameObject _blackKnight;
    [SerializeField] private GameObject _blackPawn;
    [SerializeField] private GameObject _promtionPiecesWhitePrefab;
    private GameObject _promotionPiecesWhite;
    [SerializeField] private GameObject _promtionPiecesBlackPrefab;
    private GameObject _promotionPiecesBlack;
    private Piece _pawnToPromote;

    private List<Piece> _allPieces = new();

    private readonly string _defaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    //private readonly string _defaultPosition = "r1bk3r/pP1p1pNp/n4n2/1pN1P2P/6P1/3P4/PpP1K3/q5b1";

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void LoadPosition(string position)
    {
        string[] rankStrings = position.Split('/');
        string rank;
        int fileIndex;
        string currentChar;
        GameObject piece;
        GameObject newPieceGO;
        Piece newPiece;
        Square squareToSpawnPieceOn;

        // remove all pieces
        _allPieces.Clear();
        for (int i = 0; i < _pieceHolder.childCount; i++)
        {
            Destroy(_pieceHolder.GetChild(i).gameObject);
        }

        for (int i = rankStrings.Length - 1; i >= 0; i--)
        {
            fileIndex = 0;
            rank = rankStrings[7 - i];

            // if the only text is 8, it is a blank file. No need to do anything
            if (rank == "8") continue;

            // loop through the characters in each substring
            for (int j = 0; j < rank.Length; j++)
            {
                currentChar = rank.Substring(j, 1);

                if (int.TryParse(currentChar, out int currentNumber))
                {
                    // The string is a valid integer
                    // this represents empty files so advance the current file index by the number
                    fileIndex += currentNumber;
                }
                else
                {
                    // character is not a number so it is a piece
                    piece = GetPieceGOFromText(currentChar, out bool isWhite);
                    squareToSpawnPieceOn = BoardManager.Instance.GetSquare(fileIndex, i);
                    Vector3 spawnPos = new(squareToSpawnPieceOn.SquareX, squareToSpawnPieceOn.SquareY, 0f);
                    newPieceGO = Instantiate(piece, spawnPos, Quaternion.identity, _pieceHolder);
                    newPiece = newPieceGO.GetComponent<Piece>();
                    newPiece.SetupPiece(currentChar, squareToSpawnPieceOn, isWhite);
                    squareToSpawnPieceOn.SetPieceOnSquare(newPiece);
                    _allPieces.Add(newPiece);
                    fileIndex++;
                    newPieceGO.name = (isWhite ? "White " : "Black ") + newPiece.PieceType.ToString() + " " + fileIndex;
                    // if the piece is a pawn, and it is on the stating rank, mark it as first move
                    if ((currentChar == "p" && i != 6) || (currentChar == "P" && i != 1))
                    {
                        newPiece.SetIsFirstMove(false);
                    }
                }
            }
        }

        _promotionPiecesWhite = Instantiate(_promtionPiecesWhitePrefab, _pieceHolder);
        _promotionPiecesWhite.SetActive(false);
        _promotionPiecesBlack = Instantiate(_promtionPiecesBlackPrefab, _pieceHolder);
        _promotionPiecesBlack.SetActive(false);

        foreach (Piece p in _allPieces)
        {
            p.CalculateAvailableMoves(false);
        }

        BoardManager.Instance.GenerateBoardPositionFEN();
    }

    public void LoadDefaultPosition()
    {
        LoadPosition(_defaultPosition);
    }

    public PIECE_TYPE GetPieceTypeFromCharacter(string character)
    {
        PIECE_TYPE type = PIECE_TYPE.None;

        switch (character.ToUpper())
        {
            case "P":
                type = PIECE_TYPE.Pawn;
                break;
            case "N":
                type = PIECE_TYPE.Knight;
                break;
            case "B":
                type = PIECE_TYPE.Bishop;
                break;
            case "R":
                type = PIECE_TYPE.Rook;
                break;
            case "Q":
                type = PIECE_TYPE.Queen;
                break;
            case "K":
                type = PIECE_TYPE.King;
                break;
        }

        return type;
    }

    private GameObject GetPieceGOFromText(string character, out bool isWhite)
    {
        GameObject piece = _whiteKing;
        isWhite = true;

        switch (character)
        {
            case "K":
                piece = _whiteKing;
                isWhite = true;
                break;
            case "Q":
                piece = _whiteQueen;
                isWhite = true;
                break;
            case "R":
                piece = _whiteRook;
                isWhite = true;
                break;
            case "B":
                piece = _whiteBishop;
                isWhite = true;
                break;
            case "N":
                piece = _whiteKnight;
                isWhite = true;
                break;
            case "P":
                piece = _whitePawn;
                isWhite = true;
                break;
            case "k":
                piece = _blackKing;
                isWhite = false;
                break;
            case "q":
                piece = _blackQueen;
                isWhite = false;
                break;
            case "r":
                piece = _blackRook;
                isWhite = false;
                break;
            case "b":
                piece = _blackBishop;
                isWhite = false;
                break;
            case "n":
                piece = _blackKnight;
                isWhite = false;
                break;
            case "p":
                piece = _blackPawn;
                isWhite = false;
                break;
        }

        return piece;
    }

    public bool CheckIfAnyPieceCanTakeKing(bool isWhite, Piece pieceToIgnore = null)
    {
        foreach (Piece piece in _allPieces)
        {
            // piece to ignore allows us to simulate a capture without altering the pieces list
            if (pieceToIgnore != null && piece == pieceToIgnore) continue;

            if (piece.IsWhite != isWhite) continue;

            piece.CalculateAvailableMoves(false);

            if (piece.CheckIfPieceCanTakeKing())
                return true;
        }

        return false;
    }

    public bool CheckForMate()
    {
        // if any piece has moves, its not mate
        foreach (Piece piece in _allPieces)
        {
            // we are checking the other colour, not the one making moves
            if (piece.IsWhite == GameManager.Instance.IsCurrentPlayerWhite) continue;

            piece.CalculateAvailableMoves(true);

            if (piece.AvailableMoveCount > 0)
                return false;
        }

        return true;
    }

    public void TakePiece(Piece piece)
    {
        _allPieces.Remove(piece);
        Destroy(piece.gameObject);
    }

    public void ShowPromotionPieces(MoveDetails move)
    {
        Square promotionSquare = move.MoveToSquare;
        _pawnToPromote = move.PieceToMove;

        if (GameManager.Instance.IsCurrentPlayerWhite)
        {
            _promotionPiecesWhite.transform.position = new Vector3(promotionSquare.transform.position.x, promotionSquare.transform.position.y + 1.5f, 0f);
            _promotionPiecesWhite.SetActive(true);
        }
        else
        {
            _promotionPiecesBlack.transform.position = new Vector3(promotionSquare.transform.position.x, promotionSquare.transform.position.y - 1.5f, 0f);
            _promotionPiecesBlack.SetActive(true);
        }

        GameManager.Instance.UpdateGameState(GameState.WaitingForPromotion);
    }

    public void SelectPromotionPiece(PIECE_TYPE newPieceType)
    {
        if (GameManager.Instance.IsCurrentPlayerWhite) _promotionPiecesWhite.SetActive(false);
        else _promotionPiecesBlack.SetActive(false);

        Square promotionSquare = _pawnToPromote.Square;
        _allPieces.Remove(_pawnToPromote);
        Destroy(_pawnToPromote.gameObject);
        GameObject newPiecePrefab = GameManager.Instance.IsCurrentPlayerWhite ? _whiteQueen : _blackQueen;
        string newPieceCode = GameManager.Instance.IsCurrentPlayerWhite ? "Q" : "q";

        switch (newPieceType)
        {
            case PIECE_TYPE.Knight:
                newPiecePrefab = GameManager.Instance.IsCurrentPlayerWhite ? _whiteKnight : _blackKnight;
                newPieceCode = GameManager.Instance.IsCurrentPlayerWhite ? "N" : "n";
                break;
            case PIECE_TYPE.Bishop:
                newPiecePrefab = GameManager.Instance.IsCurrentPlayerWhite ? _whiteBishop : _blackBishop;
                newPieceCode = GameManager.Instance.IsCurrentPlayerWhite ? "B" : "b";
                break;
            case PIECE_TYPE.Rook:
                newPiecePrefab = GameManager.Instance.IsCurrentPlayerWhite ? _whiteRook : _blackRook;
                newPieceCode = GameManager.Instance.IsCurrentPlayerWhite ? "R" : "r";
                break;
        }

        GameObject newPieceGO = Instantiate(newPiecePrefab, promotionSquare.transform.position, Quaternion.identity, _pieceHolder);
        Piece newPiece = newPieceGO.GetComponent<Piece>();
        newPiece.SetupPiece(newPieceCode, promotionSquare, GameManager.Instance.IsCurrentPlayerWhite);
        promotionSquare.SetPieceOnSquare(newPiece);
        _allPieces.Add(newPiece);
        newPiece.CalculateAvailableMoves(false);

        InputManager.Instance.PieceMoved();
    }

    public void SetPiecesAsNotEnPassantable(bool isWhite)
    {
        foreach (Piece piece in _allPieces)
        {
            if (piece.IsWhite == isWhite)
                piece.SetPossibleEnPassant(false);
        }
    }

    public Piece GetPieceByMove(Square square, bool isWhite, PIECE_TYPE pieceType)
    {
        foreach (Piece piece in _allPieces)
        {
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            // the piece is the correct colour and type, check its moves
            foreach (MoveDetails move in piece.AvailableMoves)
            {
                if (move.MoveToSquare == square)
                    return move.PieceToMove;
            }
        }

        return null;
    }

    public Piece GetPieceByFile(string file, bool isWhite, PIECE_TYPE pieceType)
    {
        foreach (Piece piece in _allPieces)
        {
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            if (piece.Square.SquarePGNCode[..1] == file)
                return piece;
        }

        return null;
    }

    public Piece GetPieceFromCharacter(string character, bool isWhite)
    {
        PIECE_TYPE pieceType = GetPieceTypeFromCharacter(character);

        foreach (Piece piece in _allPieces)
        {
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            if (piece.PieceType == pieceType)
                return piece;
        }

        return null;
    }
}
