using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;
using System.Text;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    [SerializeField] private string _exeFileName = "stockfish/stockfish-windows-x86-64-sse41-popcnt.exe"; // Specify the file name of the .exe file
    private Process _stockfishProcess;

    private StringBuilder _gameMoves = new();
    [SerializeField] private float _timeBetweenMoves = 1f;
    private float _nextMoveTime;
    private bool _moveReady = false;
    private string _bestMove;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        SetupStockfish();
    }

    private void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnWaitingForMove += GameManager_OnWaitingForMove;
        PieceManager.Instance.OnMoveCompleted += PieceManager_OnMoveCompleted;
    }

    private void Update()
    {
        if (!_moveReady) return;

        if (Time.time >= _nextMoveTime)
        {
            _moveReady = false;
            PieceManager.Instance.CreateMoveFromUCIString(_bestMove);
        }
    }

    private void GameManager_OnGameStarted(object sender, EventArgs e)
    {
        // Send "uci" command to Stockfish
        SendCommandToStockfish("uci");
    }

    private void GameManager_OnWaitingForMove(object sender, EventArgs e)
    {
        if (!GameManager.Instance.IsCurrentPlayerCPU) return;

        GetMoveFromStockfish();
    }

    private void PieceManager_OnMoveCompleted(object sender, PieceManager.OnMoveCompletedArgs e)
    {
        string uciMove = e.Move.StartSquare.SquarePGNCode + e.Move.EndSquare.SquarePGNCode;

        if (e.Move.IsPromotion)
            uciMove += e.Move.PGNCode.Substring(e.Move.PGNCode.Length - 1, 1).ToLower();

        if (_gameMoves.Length == 0) _gameMoves.Append(uciMove);
        else _gameMoves.Append(" " + uciMove);
    }

    private void OnDestroy()
    {
        if (_stockfishProcess != null && !_stockfishProcess.HasExited)
        {
            _stockfishProcess.StandardInput.WriteLine("quit");
            _stockfishProcess.StandardInput.Flush();
            _stockfishProcess.WaitForExit();
            _stockfishProcess.Close();
        }
    }

    public void SetupStockfish()
    {
        // Get the path to the executable relative to the Assets folder
        string exeFilePath = Path.Combine(Application.dataPath, _exeFileName);

        // Check if the executable file exists
        if (File.Exists(exeFilePath))
        {
            // Start the process with the executable file
            ProcessStartInfo startInfo = new()
            {
                FileName = exeFilePath,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true
            };

            _stockfishProcess = new Process
            {
                StartInfo = startInfo
            };
            _stockfishProcess.Start();
        }
        else
        {
            UnityEngine.Debug.LogError("Stockfish executable file not found: " + exeFilePath);
        }
    }

    private void SendCommandToStockfish(string command)
    {
        if (_stockfishProcess != null && !_stockfishProcess.HasExited)
        {
            _stockfishProcess.StandardInput.WriteLine(command);
            _stockfishProcess.StandardInput.Flush();
        }
    }

    private void GetMoveFromStockfish()
    {
        _nextMoveTime = Time.time + _timeBetweenMoves;

        string moveList = "position startpos moves";

        if (_gameMoves.Length > 0) moveList += " " + _gameMoves.ToString();

        if (_stockfishProcess != null && !_stockfishProcess.HasExited)
        {
            SendCommandToStockfish(moveList);
            SendCommandToStockfish("go depth 10");

            // Read lines until we find the "bestmove" line
            string line;
            while ((line = _stockfishProcess.StandardOutput.ReadLine()) != null)
            {
                if (line.StartsWith("bestmove"))
                {
                    // Extract the move (format: "bestmove e2e4 ponder e7e5")
                    string[] parts = line.Split(' ');
                    if (parts.Length >= 2)
                    {
                        string bestMove = parts[1];
                        _bestMove = bestMove;
                        _moveReady = true;
                    }
                    break;
                }
            }
        }
    }
}