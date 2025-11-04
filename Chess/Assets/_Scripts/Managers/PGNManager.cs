using System.Collections.Generic;
using UnityEngine;

public class PGNManager : MonoBehaviour
{
    public static PGNManager Instance { get; private set; }
    public string[] _moveList;
    public List<MoveDetails> _moveDetailsList = new();
    private int _currentMove = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ParsePGN(string pgn)
    {
        _moveDetailsList = new List<MoveDetails>();

        string[] fieldStrings = pgn.Split(']');
        string moveList = fieldStrings[^1].Trim();
        string[] moves = moveList.Split(' ');
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
            moves[i] = moves[i].Replace('+', ' ').Replace('#', ' ').Trim();
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
            if (moves[i].IndexOf('.') != -1)
                moves[i] = moves[i].Substring(moves[i].IndexOf('.') + 1, moves[i].Length - moves[i].IndexOf('.') - 1);

            pieceCode = moves[i][..1];

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

                if (moveSquare == null) Debug.LogError($"{moves[i]} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{moves[i]} piece not found");

                if (Mathf.Abs(int.Parse(moveSquare.SquarePGNCode.Substring(1, 1)) - int.Parse(movePiece.Square.SquarePGNCode.Substring(1, 1))) == 2)
                    isEnPassantable = true;

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(isWhite, movePiece, moveSquare, null, null, PIECE_TYPE.None, isEnPassantable);
                continue;
            }

            // castling king side
            if (moves[i].Length == 3 && pieceCode.ToUpper() == "O")
            {
                // first get the king
                movePiece = PieceManager.Instance.GetPieceFromCharacter("K", isWhite);

                if (movePiece == null) Debug.LogError($"{moves[i]} piece not found");

                // get the square to castle to - G1 or G8
                squareCode = isWhite ? "G1" : "G8";
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (moveSquare == null) Debug.LogError($"{squareCode} square not found");

                // get the king side rook - on H1 or H8
                squareCode = isWhite ? "H1" : "H8";
                secondMovePiece = BoardManager.Instance.GetSquareFromPGNCode(squareCode).PieceOnSquare;

                if (secondMovePiece == null) Debug.LogError($"{moves[i]} (second move) piece not found");

                // get the square to move the rook to - F1 or F8
                squareCode = isWhite ? "F1" : "F8";
                secondMoveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (secondMoveSquare == null) Debug.LogError($"{moves[i]} (second move) square not found");

                //Debug.Log($"{pieceColour} castle king side");

                AddMove(isWhite, movePiece, moveSquare, secondMovePiece, secondMoveSquare);
                continue;
            }

            // castling queen side
            if (moves[i].Length == 4 && pieceCode.ToUpper() == "O")
            {
                // castling queen side, first get the king
                movePiece = PieceManager.Instance.GetPieceFromCharacter("K", isWhite);

                if (movePiece == null) Debug.LogError($"{moves[i]} piece not found");

                // get the square to castle to - C1 or C8
                squareCode = isWhite ? "C1" : "C8";
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (moveSquare == null) Debug.LogError($"{moves[i]} square not found");

                // get the king side rook - on A1 or A8
                squareCode = isWhite ? "A1" : "A8";
                secondMovePiece = BoardManager.Instance.GetSquareFromPGNCode(squareCode).PieceOnSquare;

                if (secondMovePiece == null) Debug.LogError($"{moves[i]} (second move) piece not found");

                // get the square to move the rook to - D1 or D8
                squareCode = isWhite ? "D1" : "D8";
                secondMoveSquare = BoardManager.Instance.GetSquareFromPGNCode(squareCode);

                if (secondMoveSquare == null) Debug.LogError($"{moves[i]} (second move) square not found");

                //Debug.Log($"{pieceColour} castle queen side");

                AddMove(isWhite, movePiece, moveSquare, secondMovePiece, secondMoveSquare);
                continue;
            }

