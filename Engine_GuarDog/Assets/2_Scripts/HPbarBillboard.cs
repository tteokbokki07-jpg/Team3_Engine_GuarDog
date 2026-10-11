using UnityEngine;

public class HPbarBillboard : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void LateUpdate()
    {
        if (mainCamera == null) return;

        transform.forward = mainCamera.transform.forward;
    }
}
