using System;
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

    private Piece _selectedPiece;
    //private Square _currentSquare;
    private bool _isMovingPiece;

    private List<Piece> _allPieces = new();

    private readonly string _defaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    //private readonly string _defaultPosition = "r1bk3r/pP1p1pNp/n4n2/1pN1P2P/6P1/3P4/PpP1K3/q5b1";

    public event EventHandler<OnMoveCompletedArgs> OnMoveCompleted;
    public class OnMoveCompletedArgs : EventArgs
    {
        public MoveDetails Move;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        InputManager.Instance.OnClickStarted += InputManager_OnClickStarted;
        InputManager.Instance.OnClickFinished += InputManager_OnClickFinished;
    }

    private void Update()
    {
        if (!_isMovingPiece) return;

        UpdateMovePiece();
    }

    private void InputManager_OnClickStarted(object sender, InputManager.OnClickArgs e)
    {

        if (GameManager.Instance.State == GameState.WaitingForPromotion)
        {
            if (e.PromotionPieceType == PIECE_TYPE.None) return;

            SelectPromotionPiece(new MoveDetails
            {
                PromotionPieceType = e.PromotionPieceType
            });
        }

        if (GameManager.Instance.State != GameState.WaitingForMove) return;

        if (e.CurrentPiece)
        {
            if (e.CurrentPiece.IsWhite != GameManager.Instance.IsCurrentPlayerWhite) return;

            // if there is already a piece seleced, hide their available moves
            if (_selectedPiece) _selectedPiece.ShowHideAvailableMoves(false);

            _selectedPiece = e.CurrentPiece;
            _selectedPiece.CalculateAvailableMoves(true);
            _selectedPiece.ShowHideAvailableMoves(true);
            _isMovingPiece = true;
            //_currentSquare = e.CurrentSquare;
        }
    }

    private void InputManager_OnClickFinished(object sender, InputManager.OnClickArgs e)
    {
        if (!_selectedPiece) return;

        if (_selectedPiece.Square == e.CurrentSquare)
        {
            _isMovingPiece = false;
            _selectedPiece.transform.position = e.CurrentSquare.transform.position;
            return;
        }

        if (e.CurrentSquare)
        {
            // check if the current square is part of the available move set of the piece
            if (_selectedPiece.CheckIfValidMove(e.CurrentSquare, out MoveDetails move))
                MovePiece(move);
            else
            {
                ResetPiecePosition(_selectedPiece, _selectedPiece.Square);
            }
        }
    }

    #region Position Functions
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
            if (!p.gameObject.activeInHierarchy) return;
            p.CalculateAvailableMoves(false);
        }

        BoardManager.Instance.GenerateBoardPositionFEN();
    }

    public void LoadDefaultPosition()
    {
        LoadPosition(_defaultPosition);
    }

    public void ResetBoardPosition()
    {
        Piece currentPiece;

        BoardManager.Instance.ResetAllSquares();

        for (int i = _allPieces.Count - 1; i >= 0; i--)
        {
            currentPiece = _allPieces[i];

            // hide pieces created through promotion
            if (currentPiece.IsPromotedPiece)
            {
                currentPiece.gameObject.SetActive(false);
                continue;
            }

            // activate inactive (through capture) pieces
            currentPiece.gameObject.SetActive(true);

            // reset the pieces square to the initial square it was on
            currentPiece.SetPieceSquare(currentPiece.InitialSquare);
            currentPiece.transform.position = currentPiece.InitialSquare.transform.position;

            // update the square with the current piece
            currentPiece.InitialSquare.SetPieceOnSquare(currentPiece);

            currentPiece.SetIsFirstMove(true);
        }
    }
    #endregion

    public bool CheckIfAnyPieceCanTakeKing(bool isWhite, Piece pieceToIgnore = null)
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
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
            if (!piece.gameObject.activeInHierarchy) continue;
            // we are checking the other colour, not the one making moves
            if (piece.IsWhite == GameManager.Instance.IsCurrentPlayerWhite) continue;

            piece.CalculateAvailableMoves(true);

            if (piece.AvailableMoveCount > 0)
                return false;
        }

        return true;
    }

    public void UpdateAllPieceMoves()
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            piece.CalculateAvailableMoves(false);
        }
    }

    public void TakePiece(Piece piece)
    {
        piece.gameObject.SetActive(false);
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

    public void SetAutomaticPromotion(MoveDetails move)
    {
        _pawnToPromote = move.PieceToMove;

        SelectPromotionPiece(move);
    }

    public void SelectPromotionPiece(MoveDetails move)
    {
        if (GameManager.Instance.IsCurrentPlayerWhite) _promotionPiecesWhite.SetActive(false);
        else _promotionPiecesBlack.SetActive(false);

        Square promotionSquare = _pawnToPromote.Square;
        GameObject newPieceGO;
        TakePiece(_pawnToPromote);
        GameObject newPiecePrefab = move.isWhite ? _whiteQueen : _blackQueen;
        string newPieceCode = move.isWhite ? "Q" : "q";

        switch (move.PromotionPieceType)
        {
            case PIECE_TYPE.Knight:
                newPiecePrefab = move.isWhite ? _whiteKnight : _blackKnight;
                newPieceCode = move.isWhite ? "N" : "n";
                break;
            case PIECE_TYPE.Bishop:
                newPiecePrefab = move.isWhite ? _whiteBishop : _blackBishop;
                newPieceCode = move.isWhite ? "B" : "b";
                break;
            case PIECE_TYPE.Rook:
                newPiecePrefab = move.isWhite ? _whiteRook : _blackRook;
                newPieceCode = move.isWhite ? "R" : "r";
                break;
        }

        if (move.PromotedPiece == null)
        {
            newPieceGO = Instantiate(newPiecePrefab, promotionSquare.transform.position, Quaternion.identity, _pieceHolder);
            move.PromotedPiece = newPieceGO;
            if (move.MoveNumber != -1)
                PGNManager.Instance.UpdatePromotedPieceGO(move.MoveNumber, newPieceGO);
        }
        else
        {
            newPieceGO = move.PromotedPiece;
            newPieceGO.SetActive(true);
            newPieceGO.transform.position = promotionSquare.transform.position;
        }

        Piece newPiece = newPieceGO.GetComponent<Piece>();
        newPiece.SetupPiece(newPieceCode, promotionSquare, move.isWhite, true);
        promotionSquare.SetPieceOnSquare(newPiece);
        _allPieces.Add(newPiece);
        newPiece.CalculateAvailableMoves(false);

        ResetCurrentPiece();

        PieceMoved(move);
    }

    public void SetPiecesAsNotEnPassantable(bool isWhite)
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite == isWhite)
                piece.SetPossibleEnPassant(false);
        }
    }

    #region Get Piece Functions
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

    public Piece GetPieceByMove(Square square, bool isWhite, PIECE_TYPE pieceType)
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
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

    public Piece[] GetPiecesByMove(Square square, bool isWhite, PIECE_TYPE pieceType)
    {
        List<Piece> pieceList = new();

        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            // the piece is the correct colour and type, check its moves
            foreach (MoveDetails move in piece.AvailableMoves)
            {
                if (move.MoveToSquare == square)
                {
                    pieceList.Add(move.PieceToMove);
                    break;
                }
            }
        }

        return pieceList.ToArray();
    }

    public Piece GetPieceByFile(string file, bool isWhite, PIECE_TYPE pieceType)
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            if (piece.Square.SquarePGNCode[..1] == file)
                return piece;
        }

        return null;
    }

    public Piece GetPieceByRank(int rank, bool isWhite, PIECE_TYPE pieceType)
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            if (int.Parse(piece.Square.SquarePGNCode.Substring(1, 1)) == rank)
                return piece;
        }

        return null;
    }

    public Piece GetPieceFromCharacter(string character, bool isWhite)
    {
        PIECE_TYPE pieceType = GetPieceTypeFromCharacter(character);

        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            if (piece.PieceType == pieceType)
                return piece;
        }

        return null;
    }
    #endregion

    #region Piece Movement
    public void MovePiece(MoveDetails move)
    {
        // if there is a piece on the square, capture it
        if (move.MoveToSquare.PieceOnSquare != null)
            TakePiece(move.MoveToSquare.PieceOnSquare);
        else if (move.RemovePieceEnPassant != null)
        {
            move.RemovePieceEnPassant.Square.SetPieceOnSquare(null);
            TakePiece(move.RemovePieceEnPassant);
        }

        // remove the piece from the current square
        move.PieceToMove.Square.SetPieceOnSquare(null);
        // set the pieces position to the new squares position
        move.PieceToMove.transform.position = move.MoveToSquare.transform.position;
        move.PieceToMove.ShowHideAvailableMoves(false);
        // set the new square of the piece
        move.PieceToMove.SetPieceSquare(move.MoveToSquare);
        _selectedPiece = null;
        // set the piece as the new piece on the square
        move.MoveToSquare.SetPieceOnSquare(move.PieceToMove);

        if (move.ActivatesEnPassant)
            move.PieceToMove.SetPossibleEnPassant(true);

        move.PieceToMove.SetIsFirstMove(false);

        // if we are castling, we need to move the rook as well
        if (move.SecondPieceToMove != null && move.SecondMoveToSquare != null)
        {
            // remove the piece from the current square
            move.SecondPieceToMove.Square.SetPieceOnSquare(null);
            // set the pieces position to the new squares position
            move.SecondPieceToMove.transform.position = move.SecondMoveToSquare.transform.position;
            move.SecondPieceToMove.ShowHideAvailableMoves(false);
            // set the new square of the piece
            move.SecondPieceToMove.SetPieceSquare(move.SecondMoveToSquare);
            // set the piece as the new piece on the square
            move.SecondMoveToSquare.SetPieceOnSquare(move.SecondPieceToMove);

            move.SecondPieceToMove.SetIsFirstMove(false);
        }

        // this should only be used moving through the PGN moves
        if (move.PromotionPieceType != PIECE_TYPE.None)
        {
            SetAutomaticPromotion(move);
        }

        //_currentSquare = null;
        _isMovingPiece = false;

        // if we didn't en passant this move, set any available en passantable pieces back to not en passantable
        SetPiecesAsNotEnPassantable(!GameManager.Instance.IsCurrentPlayerWhite);

        if (move.IsPromotion)
            ShowPromotionPieces(move);
        else
        {
            UpdateAllPieceMoves();
            PieceMoved(move);
        }
    }

    public void PieceMoved(MoveDetails move)
    {
        OnMoveCompleted?.Invoke(this, new()
        {
            Move = move
        });

        // check for check
        if (CheckIfAnyPieceCanTakeKing(GameManager.Instance.IsCurrentPlayerWhite))
        {
            if (CheckForMate())
            {
                // check mate baby
                Debug.Log("CheckMate");
                GameManager.Instance.UpdateGameState(GameState.GameOver);
            }
            else
            {
                //Debug.Log("Check");
                GameManager.Instance.UpdateGameState(GameState.NextTurn);
            }
        }
        else
            GameManager.Instance.UpdateGameState(GameState.NextTurn);
    }

    private void ResetPiecePosition(Piece piece, Square square)
    {
        piece.transform.position = square.transform.position;
        piece.ShowHideAvailableMoves(false);
        _selectedPiece = null;
        // _currentSquare = null;
        _isMovingPiece = false;
    }

    private void UpdateMovePiece()
    {
        _selectedPiece.transform.position = InputManager.Instance.MousePostion;
    }

    public void ResetCurrentPiece()
    {
        _selectedPiece = null;
    }
    #endregion
}