            // check for a capture
            if (moves[i].ToUpper().IndexOf("X") != -1)
            {
                // there is a capture here
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 2));

                if (moveSquare == null) Debug.LogError($"{moves[i].Substring(2, 2)} square not found");

                // check to see if this capture is en passant
                if (moveSquare.PieceOnSquare == null)
                {
                    // as this is a capture, there will always be a piece on the target square
                    // that is, of course, unless we are en passant-ing
                    rank = int.Parse(moves[i].Substring(3, 1));
                    rank = isWhite ? rank - 1 : rank + 1;
                    secondMoveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 1) + rank.ToString());
                    if (secondMoveSquare == null) Debug.LogError($"{moves[i].Substring(2, 1) + rank.ToString()} square not found");
                    if (secondMoveSquare.PieceOnSquare == null) Debug.LogError($"Piece on {moves[i].Substring(2, 1) + rank.ToString()} not found");
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

                    if (movePiece == null) Debug.LogError($"{pieceColour} Pawn on {moves[i][..1].ToUpper()} take on {squareCode} piece not found");
                }
                else
                {
                    movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);
                    if (movePiece == null) Debug.LogError($"{pieceColour} {pieceType} that takes on {moveSquare.SquarePGNCode} not found");
                    //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} take on {moveSquare.SquarePGNCode} ");
                }

                if (moves[i].IndexOf("=") != -1)
                {
                    // there is also a promotion
                    promotionPieceType = PieceManager.Instance.GetPieceTypeFromCharacter(moves[i].Substring(moves[i].Length - 1, 1));
                }

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} takes on {moveSquare.SquarePGNCode}");

                AddMove(isWhite, movePiece, moveSquare, null, null, promotionPieceType, false, secondMovePiece);
                continue;
            }

            // check for promotion
            if (moves[i].IndexOf("=") != -1)
            {
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(moves[i].Substring(moves[i].Length - 1, 1));
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i][..2]);

                if (moveSquare == null) Debug.LogError($"{moves[i][..2]} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, PIECE_TYPE.Pawn);

                if (movePiece == null) Debug.LogError($"{pieceColour} Pawn to promote on {moveSquare} piece not found");

                AddMove(isWhite, movePiece, moveSquare, null, null, pieceType);
                continue;
            }

            if (moves[i].Length == 3)
            {
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(1, 2));

                if (moveSquare == null) Debug.LogError($"{moves[i].Substring(1, 2)} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{moves[i][..1]} {pieceType} {moves[i].Substring(1, 2)} piece not found");

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(isWhite, movePiece, moveSquare);
                continue;
            }

            if (moves[i].Length == 4)
            {
                file = moves[i].Substring(1, 1);
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 2));

                if (moveSquare == null) Debug.LogError($"{moves[i].Substring(1, 2)} square not found");

                if (int.TryParse(file, out rank))
                    movePiece = PieceManager.Instance.GetPieceByRank(rank, isWhite, pieceType);
                else
                    movePiece = PieceManager.Instance.GetPieceByFile(file, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{moves[i][..1]} {pieceType} {moves[i].Substring(1, 2)} piece not found");

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(isWhite, movePiece, moveSquare);
                continue;
            }
        }
    }

    private void AddMove(bool isWhite, Piece pieceToMove, Square squareToMoveTo, Piece secondPieceToMove = null, Square secondSquareToMoveTo = null, PIECE_TYPE pieceToPromoteTo = PIECE_TYPE.None, bool isEnPassantable = false, Piece pieceToTakeEnPassant = null)
    {
        MoveDetails move;

        if (pieceToMove != null && squareToMoveTo != null)
        {
            move = new MoveDetails
            {
                MoveNumber = _moveDetailsList.Count,
                isWhite = isWhite,
                PieceToMove = pieceToMove,
                MoveToSquare = squareToMoveTo,
                SecondPieceToMove = secondPieceToMove,
                SecondMoveToSquare = secondSquareToMoveTo,
                PromotionPieceType = pieceToPromoteTo,
                ActivatesEnPassant = isEnPassantable,
                RemovePieceEnPassant = pieceToTakeEnPassant
            };

            _moveDetailsList.Add(move);

            InputManager.Instance.MovePiece(move);
        }
    }

    public void UpdatePromotedPieceGO(int moveNumber, GameObject piece)
    {
        if (moveNumber == -1) return;

        MoveDetails tmp = _moveDetailsList[moveNumber];
        tmp.PromotedPiece = piece;

        _moveDetailsList[moveNumber] = tmp;
    }

    public void FirstMove()
    {
        GameManager.Instance.SetCurrentPlayerColour(true);
        _currentMove = 0;
        PieceManager.Instance.ResetBoardPosition();
    }

    public void LastMove()
    {
        FirstMove();
        for (int i = 0; i < _moveDetailsList.Count; i++)
        {
            NextMove();
        }
    }

    public void NextMove()
    {
        if (_currentMove == _moveDetailsList.Count) return;

        MoveDetails move = _moveDetailsList[_currentMove];
        InputManager.Instance.MovePiece(move);
        _currentMove++;
    }

    public void PreviousMove()
    {
        if (_currentMove == 0) return;

        int moveTo = _currentMove - 1;

        FirstMove();

        for (int i = 0; i < moveTo; i++)
        {
            NextMove();
        }
    }
}
