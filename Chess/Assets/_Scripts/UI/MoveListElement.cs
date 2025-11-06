using TMPro;
using UnityEngine;

public class MoveListElement : MonoBehaviour
{
    [SerializeField] private TMP_Text _moveNumberText;
    [SerializeField] private TMP_Text _whiteMoveText;
    [SerializeField] private TMP_Text _backMoveText;

    public void SetMoveDetails(MoveDetails whiteMove, MoveDetails blackMove)
    {
        int moveNumber = Mathf.FloorToInt(whiteMove.MoveNumber / 2) + 1;
        _moveNumberText.text = moveNumber.ToString();

        _whiteMoveText.text = whiteMove.PGNCode;
        _backMoveText.text = blackMove.MoveNumber == -1 ? "..." : blackMove.PGNCode;
    }
}
