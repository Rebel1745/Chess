using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager Instance { get; private set; }

    [SerializeField] private Transform _arrowHolder;
    [SerializeField] private GameObject _arrowShaftPrefab;
    [SerializeField] private GameObject _arrowHeadPrefab;
    private Dictionary<string, GameObject> _arrowSquareCodesToArrowGameObjectDictionary = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void DrawArrow(Square startSquare, Square endSquare)
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
            direction = direction.normalized;

            float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) - 90;

            GameObject newArrow = new()
            {
                name = "Arrow " + startSquare.SquarePGNCode + " - " + endSquare.SquarePGNCode
            };
            newArrow.transform.parent = _arrowHolder;

            // instantiate the shaft and then scale it
            GameObject newShaft = Instantiate(_arrowShaftPrefab, midpoint, Quaternion.Euler(0, 0, angle), newArrow.transform);
            newShaft.transform.localScale = new Vector3(newArrow.transform.localScale.x, distance, newArrow.transform.localScale.z);

            // instantiate the arrow head and place it at the center of the end square and angle it
            GameObject newHead = Instantiate(_arrowHeadPrefab, endSquare.transform.position, Quaternion.Euler(0, 0, angle), newArrow.transform);

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
}
