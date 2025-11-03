using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
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
        string[] fieldStrings = pgn.Split(']');
        string moveList = fieldStrings[^1].Trim();
        string[] moves = moveList.Split(' ');
        bool isWhite = false;
        Square moveSquare = null, secondMoveSquare = null;
        PIECE_TYPE pieceType = PIECE_TYPE.None;
        Piece movePiece = null, secondMovePiece = null;
        string pieceCode;
        string file;
        string squareCode;
        int rank;
        int moveNumber = 0;
        string pieceColour;

        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = moves[i].Replace('+', ' ').Trim();
        }

        _moveList = new string[moves.Length];
        _moveList = moves;

        for (int i = 0; i < moves.Length; i++)
        {
            isWhite = !isWhite;
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

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                AddMove(movePiece, moveSquare);
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

                AddMove(movePiece, moveSquare, secondMovePiece, secondMoveSquare);
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

                AddMove(movePiece, moveSquare, secondMovePiece, secondMoveSquare);
                continue;
            }

            // check for a capture
            if (moves[i].ToUpper().IndexOf("X") != -1)
            {
                // there is a capture here
                pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 2));

                if (moveSquare == null) Debug.LogError($"{moves[i].Substring(2, 2)} square not found");

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

                //Debug.Log($"{pieceColour} {pieceType} on {movePiece.Square.SquarePGNCode} takes on {moveSquare.SquarePGNCode}");

                AddMove(movePiece, moveSquare);
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

                AddMove(movePiece, moveSquare, null, null, pieceType);
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

                AddMove(movePiece, moveSquare);
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

                AddMove(movePiece, moveSquare);
                continue;
            }
        }
    }

    private void AddMove(Piece pieceToMove, Square squareToMoveTo, Piece secondPieceToMove = null, Square secondSquareToMoveTo = null, PIECE_TYPE pieceToPromoteTo = PIECE_TYPE.None)
    {
        MoveDetails move;

        if (pieceToMove != null && squareToMoveTo != null)
        {
            move = new MoveDetails
            {
                PieceToMove = pieceToMove,
                MoveToSquare = squareToMoveTo,
                SecondPieceToMove = secondPieceToMove,
                SecondMoveToSquare = secondSquareToMoveTo,
                PromotionPieceType = pieceToPromoteTo
            };

            InputManager.Instance.MovePiece(move);

            _moveDetailsList.Add(move);
        }
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
        if (_currentMove == _moveDetailsList.Count - 1) return;

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
