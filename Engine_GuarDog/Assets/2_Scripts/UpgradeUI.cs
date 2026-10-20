using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[Serializable]
public class UpgradeTextMapping
{
    public int number;
    [TextArea(2, 5)]
    public string text;
}

public class UpgradeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    public GameObject descriptionPanel;

    public TextMeshProUGUI upgradeText;

    [Header("숫자별 표시 텍스트")]
    public List<UpgradeTextMapping> textMappings = new List<UpgradeTextMapping>();

    // 누적된 선택 번호를 저장
    private readonly List<int> pickedHistory = new List<int>();

    public void OnPointerEnter(PointerEventData eventData)
    {
        descriptionPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        descriptionPanel.SetActive(false);
    }

    // UpgradeManager에서 전달된 숫자를 누적합니다.
    public void SetPickedNumbers(List<int> picked)
    {
        if (upgradeText == null)
        {
            Debug.LogWarning("[UpgradeUI] upgradeText가 할당되지 않았습니다.");
            return;
        }

        if (picked == null || picked.Count == 0)
            return;

        foreach (int number in picked)
        {
            if (!pickedHistory.Contains(number))
                pickedHistory.Add(number);
        }

        UpdateText();
    }

    // 누적 목록을 초기화하고 UI를 비웁니다.
    public void ClearPickedNumbers()
    {
        pickedHistory.Clear();

        if (upgradeText != null)
            upgradeText.text = "";
    }

    private void UpdateText()
    {
        List<string> displayTexts = new List<string>();

        foreach (int number in pickedHistory)
        {
            UpgradeTextMapping mapping = textMappings.Find(
                item => item.number == number
            );

            if (mapping != null && !string.IsNullOrEmpty(mapping.text))
            {
                displayTexts.Add(mapping.text);
            }
            else
            {
                // 매핑되지 않은 숫자는 기존 숫자를 표시
                displayTexts.Add(number.ToString());
            }
        }

        upgradeText.text = string.Join("\n", displayTexts);
    }
}
