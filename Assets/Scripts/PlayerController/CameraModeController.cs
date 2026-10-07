using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using StarterAssets;
public class CameraModeController : MonoBehaviour
{
    [Header("Cameras")]
    [SerializeField] private Transform fpsTarget;
    [SerializeField] private CinemachineVirtualCamera tpsCamera;
    [SerializeField] private CinemachineVirtualCamera fpsCamera;
    [SerializeField] private Camera mainCamera;

    [Header("Transition")]
    [SerializeField] private CinemachineBrain cinemachineBrain;
    [SerializeField] private float tpsToFpsTime = 0.3f;
    [SerializeField] private float fpsToTpsTime = 0.25f;

    [Header("PC Input")]
    [SerializeField] private bool usePCInput = true;
    [SerializeField] private KeyCode toggleKey = KeyCode.V;

    [Header("Mobile Input")]
    [SerializeField] private bool useMobileButton = false;
    [SerializeField] private Button mobileToggleButton;
    [SerializeField] private PlayerController playerController;


    private bool isFPS;

    private void Awake()
    {
        SetupMobileButton();
    }

    private void Start()
    {
        SetTPSImmediate();
    }

    private void Update()
    {
        if (usePCInput && Input.GetKeyDown(toggleKey))
        {
            Debug.LogWarning($"{name}: View mode Changed");
            ToggleCamera();
        }
    }

    private void SetupMobileButton()
    {
        if (!useMobileButton)
            return;

        if (mobileToggleButton == null)
        {
            Debug.LogWarning(
                $"{name}: Mobile input is enabled but no Button is assigned."
            );

            return;
        }

        mobileToggleButton.onClick.RemoveListener(ToggleCamera);
        mobileToggleButton.onClick.AddListener(ToggleCamera);
    }

    public void ToggleCamera()
    {
        if (isFPS)
            SetTPS();
        else
            SetFPS();
    }

    public void SetFPS()
    {
        // =====================================================
        // 1. Đồng bộ hướng nhìn hiện tại của TPS sang FPS
        // =====================================================

        UpdateFPSTarget();

        // =====================================================
        // 2. Đổi state
        // =====================================================

        isFPS = true;
        playerController.SetViewMode(true);

        // =====================================================
        // 3. Set thời gian blend
        // =====================================================

        SetBlendTime(tpsToFpsTime);

        // =====================================================
        // 4. Chuyển camera
        // =====================================================

        fpsCamera.Priority = 20;
        tpsCamera.Priority = 10;
    }

    public void SetTPS()
    {
        isFPS = false;
        playerController.SetViewMode(false);

        SetBlendTime(fpsToTpsTime);

        tpsCamera.Priority = 20;
        fpsCamera.Priority = 10;
    }

    private void SetTPSImmediate()
    {
        isFPS = false;

        tpsCamera.Priority = 20;
        fpsCamera.Priority = 10;
    }

    private void UpdateFPSTarget()
    {
        if (fpsTarget == null)
        {
            Debug.LogWarning(
                $"{name}: FPS Target is not assigned."
            );

            return;
        }

        if (mainCamera == null)
        {
            Debug.LogWarning(
                $"{name}: Main Camera is not assigned."
            );

            return;
        }

        // Lấy hướng nhìn hiện tại của camera
        fpsTarget.rotation = mainCamera.transform.rotation;
    }
    private void LateUpdate()
    {
        if (!isFPS)
        {
            UpdateFPSTarget();
        }
    }
    private void SetBlendTime(float time)
    {
        if (cinemachineBrain == null)
            return;

        cinemachineBrain.m_DefaultBlend =
            new CinemachineBlendDefinition(
                CinemachineBlendDefinition.Style.EaseInOut,
                time
            );
    }

    public bool IsFPS()
    {
        return isFPS;
    }
}