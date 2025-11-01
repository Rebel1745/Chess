using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class PGNManager : MonoBehaviour
{
    public static PGNManager Instance { get; private set; }
    public string[] _moveList;
    public List<MoveDetails> _moveDetailsList = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ParsePGN(string pgn)
    {
        string[] fieldStrings = pgn.Split(']');
        string moveList = fieldStrings[^1].Trim();
        string[] moves = moveList.Split(' ');
        bool isWhite = true;
        Square moveSquare = null, secondMoveSquare = null;
        PIECE_TYPE pieceType = PIECE_TYPE.None;
        Piece movePiece = null, secondMovePiece = null;
        MoveDetails move;
        string pieceCode;
        string file;
        string squareCode;
        int rank;
        int moveNumber = 0;

        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = moves[i].Trim();
        }

        _moveList = new string[moves.Length];
        _moveList = moves;

        for (int i = 0; i < moves.Length; i++)
        {
            if (isWhite) moveNumber++;
            Debug.Log($"Move: {moveNumber}");
            moves[i] = moves[i].Trim();
            if (moves[i].IndexOf('.') != -1)
                moves[i] = moves[i].Substring(moves[i].IndexOf('.') + 1, moves[i].Length - moves[i].IndexOf('.') - 1);

            if (moves[i].Length == 1)
            {
                continue;
            }
            else if (moves[i].Length == 2)
            {
                // pawn move
                pieceType = PIECE_TYPE.Pawn;
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i]);

                if (moveSquare == null) Debug.LogError($"{moves[i]} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{moves[i]} piece not found");

                Debug.Log($"{isWhite} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");
            }
            else if (moves[i].Length == 3)
            {
                // piece code is the first character
                pieceCode = moves[i][..1];
                if (pieceCode.ToUpper() == "O")
                {
                    // castling king side, first get the king
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

                    Debug.Log($"{isWhite} castle king side");
                }
                else
                {
                    pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                    moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(1, 2));

                    if (moveSquare == null) Debug.LogError($"{moves[i].Substring(1, 2)} square not found");

                    movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                    if (movePiece == null) Debug.LogError($"{moves[i][..1]} {pieceType} {moves[i].Substring(1, 2)} piece not found");

                    Debug.Log($"{isWhite} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");
                }
            }
            else if (moves[i].Length == 4)
            {
                if (moves[i].ToUpper().IndexOf("X") == -1)
                {
                    // we aren't capturing, that leaves castling queen side or confirming which piece is moving if the same piece type can move to the same square
                    pieceCode = moves[i][..1];
                    file = moves[i].Substring(1, 1);
                    if (pieceCode.ToUpper() == "O")
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

                        Debug.Log($"{isWhite} castle king side");
                    }
                    else
                    {
                        pieceType = PieceManager.Instance.GetPieceTypeFromCharacter(pieceCode);
                        moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i].Substring(2, 2));

                        if (moveSquare == null) Debug.LogError($"{moves[i].Substring(1, 2)} square not found");

                        movePiece = PieceManager.Instance.GetPieceByFile(file, isWhite, pieceType);

                        if (movePiece == null) Debug.LogError($"{moves[i][..1]} {pieceType} {moves[i].Substring(1, 2)} piece not found");

                        Debug.Log($"{isWhite} {pieceType} on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");
                    }
                }
                else
                {
                    // there is a capture here
                    pieceCode = moves[i][..1];
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

                        if (movePiece == null) Debug.LogError($"{isWhite} Pawn on {moves[i][..1].ToUpper()} take on {squareCode} piece not found");
                    }
                    else
                    {
                        Debug.Log(moveSquare.SquarePGNCode);
                        movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);
                        //Debug.Log($"{isWhite} {pieceType} on {movePiece.Square.SquarePGNCode} take on {moveSquare.SquarePGNCode} ");
                        if (movePiece == null) Debug.LogError($"{isWhite} {pieceType} on {movePiece.Square.SquarePGNCode} take on {moveSquare.SquarePGNCode} piece not found");
                    }

                    Debug.Log($"{isWhite} {pieceType} on {movePiece.Square.SquarePGNCode} takes on {moveSquare.SquarePGNCode}");
                }
            }

            if (movePiece != null && moveSquare != null)
            {
                move = new MoveDetails
                {
                    PieceToMove = movePiece,
                    MoveToSquare = moveSquare,
                    SecondPieceToMove = secondMovePiece,
                    SecondMoveToSquare = secondMoveSquare
                };

                InputManager.Instance.MovePiece(move);

                _moveDetailsList.Add(move);

                isWhite = !isWhite;
            }
        }
    }
}
