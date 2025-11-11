using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }
    [SerializeField] private Camera _camera;
    [SerializeField] private Vector3 _cameraFlippedPosition;
    private Vector3 _cameraStartPosition;
    [SerializeField] private bool _flipCameraAfterEachMove = false;
    private bool _isFlipped = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;

        _cameraStartPosition = _camera.transform.position;
    }

    private void Start()
    {
        PieceManager.Instance.OnMoveCompleted += PieceManager_OnMoveCompleted;
    }

    private void PieceManager_OnMoveCompleted(object sender, PieceManager.OnMoveCompletedArgs e)
    {
        if (_flipCameraAfterEachMove && GameManager.Instance.IsCurrentPlayerWhite != _isFlipped)
            BoardManager.Instance.FlipBoard();
    }

    public void SetFlipBoard(bool flipBoard)
    {
        _flipCameraAfterEachMove = flipBoard;
    }

    public void FlipCamera()
    {
        if (_isFlipped) _camera.transform.position = _cameraStartPosition;
        else _camera.transform.position = _cameraFlippedPosition;

        _camera.transform.Rotate(0, 0, 180f);

        _isFlipped = !_isFlipped;
    }
}
