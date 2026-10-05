using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight;
    public bool isDay = true;

    [Tooltip("보간 속도(클수록 빠름)")]
    public float transitionSpeed = 1f;

    private Color targetColor;
    private float targetIntensity;

    private void Start()
    {
        if (directionalLight != null)
        {
            targetColor = directionalLight.color;
            targetIntensity = directionalLight.intensity;
        }
    }

    private void Update()
    {
        if (directionalLight == null) return;

        // 현재 값에서 타깃 값으로 서서히 보간
        directionalLight.color = Color.Lerp(directionalLight.color, targetColor, Time.deltaTime * transitionSpeed);
        directionalLight.intensity = Mathf.MoveTowards(directionalLight.intensity, targetIntensity, Time.deltaTime * transitionSpeed);
    }

    public void UpdateDay()
    {
        if (directionalLight == null) return;

        // 즉시 적용하지 않고 타깃 값만 변경 => Update에서 서서히 변경됨
        targetColor = Color.white;
        targetIntensity = 1f;
    }
    public void UpdateNight()
    {
        if (directionalLight == null) return;
     
        targetColor = Color.blue;
        targetIntensity = 0.2f;
    }

}