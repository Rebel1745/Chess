using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoveListElement : MonoBehaviour
{
    [SerializeField] private TMP_Text _moveNumberText;
    [SerializeField] private Button _whiteMoveButton;
    [SerializeField] private Button _blackMoveButton;

    private MoveList _moveList;
    private MoveDetails _whiteMove;
    private MoveDetails _blackMove;

    private void Start()
    {
        _whiteMoveButton.onClick.AddListener(OnWhiteMoveClicked);
        _blackMoveButton.onClick.AddListener(OnBlackMoveClicked);
    }

    private void Oestroy()
    {
        _whiteMoveButton.onClick.RemoveListener(OnWhiteMoveClicked);
        _blackMoveButton.onClick.RemoveListener(OnBlackMoveClicked);
    }

    private void OnWhiteMoveClicked()
    {
        _moveList.TriggerMoveClicked(_whiteMove);
    }

    private void OnBlackMoveClicked()
    {
        if (_blackMove.MoveNumber != -1)
            _moveList.TriggerMoveClicked(_blackMove);
    }

    public void SetMoveDetails(MoveList moveList, MoveDetails whiteMove, MoveDetails blackMove)
    {
        _moveList = moveList;
        _whiteMove = whiteMove;
        _blackMove = blackMove;

        int moveNumber = Mathf.FloorToInt(whiteMove.MoveNumber / 2) + 1;
        _moveNumberText.text = moveNumber.ToString();

        _whiteMoveButton.GetComponentInChildren<TMP_Text>().text = whiteMove.PGNCode;
        _blackMoveButton.GetComponentInChildren<TMP_Text>().text = blackMove.MoveNumber == -1 ? "..." : blackMove.PGNCode;
    }
}
