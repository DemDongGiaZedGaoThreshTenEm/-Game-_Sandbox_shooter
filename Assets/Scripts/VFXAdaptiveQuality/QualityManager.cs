using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Quản lý chất lượng VFX dựa trên FPS trung bình.
/// Chỉ quyết định QualityLevel và yêu cầu ObjectPoolManager
/// cập nhật toàn bộ Effect trong Pool.
public enum QualityLevel
{
    Low,
    Medium,
    High
}
public class QualityManager : MonoBehaviour
{
    public static QualityManager Instance { get; private set; }

    [Header("Reference")]
    [SerializeField]
    private PerformanceMonitor performanceMonitor;

    [Header("Auto Quality")]

    [Tooltip("Bật/Tắt tự động thay đổi Quality theo FPS.")]
    [SerializeField]
    private bool autoQuality = true;

    [Header("FPS Threshold")]

    [Tooltip("FPS nhỏ hơn ngưỡng này sẽ chuyển xuống Low.")]
    [SerializeField]
    private float lowThreshold = 45f;

    [Tooltip("FPS từ Low Threshold đến ngưỡng này sẽ là Medium.\nLớn hơn sẽ là High.")]
    [SerializeField]
    private float highThreshold = 80f;

    [Header("Change Settings")]

    [Tooltip("FPS phải duy trì trong khoảng thời gian này mới đổi Quality.")]
    [SerializeField]
    private float changeDelay = 2f;

    /// <summary>
    /// Chất lượng hiện tại.
    /// </summary>
    public QualityLevel CurrentQuality { get; private set; }
        = QualityLevel.High;

    private float timer;

    #region Unity

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ApplyQuality(CurrentQuality);
    }

    private void Update()
    {
        if (!autoQuality)
            return;

        if (performanceMonitor == null)
            return;

        QualityLevel targetQuality =
            CalculateQuality(performanceMonitor.AverageFPS);

        // Đã đúng Quality → reset timer.
        if (targetQuality == CurrentQuality)
        {
            timer = 0f;
            return;
        }

        timer += Time.deltaTime;

        if (timer >= changeDelay)
        {
            ApplyQuality(targetQuality);
            timer = 0f;
        }
    }

    #endregion

    #region Public

    /// <summary>
    /// Cho phép các UI Button hoặc Debug
    /// đổi Quality thủ công.
    /// </summary>
    public void SetQuality(QualityLevel level)
    {
        timer = 0f;
        ApplyQuality(level);
    }

    #endregion

    #region Private

    /// <summary>
    /// Xác định QualityLevel dựa trên FPS.
    /// </summary>
    private QualityLevel CalculateQuality(float fps)
    {
        if (fps < lowThreshold)
            return QualityLevel.Low;

        if (fps < highThreshold)
            return QualityLevel.Medium;

        return QualityLevel.High;
    }

    /// <summary>
    /// Áp dụng Quality mới.
    /// </summary>
    private void ApplyQuality(QualityLevel level)
    {
        // Không làm gì nếu đang ở đúng mức.
        if (CurrentQuality == level)
            return;

        CurrentQuality = level;

        // Gửi lệnh cho toàn bộ Effect trong Pool.
        if (ObjectPoolManager.Instance != null)
        {
            ObjectPoolManager.Instance.ApplyQuality(level);
        }

        Debug.Log($"[QualityManager] Current Quality : {level}");
    }

    #endregion
}