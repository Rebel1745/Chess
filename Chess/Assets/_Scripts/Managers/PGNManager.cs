using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class PGNManager : MonoBehaviour
{
    public static PGNManager Instance { get; private set; }
    public string[] _moveList;
    public List<MoveDetails> _moveDetailsList = new();
    private int _currentMove = 0;
    [SerializeField] private MoveList _moveListDisplay;

    // draw rule checks
    private Dictionary<string, int> _fenMoveCount = new();
    private int _fiftyMoveDrawCount = 0;

    // play stuff
    private bool _isPlaying = false;
    private float _timeBetweenMoves = 1.0f;
    private float _nextMoveTime;

    public event EventHandler<OnMoveDetailsChangedArgs> OnMoveDetailsChanged;
    public class OnMoveDetailsChangedArgs : EventArgs
    {
        public List<MoveDetails> MoveDetailsList;
    }

    public event EventHandler<OnMoveNumberChangedArgs> OnMoveNumberChanged;
    public class OnMoveNumberChangedArgs : EventArgs
    {
        public int MoveNumber;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        PieceManager.Instance.OnMoveCompleted += PieceManager_OnMoveCompleted;
        _moveListDisplay.OnMoveListClicked += MoveList_OnMoveListClicked;
    }

    private void Update()
    {
        if (!_isPlaying) return;

        if (Time.time >= _nextMoveTime)
        {
            _nextMoveTime = Time.time + _timeBetweenMoves;
            NextMove(true);
        }
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        _moveDetailsList.Clear();
        _currentMove = 0;
        OnMoveDetailsChanged?.Invoke(this, new OnMoveDetailsChangedArgs
        {
            MoveDetailsList = _moveDetailsList
        });
    }

    private void PieceManager_OnMoveCompleted(object sender, PieceManager.OnMoveCompletedArgs e)
    {
        AddMove(e.Move);

        AddPositionToDictionary();

        UpdateFiftyMoveRule(e.Move);
    }

    private void MoveList_OnMoveListClicked(object sender, MoveList.OnMoveListClickedArgs e)
    {
        GoToMove(e.Move.MoveNumber);
    }

    public void ParsePGN(string pgn)
    {
        List<string> moveListStrings = new();
        _moveDetailsList = new List<MoveDetails>();

        string cleanData = pgn;

        // Find the split point (last ']' character)
        int splitPoint = cleanData.LastIndexOf(']') + 1;

        // Extract the move list
        string moveList = cleanData[splitPoint..];

        // Remove line breaks
        moveList = moveList.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");

        // remove annotations ($2, $6 etc)
        string pattern = @"\s*\$[0-9]+";
        moveList = Regex.Replace(moveList, pattern, "");

        // remove alternative lines e.g. (8... Ng4 9. hxg4 Bxe5 10. Nxe5 Nxe5 11. dxe5)
        pattern = @"\([^\)]+\)";
        moveList = Regex.Replace(moveList, pattern, "");

        // remove comments e.g. { That was an incredible move Darren }
        pattern = @"\{[^\}]+\}";
        moveList = Regex.Replace(moveList, pattern, "");

        pattern = @"([0-9]+\.\s*)([\w\-]+)(\+|\#)?(?:\$[0-9]+)?\s*([\w\-]+)(\+|\#)?";
        MatchCollection matches = Regex.Matches(moveList, pattern);

        foreach (Match match in matches)
        {
            moveListStrings.Add(match.Groups[2].Value);
            moveListStrings.Add(match.Groups[4].Value);
        }

        //string[] moves = moveList.Split(' ');
        string[] moves = moveListStrings.ToArray();
        bool isWhite = false;
        Square moveSquare, secondMoveSquare;
        PIECE_TYPE pieceType = PIECE_TYPE.None, promotionPieceType = PIECE_TYPE.None;
        Piece movePiece, secondMovePiece;
        string pieceCode;
        string file;
        string squareCode;
        int rank;
        int moveNumber = 0;
        string pieceColour;
        bool isEnPassantable;

        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = moves[i].Replace('+', ' ').Replace('#', ' ').Replace("  ", " ").Trim();
        }

        _moveList = new string[moves.Length];
        _moveList = moves;

        for (int i = 0; i < moves.Length; i++)
        {
            isWhite = !isWhite;
            isEnPassantable = false;
            pieceType = PIECE_TYPE.None;
            promotionPieceType = PIECE_TYPE.None;
            moveSquare = null;
            secondMoveSquare = null;
            movePiece = null;
            secondMovePiece = null;

            if (isWhite) moveNumber++;

            pieceColour = isWhite ? "White" : "Black";

            moves[i] = moves[i].Trim();

            if (moves[i] == "") continue;

            if (moves[i].IndexOf('.') != -1)
                moves[i] = moves[i].Substring(moves[i].IndexOf('.') + 1, moves[i].Length - moves[i].IndexOf('.') - 1);

            pieceCode = moves[i][..1].Trim();

            if (moves[i].Length == 0 || moves[i] == "") continue;
            if (moves[i].Length == 1)
            {
                // maybe a checkmate symbol, or a * showing the game is not over
                Debug.LogWarning($"{moves[i]} could not be parsed");
                continue;
            }

            // basic pawn move
            else if (moves[i].Length == 2)
            {
                pieceType = PIECE_TYPE.Pawn;
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i]);

                if (moveSquare == null) Debug.LogError($"{i} - {moves[i]} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{i} - {moves[i]} piece not found");

                if (Mathf.Abs(int.Parse(moveSquare.SquarePGNCode.Substring(1, 1)) - int.Parse(movePiece.Square.SquarePGNCode.Substring(1, 1))) == 2)
                    isEnPassantable = true;

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(moves[i], isWhite, movePiece, moveSquare, null, null, PIECE_TYPE.None, isEnPassantable);
                continue;
            }

            // castling king side
            if (moves[i].Length == 3 && pieceCode.ToUpper() == "O")
            {
                // first get the king
                movePiece = PieceManager.Instance.GetPieceFromCharacter("K", isWhite);

                if (movePiece == null) Debug.LogError($"{i} - {moves[i]} piece not found");

                // get the square to castle to - G1 or G8
                squareCode = isWhite ? "G1" : "G8";
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (moveSquare == null) Debug.LogError($"{i} - {squareCode} square not found");

                // get the king side rook - on H1 or H8
                squareCode = isWhite ? "H1" : "H8";
                secondMovePiece = BoardManager.Instance.GetSquareFromPGNCode(squareCode).PieceOnSquare;

                if (secondMovePiece == null) Debug.LogError($"{i} - {moves[i]} (second move) piece not found");

                // get the square to move the rook to - F1 or F8
                squareCode = isWhite ? "F1" : "F8";
                secondMoveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (secondMoveSquare == null) Debug.LogError($"{i} - {moves[i]} (second move) square not found");

                //Debug.Log($"{pieceColour} castle king side");

                AddMove(moves[i], isWhite, movePiece, moveSquare, secondMovePiece, secondMoveSquare);
                continue;
            }

            // castling queen side
            if (moves[i].Length == 4 && pieceCode.ToUpper() == "O")
            {
                // castling queen side, first get the king
                movePiece = PieceManager.Instance.GetPieceFromCharacter("K", isWhite);

                if (movePiece == null) Debug.LogError($"{i} - {moves[i]} piece not found");

                // get the square to castle to - C1 or C8
                squareCode = isWhite ? "C1" : "C8";
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (moveSquare == null) Debug.LogError($"{i} - {moves[i]} square not found");

                // get the king side rook - on A1 or A8
                squareCode = isWhite ? "A1" : "A8";
                secondMovePiece = BoardManager.Instance.GetSquareFromPGNCode(squareCode).PieceOnSquare;

                if (secondMovePiece == null) Debug.LogError($"{i} - {moves[i]} (second move) piece not found");

                // get the square to move the rook to - D1 or D8
                squareCode = isWhite ? "D1" : "D8";
                secondMoveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (secondMoveSquare == null) Debug.LogError($"{i} - {moves[i]} (second move) square not found");

                //Debug.Log($"{pieceColour} castle queen side");

                AddMove(moves[i], isWhite, movePiece, moveSquare, secondMovePiece, secondMoveSquare);
                continue;
            }

            // check for a capture
            if (moves[i].ToUpper().IndexOf("X") != -1)
            {
                // first check if the character is lower case, if it is then it is a pawn
                if (pieceCode == pieceCode.ToLower())
                    pieceType = PIECE_TYPE.Pawn;
                else
                    // there is a capture here
                    pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);

                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(moves[i].Length - 2, 2));

                if (moveSquare == null) Debug.LogError($"{i} - {moves[i].Substring(2, 2)} square not found");

                // check to see if this capture is en passant
                if (moveSquare.PieceOnSquare == null)
                {
                    // as this is a capture, there will always be a piece on the target square
                    // that is, of course, unless we are en passant-ing
                    rank = int.Parse(moves[i].Substring(3, 1));
                    rank = isWhite ? rank - 1 : rank + 1;
                    secondMoveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 1) + rank.ToString());
                    if (secondMoveSquare == null) Debug.LogError($"{i} - {moves[i].Substring(2, 1) + rank.ToString()} square not found");
                    if (secondMoveSquare.PieceOnSquare == null) Debug.LogError($"{i} - Piece on {moves[i].Substring(2, 1) + rank.ToString()} not found");
                    secondMovePiece = secondMoveSquare.PieceOnSquare;
                }

                if (pieceType == PIECE_TYPE.None)
                {
                    pieceType = PIECE_TYPE.Pawn;
                    rank = int.Parse(moves[i].Substring(3, 1));
                    rank = isWhite ? rank - 1 : rank + 1;
                    squareCode = moves[i][..1] + rank;
                    // there is no piece type, that means the letter is a-h and refers to a pawn on that file capturing on the square code
                    movePiece = BoardManager.Instance.GetSquareFromPGNCode(squareCode).PieceOnSquare;

                    if (movePiece == null) Debug.LogError($"{i} - {pieceColour} Pawn on {moves[i][..1].ToUpper()} take on {squareCode} piece not found");
                }
                else
                {
                    // check to see if there is a character to clarify the piece taking
                    string[] pieceAndSquareStrings = moves[i].ToUpper().Split("X");
                    string rankOrFile = moves[i].Substring(1, 1);

                    // if there is only one character to identify the piece, we can find it by using the piece typ
                    if (pieceAndSquareStrings[0].Length == 1)
                        movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);
                    else if (pieceAndSquareStrings[0].Length == 2)
                    {
                        // if there are two characters to identify the piece it could be either the rank or the file
                        // if it is a letter it is the file, if it is a number, it is the rank
                        if (int.TryParse(rankOrFile, out rank))
                            movePiece = PieceManager.Instance.GetPieceByRank(rank, isWhite, pieceType);
                        else
                            movePiece = PieceManager.Instance.GetPieceByFile(rankOrFile, isWhite, pieceType);
                    }
                    else if (pieceAndSquareStrings[0].Length == 3)
                    {
                        // if we are here then it is both the rank and the file that denotes which piece is doing the capture
                        movePiece = PieceManager.Instance.GetPieceByRankAndFile(int.Parse(moves[i].Substring(1, 1)), moves[i].Substring(2, 1), isWhite, pieceType);
                    }

                    if (movePiece == null) Debug.LogError($"{i} - {pieceColour} {pieceType} that takes on {moveSquare.SquarePGNCode} not found");
                    //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} take on {moveSquare.SquarePGNCode} ");
                }

                if (moves[i].IndexOf("=") != -1)
                {
                    // there is also a promotion
                    promotionPieceType = PieceManager.Instance.GetPieceTypeFromCharacter(moves[i].Substring(moves[i].Length - 1, 1));
                }

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} takes on {moveSquare.SquarePGNCode}");

                AddMove(moves[i], isWhite, movePiece, moveSquare, null, null, promotionPieceType, false, secondMovePiece);
                continue;
            }

            // check for promotion
            if (moves[i].IndexOf("=") != -1)
            {
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(moves[i].Substring(moves[i].Length - 1, 1));
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i][..2]);

                if (moveSquare == null) Debug.LogError($"{i} - {moves[i][..2]} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, PIECE_TYPE.Pawn);

                if (movePiece == null) Debug.LogError($"{i} - {pieceColour} Pawn to promote on {moveSquare} piece not found");

                AddMove(moves[i], isWhite, movePiece, moveSquare, null, null, pieceType);
                continue;
            }

            if (moves[i].Length == 3)
            {
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(1, 2));

                if (moveSquare == null) Debug.LogError($"{i} - {moves[i].Substring(1, 2)} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{i} - {moves[i][..1]} {pieceType} {moves[i].Substring(1, 2)} piece not found");

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(moves[i], isWhite, movePiece, moveSquare);
                continue;
            }

            if (moves[i].Length == 4)
            {
                file = moves[i].Substring(1, 1);
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 2));

                if (moveSquare == null) Debug.LogError($"{i} - {moves[i].Substring(1, 2)} square not found");

                if (int.TryParse(file, out rank))
                    movePiece = PieceManager.Instance.GetPieceByRank(rank, isWhite, pieceType);
                else
                    movePiece = PieceManager.Instance.GetPieceByFile(file, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{i} - {moves[i][..1]} {pieceType} {moves[i].Substring(1, 2)} piece not found");

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(moves[i], isWhite, movePiece, moveSquare);
                continue;
            }
        }

        TriggerOnMoveListUpdatedEvent();
        UIManager.Instance.SetTabMenuTab(1);
        FirstMove();
    }

    private void AddMove(string pgnString, bool isWhite, Piece pieceToMove, Square squareToMoveTo, Piece secondPieceToMove = null, Square secondSquareToMoveTo = null, PIECE_TYPE pieceToPromoteTo = PIECE_TYPE.None, bool isEnPassantable = false, Piece pieceToTakeEnPassant = null)
    {
        MoveDetails move;

        if (pieceToMove != null && squareToMoveTo != null)
        {
            move = new MoveDetails
            {
                MoveNumber = _moveDetailsList.Count,
                isWhite = isWhite,
                PieceToMove = pieceToMove,
                StartSquare = pieceToMove.Square,
                EndSquare = squareToMoveTo,
                SecondPieceToMove = secondPieceToMove,
                SecondEndSquare = secondSquareToMoveTo,
                PromotionPieceType = pieceToPromoteTo,
                ActivatesEnPassant = isEnPassantable,
                RemovePieceEnPassant = pieceToTakeEnPassant,
                PGNCode = pgnString
            };

            AddMove(move, false);

            PieceManager.Instance.MovePiece(move, false, false, false);
        }
    }

    public void AddMove(MoveDetails move, bool triggerListUpdatedEvent = true)
    {
        move.MoveNumber = _currentMove;
        _currentMove++;
        _moveDetailsList.Add(move);

        if (triggerListUpdatedEvent)
            TriggerOnMoveListUpdatedEvent();
    }

    private void TriggerOnMoveListUpdatedEvent()
    {
        OnMoveDetailsChanged?.Invoke(this, new OnMoveDetailsChangedArgs
        {
            MoveDetailsList = _moveDetailsList
        });
    }

    public void UpdatePromotedPieceGO(int moveNumber, GameObject piece)
    {
        if (moveNumber == -1) return;

        MoveDetails tmp = _moveDetailsList[moveNumber];
        tmp.PromotedPiece = piece;

        _moveDetailsList[moveNumber] = tmp;
    }

    public void UpdatePGNString(int moveNumber, string pgnString)
    {
        if (moveNumber == -1) return;

        MoveDetails tmp = _moveDetailsList[moveNumber];
        tmp.PGNCode = pgnString;

        _moveDetailsList[moveNumber] = tmp;

        TriggerOnMoveListUpdatedEvent();
    }

    public void FirstMove()
    {
        PauseGame();
        GameManager.Instance.SetCurrentPlayerColour(true);
        _currentMove = 0;
        PieceManager.Instance.ResetBoardPosition();
        BoardManager.Instance.ResetSquareColours();
        PieceManager.Instance.ResetCapturedPieces();
        UIManager.Instance.ResetPieceIcons();

        OnMoveNumberChanged?.Invoke(this, new OnMoveNumberChangedArgs
        {
            MoveNumber = -1
        });
    }

    public void LastMove()
    {
        FirstMove();
        for (int i = 0; i < _moveDetailsList.Count; i++)
        {
            NextMove(false);
        }
    }

    public void NextMove(bool animate = true)
    {
        if (_currentMove == _moveDetailsList.Count)
        {
            PauseGame();
            return;
        }

        OnMoveNumberChanged?.Invoke(this, new OnMoveNumberChangedArgs
        {
            MoveNumber = _currentMove
        });

        MoveDetails move = _moveDetailsList[_currentMove];
        PieceManager.Instance.MovePiece(move, false, animate, animate); // use the animate flag for the play audio option as they will be the same

        _currentMove++;
    }

    public void PreviousMove()
    {
        if (_isPlaying) PauseGame();

        if (_currentMove == 0) return;

        int moveTo = _currentMove - 1;

        FirstMove();

        for (int i = 0; i < moveTo; i++)
        {
            NextMove(false);
        }
    }

    private void GoToMove(int moveNumber)
    {
        if (_isPlaying) PauseGame();
        FirstMove();
        for (int i = 0; i <= moveNumber; i++)
        {
            NextMove(false);
        }
    }

    public int GetNextMoveNumber()
    {
        return _moveDetailsList.Count;
    }

    public void PlayGame()
    {
        UIManager.Instance.ShowHidePlayButton(false);
        UIManager.Instance.ShowHidePauseButton(true);

        _isPlaying = true;
        _nextMoveTime = Time.time + _timeBetweenMoves;
    }

    public void PauseGame()
    {
        UIManager.Instance.ShowHidePlayButton(true);
        UIManager.Instance.ShowHidePauseButton(false);

        _isPlaying = false;
    }

    private void AddPositionToDictionary()
    {
        string fenString = BoardManager.Instance.GenerateBoardPositionFEN();

        if (_fenMoveCount.ContainsKey(fenString))
            _fenMoveCount[fenString] = _fenMoveCount[fenString] + 1;
        else
            _fenMoveCount.Add(fenString, 1);
    }

    public bool CheckForRepetition()
    {
        foreach (KeyValuePair<string, int> kvp in _fenMoveCount)
        {
            if (kvp.Value >= 3) return true;
        }

        return false;
    }

    private void UpdateFiftyMoveRule(MoveDetails move)
    {
        // if a pawn has moved, we can reset the count and bail
        if (move.PieceToMove.PieceType == PIECE_TYPE.Pawn)
        {
            _fiftyMoveDrawCount = 0;
            return;
        }

        // if there is a capture, we can reset the count and bail
        if (move.PGNCode.ToLower().Contains('x'))
        {
            _fiftyMoveDrawCount = 0;
            return;
        }

        _fiftyMoveDrawCount++;
    }

    public bool CheckForFiftyMoveRule()
    {
        return _fiftyMoveDrawCount >= 50;
    }
}
