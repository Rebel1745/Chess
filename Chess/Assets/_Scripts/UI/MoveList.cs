using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class MoveList : MonoBehaviour
{
    [SerializeField] private Transform _moveListContainer;
    [SerializeField] private GameObject _moveDetailsPrefab;
    [SerializeField] private Color _moveHighlightColour;

    private List<MoveListElement> _moveListElements = new();

    public event EventHandler<OnMoveListClickedArgs> OnMoveListClicked;
    public class OnMoveListClickedArgs : EventArgs
    {
        public MoveDetails Move;
    }

    private void Start()
    {
        PGNManager.Instance.OnMoveDetailsChanged += PGNManager_OnMoveDetailsChanged;
        PGNManager.Instance.OnMoveNumberChanged += PGNManager_OnMoveNumberChanged;
    }

    private void PGNManager_OnMoveDetailsChanged(object sender, PGNManager.OnMoveDetailsChangedArgs e)
    {
        UpdateMoveList(e.MoveDetailsList);
    }

    private void UpdateMoveList(List<MoveDetails> moveDetailsList)
    {
        GameObject newMoveListElement;
        MoveListElement moveListElement;
        MoveDetails emptyMove = new()
        {
            MoveNumber = -1
        };
        MoveDetails whiteMove, blackMove;
        int iPlusOne;

        ClearMoveList();

        for (int i = 0; i < moveDetailsList.Count; i++)
        {
            iPlusOne = i + 1;
            newMoveListElement = Instantiate(_moveDetailsPrefab, _moveListContainer);
            moveListElement = newMoveListElement.GetComponent<MoveListElement>();
            _moveListElements.Add(moveListElement);
            whiteMove = moveDetailsList[i];
            blackMove = moveDetailsList.Count > iPlusOne ? moveDetailsList[iPlusOne] : emptyMove;

            moveListElement.SetMoveDetails(this, whiteMove, blackMove);

            // increment again
            i++;
        }
    }

    private void ClearMoveList()
    {
        for (int i = 0; i < _moveListContainer.childCount; i++)
        {
            Destroy(_moveListContainer.GetChild(i).gameObject);
        }

        _moveListElements.Clear();
    }

    public void TriggerMoveClicked(MoveDetails move)
    {
        OnMoveListClicked?.Invoke(this, new OnMoveListClickedArgs()
        {
            Move = move
        });
    }

    private void PGNManager_OnMoveNumberChanged(object sender, PGNManager.OnMoveNumberChangedArgs e)
    {
        foreach (MoveListElement element in _moveListElements)
        {
            element.ResetColours();
        }

        if (e.MoveNumber >= 0)
            _moveListElements[Mathf.FloorToInt(e.MoveNumber / 2)].HighlightMove(_moveHighlightColour);
    }
}
