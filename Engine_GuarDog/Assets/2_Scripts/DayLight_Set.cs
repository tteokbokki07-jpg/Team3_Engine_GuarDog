using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    public Light directionalLight;
    public bool isDay = false;

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
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isDay = !isDay;
            if (isDay)
            {
                UpdateDay();
            }
            else
            {
                UpdateNight();
            }
        }
    }

    public void UpdateDay()
    {
        if (directionalLight == null) return;

        // 즉시 적용하지 않고 타깃 값만 변경 => Update에서 서서히 변경됨
        targetColor = Color.white;
        targetIntensity = 3f;
    }
    public void UpdateNight()
    {
        if (directionalLight == null) return;

        targetColor = new Color32(8, 136, 254, 255); 
        targetIntensity = 1f;
    }

}