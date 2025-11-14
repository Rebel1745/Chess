using System.Collections.Generic;
using UnityEngine;

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager Instance { get; private set; }

    [SerializeField] private Transform _arrowHolder;
    [SerializeField] private GameObject _arrowShaftPrefab;
    [SerializeField] private GameObject _arrowHeadPrefab;
    private Dictionary<Square, Square> _arrowStartEndPoints = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void DrawArrow(Square startSquare, Square endSquare)
    {
        // the distance between the two points will determine the scaling of the 1 unit arror shaft (shut yo mouth)
        float distance = Vector3.Distance(startSquare.transform.position, endSquare.transform.position);

        // the midpoint of the two vectors is the instantiation point
        Vector3 midpoint = (startSquare.transform.position + endSquare.transform.position) / 2f;

        // get the direction from the start to the end squares
        Vector3 direction = endSquare.transform.position - startSquare.transform.position;
        // direction = transform.InverseTransformDirection(direction);

        float angle = Vector3.Angle(startSquare.transform.up, direction);
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        GameObject newArrow = new()
        {
            name = "Arrow " + startSquare.SquarePGNCode + " - " + endSquare.SquarePGNCode
        };
        newArrow.transform.parent = _arrowHolder;

        // instantiate the shaft and then scale it
        GameObject newShaft = Instantiate(_arrowShaftPrefab, midpoint, Quaternion.Euler(0f, 0f, angle), newArrow.transform);
        newShaft.transform.localScale = new Vector3(newArrow.transform.localScale.x, distance, newArrow.transform.localScale.z);

        // instantiate the arrow head and place it at the center of the end square and angle it
        GameObject newHead = Instantiate(_arrowHeadPrefab, endSquare.transform.position, Quaternion.Euler(0f, 0f, angle), newArrow.transform);

        Debug.Log($"{startSquare.SquarePGNCode} {startSquare.transform.position} {endSquare.SquarePGNCode} {endSquare.transform.position} {angle}");
    }
}
