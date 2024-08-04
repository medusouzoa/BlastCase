using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float _padding = 1f; // Extra space around the board

    [SerializeField] private Camera _camera;
    public static CameraController Instance { get; private set; }

    private void Start()
    {
        // _camera = Camera.main;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdjustCameraSize(int width, int height)
    {
        int boardWidth = width;
        int boardHeight = height;

        float boardAspectRatio = (float)boardWidth / boardHeight;
        float cameraAspectRatio = (float)Screen.width / Screen.height;

        if (cameraAspectRatio >= boardAspectRatio)
        {
            _camera.orthographicSize = boardHeight / 2f + _padding;
        }
        else
        {
            float differenceInSize = boardAspectRatio / cameraAspectRatio;
            _camera.orthographicSize = (boardHeight / 2f) * differenceInSize + _padding;
        }
    }
}