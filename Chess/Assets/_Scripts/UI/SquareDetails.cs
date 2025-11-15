using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SquareDetails : MonoBehaviour
{
    [SerializeField] private Image _squareImage;
    [SerializeField] private TMP_Text _squareText;

    public void SetSquareDetails(Square square)
    {
        _squareImage.color = square.DefaultSquareColour;
        _squareText.text = square.SquarePGNCode;
    }
}
