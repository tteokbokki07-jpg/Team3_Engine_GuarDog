using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpgradeUI : MonoBehaviour
{
    public TextMeshProUGUI upgradeText;

    // 누적된 선택 번호를 저장
    private readonly List<int> pickedHistory = new List<int>();

    // UpgradeManager에서 전달된 숫자들을 누적하여 텍스트에 표시합니다.
    // 이미 존재하는 숫자는 중복 추가하지 않습니다.
    public void SetPickedNumbers(List<int> picked)
    {
        if (upgradeText == null)
        {
            Debug.LogWarning("[UpgradeUI] upgradeText가 할당되지 않았습니다.");
            return;
        }

        if (picked == null || picked.Count == 0)
            return;

        foreach (var p in picked)
        {
            if (!pickedHistory.Contains(p))
                pickedHistory.Add(p);
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
        var strArr = pickedHistory.ConvertAll(p => p.ToString()).ToArray();
        upgradeText.text = string.Join(", ", strArr);
    }
}
