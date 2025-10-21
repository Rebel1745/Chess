using System;
using System.Linq;
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

    private string _defaultPosition = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";
    //private string _defaultPosition = "r1bk3r/p2pBpNp/n4n2/1p1NP2P/6P1/3P4/P1P1K3/q5b1";

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
                    piece = GetPieceGOFromText(currentChar);
                    Vector3 spawnPos = new(fileIndex, i, 0f);
                    Instantiate(piece, spawnPos, Quaternion.identity, _pieceHolder);
                    fileIndex++;
                }
            }
        }
    }

    public void LoadDefaultPosition()
    {
        LoadPosition(_defaultPosition);
    }

    private GameObject GetPieceGOFromText(string character)
    {
        GameObject piece = _whiteKing;

        switch (character)
        {
            case "K":
                piece = _whiteKing;
                break;
            case "Q":
                piece = _whiteQueen;
                break;
            case "R":
                piece = _whiteRook;
                break;
            case "B":
                piece = _whiteBishop;
                break;
            case "N":
                piece = _whiteKnight;
                break;
            case "P":
                piece = _whitePawn;
                break;
            case "k":
                piece = _blackKing;
                break;
            case "q":
                piece = _blackQueen;
                break;
            case "r":
                piece = _blackRook;
                break;
            case "b":
                piece = _blackBishop;
                break;
            case "n":
                piece = _blackKnight;
                break;
            case "p":
                piece = _blackPawn;
                break;
        }

        return piece;
    }
}
