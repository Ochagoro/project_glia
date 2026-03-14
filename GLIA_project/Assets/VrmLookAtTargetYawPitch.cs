using UnityEngine;
using UniVRM10;

public class VrmLookAtWithHead : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Transform head;
    [SerializeField] private Transform neck;

    [Header("Eye LookAt")]
    [SerializeField] private float maxEyeYaw = 25f;
    [SerializeField] private float maxEyePitch = 20f;

    [Header("Head Follow")]
    [SerializeField] private float maxHeadYaw = 12f;
    [SerializeField] private float maxHeadPitch = 8f;
    [SerializeField] private float headFollowSpeed = 6f;

    private Vrm10Instance _vrm;
    private Quaternion _headInitialLocalRotation;
    private Quaternion _neckInitialLocalRotation;

    private void Awake()
    {
        _vrm = GetComponent<Vrm10Instance>();

        if (_vrm == null)
        {
            Debug.LogError("Vrm10Instance が見つかりません。");
            enabled = false;
            return;
        }

        if (head == null)
        {
            Debug.LogWarning("head が未設定です。Headボーンを入れてください。");
        }
        else
        {
            _headInitialLocalRotation = head.localRotation;
        }

        if (neck != null)
        {
            _neckInitialLocalRotation = neck.localRotation;
        }
    }

    private void LateUpdate()
    {
        Transform t = target;
        if (t == null && Camera.main != null)
        {
            t = Camera.main.transform;
        }

        if (t == null || head == null) return;

        Vector3 dirWorld = (t.position - head.position).normalized;
        Vector3 dirLocal = head.InverseTransformDirection(dirWorld);

        float yaw = Mathf.Atan2(dirLocal.x, dirLocal.z) * Mathf.Rad2Deg;
        float pitch = -Mathf.Atan2(dirLocal.y, dirLocal.z) * Mathf.Rad2Deg;

        // 目線
        float eyeYaw = Mathf.Clamp(yaw, -maxEyeYaw, maxEyeYaw);
        float eyePitch = Mathf.Clamp(pitch, -maxEyePitch, maxEyePitch);
        _vrm.Runtime.LookAt.SetLookAtYawPitch(eyeYaw, eyePitch);

        // 頭
        float headYaw = Mathf.Clamp(yaw, -maxHeadYaw, maxHeadYaw);
        float headPitch = Mathf.Clamp(pitch, -maxHeadPitch, maxHeadPitch);

        Quaternion headTargetRot =
            _headInitialLocalRotation *
            Quaternion.Euler(headPitch, headYaw, 0f);

        head.localRotation = Quaternion.Slerp(
            head.localRotation,
            headTargetRot,
            Time.deltaTime * headFollowSpeed
        );

        // 首も少しだけ追従させるなら
        if (neck != null)
        {
            Quaternion neckTargetRot =
                _neckInitialLocalRotation *
                Quaternion.Euler(headPitch * 0.4f, headYaw * 0.4f, 0f);

            neck.localRotation = Quaternion.Slerp(
                neck.localRotation,
                neckTargetRot,
                Time.deltaTime * headFollowSpeed
            );
        }
    }
}