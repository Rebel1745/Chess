using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class TabMenu : MonoBehaviour
{
    [Header("Current Index")]
    [SerializeField] private int _pageIndex = 0;

    [Header("Components")]
    [SerializeField] private ToggleGroup _toggleGroup;
    [SerializeField] private Transform _tabsHolder;
    [SerializeField] private List<Toggle> _tabs = new();
    [SerializeField] private Transform _pagesHolder;
    [SerializeField] private List<CanvasGroup> _pages = new();

    public event EventHandler<OnPageIndexChangedArgs> OnPageIndexChanged;
    public class OnPageIndexChangedArgs : EventArgs
    {
        public int Index;
    }

    private void Initialise()
    {
        _toggleGroup = GetComponent<ToggleGroup>();

        _tabs.Clear();
        _pages.Clear();

        for (int i = 0; i < _tabsHolder.childCount; i++)
        {
            _tabs.Add(_tabsHolder.GetChild(i).GetComponent<Toggle>());
        }
        for (int i = 0; i < _pagesHolder.childCount; i++)
        {
            _pages.Add(_pagesHolder.GetChild(i).GetComponent<CanvasGroup>());
        }
    }

    private void Reset()
    {
        Initialise();
    }

    private void OnValidate()
    {
        Initialise();
        OpenPage(_pageIndex);
        _tabs[_pageIndex].SetIsOnWithoutNotify(true);
    }

    private void Awake()
    {
        foreach (var toggle in _tabs)
        {
            toggle.onValueChanged.AddListener(CheckForTab);
            toggle.group = _toggleGroup;
        }
    }

    private void OnDestroy()
    {
        foreach (var toggle in _tabs)
        {
            toggle.onValueChanged.RemoveListener(CheckForTab);
        }
    }

    private void CheckForTab(bool value)
    {
        for (int i = 0; i < _tabs.Count; i++)
        {
            if (!_tabs[i].isOn) continue;
            _pageIndex = i;
        }

        OpenPage(_pageIndex);
    }

    private void OpenPage(int index)
    {
        EnsureIndexIsInRange(index);

        for (int i = 0; i < _pages.Count; i++)
        {
            bool isActivePage = (i == _pageIndex);

            _pages[i].alpha = isActivePage ? 1.0f : 0.0f;
            _pages[i].interactable = isActivePage;
            _pages[i].blocksRaycasts = isActivePage;
        }

        if (Application.isPlaying)
            OnPageIndexChanged?.Invoke(this, new OnPageIndexChangedArgs
            {
                Index = _pageIndex
            });
    }

    private void EnsureIndexIsInRange(int index)
    {
        if (_tabs.Count == 0 || _pages.Count == 0)
        {
            Debug.Log("Forgot to setup Tabs or Pages");
            return;
        }

        _pageIndex = Mathf.Clamp(index, 0, _pages.Count - 1);
    }

    public void JumpToPage(int page)
    {
        EnsureIndexIsInRange(page);

        _tabs[_pageIndex].isOn = true;
    }
}
