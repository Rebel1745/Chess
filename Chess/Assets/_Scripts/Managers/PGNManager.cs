using UnityEngine;

public class PGNManager : MonoBehaviour
{
    public static PGNManager Instance { get; private set; }
    public string[] _moveList;

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
        Square moveSquare;
        PIECE_TYPE pieceType;
        Piece movePiece;
        MoveDetails move;

        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = moves[i].Trim();
            if (moves[i].IndexOf('.') != -1)
                moves[i] = moves[i].Substring(moves[i].IndexOf('.') + 1, moves[i].Length - moves[i].IndexOf('.') - 1);

            if (moves[i].Length == 2)
            {
                // pawn move
                pieceType = PIECE_TYPE.Pawn;
                moveSquare = BoardManager.Instance.GetSquareFromPGNCode(moves[i]);

                if (moveSquare == null) Debug.LogError($"{moves[i]} square not found");

                movePiece = PieceManager.Instance.GetPieceByMove(moveSquare, isWhite, pieceType);

                if (movePiece == null) Debug.LogError($"{moves[i]} piece not found");

                Debug.Log($"{isWhite} Pawn on {movePiece.Square.SquarePGNCode} move to {moveSquare.SquarePGNCode}");

                move = new MoveDetails
                {
                    PieceToMove = movePiece,
                    MoveToSquare = moveSquare
                };
            }

            isWhite = !isWhite;
        }

        _moveList = new string[moves.Length];
        _moveList = moves;
    }
}
