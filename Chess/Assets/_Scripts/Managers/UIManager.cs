using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _fenInput;
    [SerializeField] private Button _loadFENButton;
    [SerializeField] private TMP_InputField _pgnInput;
    [SerializeField] private Button _loadPGNButton;
    [SerializeField] private Button _nextMovePGNButton;
    [SerializeField] private Button _previousMovePGNButton;
    [SerializeField] private Button _firstMovePGNButton;
    [SerializeField] private Button _lastMovePGNButton;
    [SerializeField] private TabMenu _tabMenu;
    [SerializeField] private MoveHighlightingToggles _togglesTab;
    [SerializeField] private Transform _whiteCapturedPieces;
    [SerializeField] private Transform _blackCapturedPieces;

    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _loadFENButton.onClick.AddListener(OnLoadFENButtonClicked);
        _loadPGNButton.onClick.AddListener(OnLoadPGNButtonClicked);
        _nextMovePGNButton.onClick.AddListener(_nextMovePGNButtonClicked);
        _previousMovePGNButton.onClick.AddListener(_previousMovePGNButtonClicked);
        _firstMovePGNButton.onClick.AddListener(OnFirstMovePGNButtonClicked);
        _lastMovePGNButton.onClick.AddListener(OnLastMovePGNButtonClicked);

        _nextMovePGNButton.gameObject.SetActive(false);
        _previousMovePGNButton.gameObject.SetActive(false);
        _firstMovePGNButton.gameObject.SetActive(false);
        _lastMovePGNButton.gameObject.SetActive(false);
    }

    private void OnLoadFENButtonClicked()
    {
        PieceManager.Instance.LoadPosition(_fenInput.text);
    }

    private void OnLoadPGNButtonClicked()
    {
        PGNManager.Instance.ParsePGN(_pgnInput.text);

        _nextMovePGNButton.gameObject.SetActive(true);
        _previousMovePGNButton.gameObject.SetActive(true);
        _firstMovePGNButton.gameObject.SetActive(true);
        _lastMovePGNButton.gameObject.SetActive(true);
    }

    private void _nextMovePGNButtonClicked()
    {
        PGNManager.Instance.NextMove();
    }

    private void _previousMovePGNButtonClicked()
    {
        PGNManager.Instance.PreviousMove();
    }

    private void OnFirstMovePGNButtonClicked()
    {
        PGNManager.Instance.FirstMove();
    }

    private void OnLastMovePGNButtonClicked()
    {
        PGNManager.Instance.LastMove();
    }

    public void UpdateFENText(string text)
    {
        _fenInput.text = text;
    }

    public void SetTabMenuTab(int index)
    {
        _tabMenu.JumpToPage(index);
    }

    public void ShowActiveSquare(Square square)
    {
        _togglesTab.ShowSquareDetails(square);
    }

    public void HideActiveSquare()
    {
        _togglesTab.RemoveSquareDetails();
    }

    public void UpdatePieceIcons(bool isWhite, int pawnCount, int knightCount, int bishopCount, int rookCount, int queenCount)
    {
        Transform pieceHolder = isWhite ? _whiteCapturedPieces : _blackCapturedPieces;
        GameObject pieceIcon = isWhite ? PieceManager.Instance.WhitePawnIcon : PieceManager.Instance.BlackPawnIcon;
        GameObject newIcon;
        float xPos = 0f;
        float samePieceOffset = 0.15f;
        float newPieceOffset = 0.2f;
        int pieceCount = 0;

        for (int i = 0; i < pieceHolder.childCount; i++)
        {
            Destroy(pieceHolder.GetChild(i).gameObject);
        }

        for (int i = 0; i < pawnCount; i++)
        {
            newIcon = Instantiate(pieceIcon, pieceHolder.position + new Vector3(xPos, 0f, 0f), Quaternion.identity, pieceHolder);
            newIcon.GetComponentInChildren<SpriteRenderer>().sortingOrder = pieceCount;
            xPos += samePieceOffset;
            pieceCount++;
        }

        pieceIcon = isWhite ? PieceManager.Instance.WhiteKnightIcon : PieceManager.Instance.BlackKnightIcon;
        if (pawnCount > 0) xPos += newPieceOffset;

        for (int i = 0; i < knightCount; i++)
        {
            newIcon = Instantiate(pieceIcon, pieceHolder.position + new Vector3(xPos, 0f, 0f), Quaternion.identity, pieceHolder);
            newIcon.GetComponentInChildren<SpriteRenderer>().sortingOrder = pieceCount;
            xPos += samePieceOffset;
            pieceCount++;
        }

        pieceIcon = isWhite ? PieceManager.Instance.WhiteBishopIcon : PieceManager.Instance.BlackBishopIcon;
        if (knightCount > 0) xPos += newPieceOffset;

        for (int i = 0; i < bishopCount; i++)
        {
            newIcon = Instantiate(pieceIcon, pieceHolder.position + new Vector3(xPos, 0f, 0f), Quaternion.identity, pieceHolder);
            newIcon.GetComponentInChildren<SpriteRenderer>().sortingOrder = pieceCount;
            xPos += samePieceOffset;
            pieceCount++;
        }

        pieceIcon = isWhite ? PieceManager.Instance.WhiteRookIcon : PieceManager.Instance.BlackRookIcon;
        if (bishopCount > 0) xPos += newPieceOffset;

        for (int i = 0; i < rookCount; i++)
        {
            newIcon = Instantiate(pieceIcon, pieceHolder.position + new Vector3(xPos, 0f, 0f), Quaternion.identity, pieceHolder);
            newIcon.GetComponentInChildren<SpriteRenderer>().sortingOrder = pieceCount;
            xPos += samePieceOffset;
            pieceCount++;
        }

        pieceIcon = isWhite ? PieceManager.Instance.WhiteQueenIcon : PieceManager.Instance.BlackQueenIcon;
        if (rookCount > 0) xPos += newPieceOffset;

        for (int i = 0; i < queenCount; i++)
        {
            newIcon = Instantiate(pieceIcon, pieceHolder.position + new Vector3(xPos, 0f, 0f), Quaternion.identity, pieceHolder);
            newIcon.GetComponentInChildren<SpriteRenderer>().sortingOrder = pieceCount;
            xPos += samePieceOffset;
            pieceCount++;
        }
    }

    public void ResetPieceIcons()
    {
        for (int i = 0; i < _whiteCapturedPieces.childCount; i++)
        {
            Destroy(_whiteCapturedPieces.GetChild(i).gameObject);
        }

        for (int i = 0; i < _blackCapturedPieces.childCount; i++)
        {
            Destroy(_blackCapturedPieces.GetChild(i).gameObject);
        }
    }
}
