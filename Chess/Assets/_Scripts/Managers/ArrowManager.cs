using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager Instance { get; private set; }

    [SerializeField] private Transform _arrowHolder;
    [SerializeField] private GameObject _arrowShaftPrefab;
    [SerializeField] private GameObject _arrowHeadPrefab;
    [SerializeField] private Color _standardMoveColour;
    [SerializeField] private Color _captureMoveColour;
    [SerializeField] private Color _protectionMoveColour;
    [SerializeField] private Color _xRayMoveColour;
    [SerializeField] private GameObject _shieldIconPrefab;
    [SerializeField] private GameObject _swordIconPrefab;

    private Dictionary<string, GameObject> _arrowSquareCodesToArrowGameObjectDictionary = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void DrawArrow(Square startSquare, Square endSquare, ANALYSIS_MOVE_TYPE moveType = ANALYSIS_MOVE_TYPE.Standard, bool drawIcon = false, PIECE_TYPE[] pieceTypeIcons = null, bool isWhite = true)
    {
        string squareCodesString = startSquare.SquarePGNCode + endSquare.SquarePGNCode;

        if (_arrowSquareCodesToArrowGameObjectDictionary.ContainsKey(squareCodesString))
        {
            RemoveArrow(startSquare, endSquare);
        }
        else
        {
            // the distance between the two points will determine the scaling of the 1 unit arror shaft (shut yo mouth)
            float distance = Vector3.Distance(startSquare.transform.position, endSquare.transform.position);

            // the midpoint of the two vectors is the instantiation point
            Vector3 midpoint = (startSquare.transform.position + endSquare.transform.position) / 2f;

            // get the direction from the start to the end squares
            Vector3 direction = endSquare.transform.position - startSquare.transform.position;
            //direction = direction.normalized;

            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90;

            float baseOffset = 0.5f;

            // Calculate how much of the movement is diagonal vs straight
            // For pure horizontal/vertical: abs(x) or abs(y) = 1, the other = 0
            // For pure diagonal: abs(x) = abs(y) = 0.707...
            float diagonalFactor = Mathf.Min(Mathf.Abs(direction.x), Mathf.Abs(direction.y));

            // Scale the offset: 1.0 for straight moves, up to ~1.414 for diagonal moves
            float scaledOffset = baseOffset * (1f + diagonalFactor * (Mathf.Sqrt(2f) - 1f));

            float shaftLength = distance - scaledOffset;

            GameObject newArrow = new()
            {
                name = "Arrow " + startSquare.SquarePGNCode + " - " + endSquare.SquarePGNCode
            };
            newArrow.transform.parent = _arrowHolder;

            // instantiate the shaft and then scale it
            GameObject newShaft = Instantiate(_arrowShaftPrefab, midpoint, Quaternion.Euler(0, 0, angle), newArrow.transform);
            newShaft.transform.localScale = new Vector3(1f, shaftLength, 1f);
            Color arrowColour = GetColourFromMoveType(moveType);
            SpriteRenderer sr = newShaft.GetComponentInChildren<SpriteRenderer>();
            sr.color = arrowColour;
            sr.sortingOrder = 999 - (_arrowSquareCodesToArrowGameObjectDictionary.Count * 2);

            // Calculate the head position offset based on the reduction in shaft length
            Vector3 normalizedDirection = direction.normalized;
            float lengthReduction = distance - shaftLength;
            Vector3 headOffset = normalizedDirection * (distance / 2f - lengthReduction / 2f);
            Vector3 headSpawnPoint = midpoint + headOffset;

            // instantiate the arrow head and place it at the center of the end square and angle it
            GameObject newHead = Instantiate(_arrowHeadPrefab, headSpawnPoint, Quaternion.Euler(0, 0, angle), newArrow.transform);
            sr = newHead.GetComponentInChildren<SpriteRenderer>();
            sr.color = arrowColour;
            sr.sortingOrder = 1000 - (_arrowSquareCodesToArrowGameObjectDictionary.Count * 2);

            // should we add an icon to the arrow?
            if (ToggleManager.Instance.ShowMoveIcons && drawIcon)
            {
                GameObject iconPrefab = _swordIconPrefab;

                if (pieceTypeIcons == null || pieceTypeIcons.Length == 0)
                {
                    if (moveType != ANALYSIS_MOVE_TYPE.Standard)
                    {
                        switch (moveType)
                        {
                            case ANALYSIS_MOVE_TYPE.Capture:
                                iconPrefab = _swordIconPrefab;
                                break;
                            case ANALYSIS_MOVE_TYPE.Protection:
                                iconPrefab = _shieldIconPrefab;
                                break;
                        }

                        Instantiate(iconPrefab, midpoint, Quaternion.identity, newArrow.transform);
                    }
                }
                else
                {
                    // loop through the piece type icons and stick them on the lines
                    if (pieceTypeIcons.Length == 1)
                    {
                        iconPrefab = GetPieceIconFromType(pieceTypeIcons[0], isWhite);
                    }
                    else if (pieceTypeIcons.Length == 2)
                    {
                        iconPrefab = GetPieceIconFromTypes(pieceTypeIcons, isWhite);
                    }

                    Instantiate(iconPrefab, midpoint, Quaternion.identity, newArrow.transform);
                }
            }

            _arrowSquareCodesToArrowGameObjectDictionary.Add(squareCodesString, newArrow);
        }
    }

    public void RemoveArrow(Square startSquare, Square endSquare)
    {
        string squareCodesString = startSquare.SquarePGNCode + endSquare.SquarePGNCode;

        if (!_arrowSquareCodesToArrowGameObjectDictionary.ContainsKey(squareCodesString)) return;

        // destroy the arrow
        Destroy(_arrowSquareCodesToArrowGameObjectDictionary[squareCodesString]);
        // remove from dictionary
        _arrowSquareCodesToArrowGameObjectDictionary.Remove(squareCodesString);
    }

    public void RemoveArrow(string arrowCode)
    {
        Square startSquare, endSquare;

        startSquare = BoardManager.Instance.GetSquareFromPGNCode(arrowCode.Substring(0, 2));
        endSquare = BoardManager.Instance.GetSquareFromPGNCode(arrowCode.Substring(2, 2));

        RemoveArrow(startSquare, endSquare);
    }

    public void DestroyAllArrows()
    {
        foreach (KeyValuePair<string, GameObject> arrows in _arrowSquareCodesToArrowGameObjectDictionary)
        {
            Destroy(arrows.Value);
        }

        _arrowSquareCodesToArrowGameObjectDictionary.Clear();
    }

    public void RemoveArrowsFromSquare(Square square)
    {
        for (int i = _arrowSquareCodesToArrowGameObjectDictionary.Count - 1; i >= 0; i--)
        {
            if (_arrowSquareCodesToArrowGameObjectDictionary.ElementAt(i).Key.Substring(2, 2) == square.SquarePGNCode)
                RemoveArrow(_arrowSquareCodesToArrowGameObjectDictionary.ElementAt(i).Key);
        }
    }

    private Color GetColourFromMoveType(ANALYSIS_MOVE_TYPE moveType)
    {
        Color moveColour = _standardMoveColour;
        switch (moveType)
        {
            case ANALYSIS_MOVE_TYPE.Capture:
                moveColour = _captureMoveColour;
                break;
            case ANALYSIS_MOVE_TYPE.Protection:
                moveColour = _protectionMoveColour;
                break;
        }

        return moveColour;
    }

    private GameObject GetPieceIconFromType(PIECE_TYPE pieceType, bool isWhite)
    {
        GameObject iconPrefab = PieceManager.Instance.WhiteQueenIcon;

        switch (pieceType)
        {
            case PIECE_TYPE.Knight:
                iconPrefab = isWhite ? PieceManager.Instance.WhiteKnightIcon : PieceManager.Instance.BlackKnightIcon;
                break;
            case PIECE_TYPE.Bishop:
                iconPrefab = isWhite ? PieceManager.Instance.WhiteBishopIcon : PieceManager.Instance.BlackBishopIcon;
                break;
            case PIECE_TYPE.Rook:
                iconPrefab = isWhite ? PieceManager.Instance.WhiteRookIcon : PieceManager.Instance.BlackRookIcon;
                break;
            case PIECE_TYPE.Queen:
                iconPrefab = isWhite ? PieceManager.Instance.WhiteQueenIcon : PieceManager.Instance.BlackQueenIcon;
                break;
        }

        return iconPrefab;
    }

    private GameObject GetPieceIconFromTypes(PIECE_TYPE[] pieceTypes, bool isWhite)
    {
        GameObject iconPrefab = PieceManager.Instance.WhiteQueenAndBishopIcon;

        if (pieceTypes.Length != 2) Debug.LogError("Incorrect number of piece types");

        if (pieceTypes[0] == PIECE_TYPE.Bishop || pieceTypes[1] == PIECE_TYPE.Bishop)
            iconPrefab = isWhite ? PieceManager.Instance.WhiteQueenAndBishopIcon : PieceManager.Instance.BlackQueenAndBishopIcon;
        else if (pieceTypes[0] == PIECE_TYPE.Rook || pieceTypes[1] == PIECE_TYPE.Rook)
            iconPrefab = isWhite ? PieceManager.Instance.WhiteQueenAndRookIcon : PieceManager.Instance.BlackQueenAndRookIcon;
        else Debug.LogError("Neither bishop nor rook in the piece type array. Que pasa?");

        return iconPrefab;
    }
}
