using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Constraints;
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

    private MoveDetails _currentMove;
    private Piece _selectedPiece;
    //private Square _currentSquare;
    private bool _isMovingPiece;

    [SerializeField] private Color _checkmateSquareColour;
    [SerializeField] private Color _checkmateOutlineColour;

    private List<Piece> _allPieces = new();
    public List<Piece> AllPieces { get { return _allPieces; } }

    //private readonly string _defaultPosition = "6kr/3b2pp/Q1p2b2/3p4/1p3q2/1B5P/PP2RPP1/4R1K1";
    private readonly string _defaultPosition = "kr6/8/8/8/8/3K4/1p6/8";
    //private readonly string _defaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

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

            _currentMove.PromotionPieceType = e.PromotionPieceType;

            SelectPromotionPiece(_currentMove);
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

            // activate inactive (through capture) pieces
            currentPiece.gameObject.SetActive(true);

            // hide pieces created through promotion
            if (currentPiece.IsPromotedPiece)
            {
                currentPiece.gameObject.SetActive(false);
                continue;
            }

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

    public void UpdateAllPieceAnalysisMoves()
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            piece.CalculateAnalysisMoves(false);
        }
    }

    public void TakePiece(Piece piece)
    {
        piece.gameObject.SetActive(false);
    }

    public void ShowPromotionPieces(MoveDetails move)
    {
        Square promotionSquare = move.EndSquare;
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

    public void SetAutomaticPromotion(MoveDetails move, bool triggerMoveCompletedEvent)
    {
        _pawnToPromote = move.PieceToMove;

        SelectPromotionPiece(move, triggerMoveCompletedEvent);
    }

    public void SelectPromotionPiece(MoveDetails move, bool triggerMoveCompletedEvent = true)
    {
        if (GameManager.Instance.IsCurrentPlayerWhite) _promotionPiecesWhite.SetActive(false);
        else _promotionPiecesBlack.SetActive(false);

        Square promotionSquare = _pawnToPromote.Square;
        GameObject newPieceGO;
        TakePiece(_pawnToPromote);
        GameObject newPiecePrefab = move.isWhite ? _whiteQueen : _blackQueen;
        string newPieceCode = move.isWhite ? "Q" : "q";
        string newPGNCode;

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
            newPGNCode = "=" + newPieceCode;
            move.PGNCode = move.PGNCode.Replace(newPGNCode, " ").Trim() + "=" + newPieceCode;
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

        PieceMoved(move, triggerMoveCompletedEvent);
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
                if (move.EndSquare == square)
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
                if (move.EndSquare == square)
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

    public Piece GetPieceByRankAndFile(int rank, string file, bool isWhite, PIECE_TYPE pieceType)
    {
        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.PieceType != pieceType) continue;

            if (int.Parse(piece.Square.SquarePGNCode.Substring(1, 1)) == rank && piece.Square.SquarePGNCode[..1] == file)
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
    public void MovePiece(MoveDetails move, bool triggerMoveCompletedEvent = true, bool animate = true)
    {
        if (move.MoveNumber == -1)
            move.MoveNumber = PGNManager.Instance.GetNextMoveNumber();

        _currentMove = move;

        // if there is a piece on the square, capture it
        if (move.EndSquare.PieceOnSquare != null)
        {
            TakePiece(move.EndSquare.PieceOnSquare);
        }
        else if (move.RemovePieceEnPassant != null)
        {
            move.RemovePieceEnPassant.Square.SetPieceOnSquare(null);
            TakePiece(move.RemovePieceEnPassant);
        }

        // remove the piece from the current square
        move.PieceToMove.Square.SetPieceOnSquare(null);
        // set the pieces position to the new squares position
        if (animate)
            move.PieceToMove.AnimateToPosition(move.EndSquare.transform.position);
        else
            move.PieceToMove.transform.position = move.EndSquare.transform.position;

        move.PieceToMove.ShowHideAvailableMoves(false);
        // set the new square of the piece
        move.PieceToMove.SetPieceSquare(move.EndSquare);
        _selectedPiece = null;
        // set the piece as the new piece on the square
        move.EndSquare.SetPieceOnSquare(move.PieceToMove);

        if (move.ActivatesEnPassant)
            move.PieceToMove.SetPossibleEnPassant(true);

        move.PieceToMove.SetIsFirstMove(false);

        // if we are castling, we need to move the rook as well
        if (move.SecondPieceToMove != null && move.SecondEndSquare != null)
        {
            // remove the piece from the current square
            move.SecondPieceToMove.Square.SetPieceOnSquare(null);
            // set the pieces position to the new squares position
            move.SecondPieceToMove.transform.position = move.SecondEndSquare.transform.position;
            move.SecondPieceToMove.ShowHideAvailableMoves(false);
            // set the new square of the piece
            move.SecondPieceToMove.SetPieceSquare(move.SecondEndSquare);
            // set the piece as the new piece on the square
            move.SecondEndSquare.SetPieceOnSquare(move.SecondPieceToMove);

            move.SecondPieceToMove.SetIsFirstMove(false);
        }


        //_currentSquare = null;
        _isMovingPiece = false;

        // if we didn't en passant this move, set any available en passantable pieces back to not en passantable
        SetPiecesAsNotEnPassantable(!GameManager.Instance.IsCurrentPlayerWhite);

        // this should only be used moving through the PGN moves
        if (move.PromotionPieceType != PIECE_TYPE.None)
        {
            SetAutomaticPromotion(move, false);
        }
        else if (move.IsPromotion && move.PromotionPieceType == PIECE_TYPE.None)
            ShowPromotionPieces(move);
        else
        {
            UpdateAllPieceMoves();
            PieceMoved(move, triggerMoveCompletedEvent);
        }
    }

    public void PieceMoved(MoveDetails move, bool triggerMoveCompletedEvent = true)
    {
        Piece attackedKing;

        // reset the highlighted moves on the board
        BoardManager.Instance.ResetSquareColours();
        // highlight the current move
        BoardManager.Instance.HighlightCurrentMove(move);

        if (triggerMoveCompletedEvent)
            OnMoveCompleted?.Invoke(this, new()
            {
                Move = move
            });

        if (move.MoveNumber != -1 && move.PromotedPiece != null)
        {
            PGNManager.Instance.UpdatePromotedPieceGO(move.MoveNumber, move.PromotedPiece);
            PGNManager.Instance.UpdatePGNString(move.MoveNumber, move.PGNCode);
        }


        // check for check
        if (CheckIfAnyPieceCanTakeKing(GameManager.Instance.IsCurrentPlayerWhite))
        {
            attackedKing = GetPieceFromCharacter("K", !GameManager.Instance.IsCurrentPlayerWhite);

            if (CheckForMate())
            {
                // check mate baby
                Debug.Log("CheckMate");
                PGNManager.Instance.UpdatePGNString(move.MoveNumber, move.PGNCode.Replace('#', ' ').Trim() + "#");
                attackedKing.Square.SetSquareColour(_checkmateSquareColour, _checkmateOutlineColour);
                GameManager.Instance.UpdateGameState(GameState.GameOver);
            }
            else
            {
                //Debug.Log("Check");
                PGNManager.Instance.UpdatePGNString(move.MoveNumber, move.PGNCode.Replace('+', ' ').Trim() + "+");
                attackedKing.Square.SetSquareOutlineColour(_checkmateOutlineColour);
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

    public void FlipPieces()
    {
        foreach (Piece piece in _allPieces)
        {
            piece.transform.Rotate(0, 0, 180f);
        }
    }

    public void DrawArrowsToSquareFromAnalysisMoves(Square square)
    {
        foreach (Piece piece in _allPieces)
        {
            foreach (AnalysisMoveDetails move in piece.AnalysisMoves)
            {
                // don't show if we don't want x-ray moves
                if (!ToggleManager.Instance.ShowXRayMoves && move.IsXRayMove) continue;
                // don't show standard moves as they can't control the square
                if (move.AnalysisMoveType == ANALYSIS_MOVE_TYPE.Standard) continue;

                if (move.EndSquare == square)
                    ArrowManager.Instance.DrawArrow(piece.Square, square, move.AnalysisMoveType, true);
            }
        }
    }

    public int DrawArrowsForAllPossibleChecks(bool isWhite)
    {
        int possibleCheckCount = 0;
        ANALYSIS_MOVE_TYPE moveType;
        GameObject[] whitePromotedPiecePrefabs = { _whiteQueen, _whiteRook, _whiteBishop, _whiteKnight };
        GameObject[] blackPromotedPiecePrefabs = { _blackQueen, _blackRook, _blackBishop, _blackKnight };
        GameObject[] promotedPiecePrefabs = isWhite ? whitePromotedPiecePrefabs : blackPromotedPiecePrefabs;
        GameObject promotedPieceGO;
        Piece promotedPiece;
        bool canPromotedPieceCheck;

        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.AvailableMoves.Count == 0) continue;

            // Create a copy of the moves to avoid modification during enumeration
            List<MoveDetails> movesToCheck = piece.AvailableMoves.ToList();

            foreach (MoveDetails move in movesToCheck)
            {
                Piece capturedPiece = move.EndSquare.PieceOnSquare;
                Square startSquare = move.StartSquare;
                Square secondStartSquare = move.SecondPieceToMove == null ? null : move.SecondPieceToMove.Square;
                Square endSquare = move.EndSquare;
                Square secondEndSquare = move.SecondEndSquare;
                moveType = move.EndSquare.PieceOnSquare == null ? ANALYSIS_MOVE_TYPE.Standard : ANALYSIS_MOVE_TYPE.Capture;
                Piece pieceToMove = piece;
                Piece secondPieceToMove = move.SecondPieceToMove;

                if (move.RemovePieceEnPassant != null)
                {
                    // en passant can also check
                    capturedPiece = move.RemovePieceEnPassant;
                }
                // check if castling
                if (move.SecondPieceToMove != null)
                {
                    // if we are castling, move both pieces
                    // start with the king
                    SimulateMove(startSquare, endSquare, pieceToMove, false);
                    SimulateMove(secondStartSquare, secondEndSquare, secondPieceToMove, false);
                    pieceToMove.CalculateAvailableMoves(true);
                    move.SecondPieceToMove.CalculateAvailableMoves(true);

                    // Check for possible check, only need to check the rook though as the king can't check a king
                    if (move.SecondPieceToMove.CheckIfPieceCanTakeKing())
                    {
                        possibleCheckCount++;
                        // draw an arrow from the kings start square to end square
                        ArrowManager.Instance.DrawArrow(startSquare, endSquare, moveType, true);
                    }

                    // Restore state
                    RestoreMove(startSquare, endSquare, pieceToMove, null, false);
                    RestoreMove(secondStartSquare, secondEndSquare, secondPieceToMove, null, false);
                    pieceToMove.CalculateAvailableMoves(true);
                    secondPieceToMove.CalculateAvailableMoves(true);
                }
                else if (move.IsPromotion)
                {
                    // if we can promote, we have to check all the possible pieces we can promote to, and see if any of them can check the king
                    // start by deactivating the pawn
                    pieceToMove.gameObject.SetActive(false);

                    canPromotedPieceCheck = false;
                    List<PIECE_TYPE> promotedPieceTypes = new();

                    for (int i = 0; i < promotedPiecePrefabs.Length; i++)
                    {
                        // we have to create a piece on the endSquare, then calculate the moves, remove it and move on to the next piece
                        promotedPieceGO = Instantiate(promotedPiecePrefabs[i], endSquare.transform.position, Quaternion.identity);
                        promotedPiece = promotedPieceGO.GetComponent<Piece>();
                        promotedPiece.SetupPiece("Q", endSquare, move.isWhite, true);
                        promotedPiece.CalculateAvailableMoves(true);

                        if (promotedPiece.CheckIfPieceCanTakeKing())
                        {
                            canPromotedPieceCheck = true;
                            promotedPieceTypes.Add(promotedPiece.PieceType);
                        }

                        // now we have checked, destroy the new piece
                        Destroy(promotedPieceGO);
                    }

                    // end by reactivating the pawn
                    pieceToMove.gameObject.SetActive(true);

                    if (canPromotedPieceCheck)
                    {
                        possibleCheckCount++;
                        ArrowManager.Instance.DrawArrow(startSquare, endSquare, ANALYSIS_MOVE_TYPE.Standard, true, promotedPieceTypes.ToArray(), GameManager.Instance.IsCurrentPlayerWhite);
                    }
                }
                else
                {
                    // Simulate move
                    SimulateMove(startSquare, endSquare, pieceToMove);

                    // Check for possible check
                    if (pieceToMove.CheckIfPieceCanTakeKing())
                    {
                        possibleCheckCount++;
                        ArrowManager.Instance.DrawArrow(startSquare, endSquare, moveType, true);
                    }

                    // Restore state
                    RestoreMove(startSquare, endSquare, pieceToMove, capturedPiece);
                }
            }
        }

        return possibleCheckCount;
    }

    private void SimulateMove(Square startSquare, Square endSquare, Piece pieceToMove, bool updateMoves = true)
    {
        pieceToMove.SetPieceSquare(endSquare);
        endSquare.SetPieceOnSquare(pieceToMove);
        startSquare.SetPieceOnSquare(null);
        if (updateMoves)
            pieceToMove.CalculateAvailableMoves(true);
    }

    private void RestoreMove(Square startSquare, Square endSquare, Piece pieceToMove, Piece capturedPiece, bool updateMoves = true)
    {
        pieceToMove.SetPieceSquare(startSquare);
        endSquare.SetPieceOnSquare(capturedPiece);
        startSquare.SetPieceOnSquare(pieceToMove);
        if (updateMoves)
            pieceToMove.CalculateAvailableMoves(true);
    }

    public int DrawArrowsForAllPossibleCaptures(bool isWhite)
    {
        int possibleCaptureCount = 0;

        foreach (Piece piece in _allPieces)
        {
            if (!piece.gameObject.activeInHierarchy) continue;
            if (piece.IsWhite != isWhite) continue;
            if (piece.AvailableMoves.Count == 0) continue;

            foreach (AnalysisMoveDetails move in piece.AnalysisMoves)
            {
                if (move.AnalysisMoveType == ANALYSIS_MOVE_TYPE.Capture)
                {
                    ArrowManager.Instance.DrawArrow(move.StartSquare, move.EndSquare, ANALYSIS_MOVE_TYPE.Capture, true);
                    possibleCaptureCount++;
                }
            }
        }

        return possibleCaptureCount;
    }

    public void PrintMove(MoveDetails move)
    {
        Debug.Log($"Move Number: {move.MoveNumber}");
        Debug.Log($"Is White: {move.isWhite}");
        Debug.Log($"Piece To Move: {move.PieceToMove.name}");
        Debug.Log($"Starting Square: {move.StartSquare.name}");
        Debug.Log($"Move To Square: {move.EndSquare.name}");
        if (move.SecondPieceToMove != null)
            Debug.Log($"Second Piece To Move: {move.SecondPieceToMove.name}");
        if (move.SecondEndSquare != null)
            Debug.Log($"Second Move To Square: {move.SecondEndSquare.name}");
        Debug.Log($"Is Promotion: {move.IsPromotion}");
        Debug.Log($"Activates En Passant: {move.ActivatesEnPassant}");
        if (move.RemovePieceEnPassant != null)
            Debug.Log($"Remove Piece En Passant: {move.RemovePieceEnPassant.name}");
        Debug.Log($"Promotion Piece Type: {move.PromotionPieceType}");
        if (move.PromotedPiece != null)
            Debug.Log($"Promoted Piece: {move.PromotedPiece.name}");
        Debug.Log($"PGN Code: {move.PGNCode}");
    }
}
