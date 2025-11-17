using System;
using System.Collections.Generic;
using UnityEngine;

public class ToggleManager : MonoBehaviour
{
    public static ToggleManager Instance { get; private set; }

    [SerializeField] private bool _isAnalysisModeActivated = false;
    public bool IsAnalysisModeActivated { get { return _isAnalysisModeActivated; } }

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void SetAnalysisModeActivated(bool active)
    {
        _isAnalysisModeActivated = active;

        if (active)
        {
            PieceManager.Instance.UpdateAllPieceAnalysisMoves();
        }
    }
}
