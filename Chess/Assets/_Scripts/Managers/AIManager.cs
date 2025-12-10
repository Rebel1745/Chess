using UnityEngine;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System;
using System.Text;

public class AIManager : MonoBehaviour
{
    public static AIManager Instance { get; private set; }

    public string exeFileName = "stockfish-windows-x86-64-sse41-popcnt.exe"; // Specify the file name of the .exe file
    private Process stockfishProcess;
    private Thread outputReaderThread;

    private StringBuilder _gameMoves = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        SetupStockfish();
    }

    public void Start()
    {
        GameManager.Instance.OnGameStarted += GameManager_OnGameStarted;
        GameManager.Instance.OnWaitingForMove += GameManager_OnWaitingForMove;
        PieceManager.Instance.OnMoveCompleted += PieceManager_OnMoveCompleted;
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
        if (_gameMoves.Length == 0) _gameMoves.Append(uciMove);
        else _gameMoves.Append(" " + uciMove);
    }

    private void OnDestroy()
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            stockfishProcess.StandardInput.WriteLine("quit");
            stockfishProcess.StandardInput.Flush();
            stockfishProcess.WaitForExit();
            stockfishProcess.Close();

            // Stop the output reader thread
            //outputReaderThread.Join();
        }
    }

    public void SetupStockfish()
    {
        // Get the path to the executable relative to the Assets folder
        string exeFilePath = Path.Combine(Application.dataPath, exeFileName);

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

            stockfishProcess = new Process
            {
                StartInfo = startInfo
            };
            stockfishProcess.Start();
        }
        else
        {
            UnityEngine.Debug.LogError("Stockfish executable file not found: " + exeFilePath);
        }
    }

    private void SendCommandToStockfish(string command)
    {
        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            stockfishProcess.StandardInput.WriteLine(command);
            stockfishProcess.StandardInput.Flush();
        }
    }

    private void GetMoveFromStockfish()
    {
        string moveList = "position startpos moves";

        if (_gameMoves.Length > 0) moveList += " " + _gameMoves.ToString();

        if (stockfishProcess != null && !stockfishProcess.HasExited)
        {
            SendCommandToStockfish(moveList);
            SendCommandToStockfish("go depth 10");

            // Read lines until we find the "bestmove" line
            string line;
            while ((line = stockfishProcess.StandardOutput.ReadLine()) != null)
            {
                if (line.StartsWith("bestmove"))
                {
                    // Extract the move (format: "bestmove e2e4 ponder e7e5")
                    string[] parts = line.Split(' ');
                    if (parts.Length >= 2)
                    {
                        string bestMove = parts[1];
                        UnityEngine.Debug.Log("Best move: " + bestMove);

                        PieceManager.Instance.CreateMoveFromUCIString(bestMove);
                    }
                    break;
                }
            }
        }
    }
}