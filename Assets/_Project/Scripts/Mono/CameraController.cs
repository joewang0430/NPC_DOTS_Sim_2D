using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 20f;
    // 平滑移动的系数，越小越平滑，越大越跟手
    public float smoothTime = 0.15f; 

    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minSize = 2f;    // 最小能放多大（特写）
    public float maxSize = 50f;   // 最大能缩多小（上帝视角）
    public float zoomSmoothTime = 0.2f;

    private Camera _cam;
    private Vector3 _targetPosition;
    private float _targetSize;
    private Vector3 _currentVelocity; // 用于 SmoothDamp 的速度缓存
    private float _zoomVelocity;      // 用于缩放 SmoothDamp 的速度缓存

    void Awake()
    {
        _cam = GetComponent<Camera>();
        
        // 初始化目标值为当前值
        _targetPosition = transform.position;
        _targetSize = _cam.orthographicSize;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
        
        // 放在 LateUpdate 或者 Update 最后执行平滑应用
        ApplySmoothing();
    }

    void HandleMovement()
    {
        // 1. 获取 WASD 输入
        float h = Input.GetAxisRaw("Horizontal"); // A/D
        float v = Input.GetAxisRaw("Vertical");   // W/S

        // 2. 根据当前的缩放比例调整移动速度
        // 当拉远视野时(Size大)，移动应该更快；拉近时应该更慢，方便微调
        float speedMultiplier = _cam.orthographicSize / 10f; 
        
        Vector3 direction = new Vector3(h, v, 0).normalized;
        
        if (direction.magnitude > 0.1f)
        {
            // 累加目标位置
            _targetPosition += direction * moveSpeed * speedMultiplier * Time.deltaTime;
        }
    }

    void HandleZoom()
    {
        // 1. 获取滚轮输入 (通常是 -1 到 1 之间的小数)
        float scroll = Input.mouseScrollDelta.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            // 2. 计算目标 Size
            // 这里的逻辑是：滚轮向下(负) -> Size变大(远离)；滚轮向上(正) -> Size变小(拉近)
            _targetSize -= scroll * zoomSpeed;
            
            // 3. 限制范围
            _targetSize = Mathf.Clamp(_targetSize, minSize, maxSize);
        }
    }

    void ApplySmoothing()
    {
        // 1. 平滑移动位置
        // 注意：我们只平滑 X 和 Y，保持 Z 轴不变（因为是 2D 相机）
        Vector3 newPos = Vector3.SmoothDamp(transform.position, _targetPosition, ref _currentVelocity, smoothTime);
        // 强制锁定 Z 轴，防止相机意外跑到 Z=0 前面去
        newPos.z = -10f; 
        transform.position = newPos;

        // 2. 平滑缩放 Size
        float newSize = Mathf.SmoothDamp(_cam.orthographicSize, _targetSize, ref _zoomVelocity, zoomSmoothTime);
        _cam.orthographicSize = newSize;
    }
}