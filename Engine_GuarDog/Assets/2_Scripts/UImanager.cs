using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public GameObject PausePanel;
    public GameObject ClearPanel;
    public bool isGaming = true;
    public bool isPaused = false;

    private PlayerController playerController;

    private void Awake()
    {
        // 씬에서 PlayerController 찾기 (없다면 null 허용)
        playerController = FindObjectOfType<PlayerController>();
    }

    [System.Obsolete]
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false && isGaming == true)
        {
            Time.timeScale = 0;
            PausePanel.SetActive(true);
            Debug.Log("ESC");
            isPaused = true;

            // 커서 보이기 및 잠금 해제
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // 카메라 입력 비활성화
            if (playerController != null) playerController.SetLookEnabled(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused == true && isGaming == true)
        {
            Time.timeScale = 1;
            PausePanel.SetActive(false);
            Debug.Log("ESC_down");
            isPaused = false;

            // 커서 숨기기 및 잠금
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // 카메라 입력 활성화
            if (playerController != null) playerController.SetLookEnabled(true);
        }

    }
    public void GamePause()
    {
        Time.timeScale = 1;
        PausePanel.SetActive(false);
        isPaused = false;

        // 커서 보이기 및 잠금 해제
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;        

        if (playerController != null) playerController.SetLookEnabled(true);
    }
    public void GameStopNoPanel()
    {
        Time.timeScale = 0;
        isPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null) playerController.SetLookEnabled(false);
    }
    public void GamePauseNoPanel()
    {
        Time.timeScale = 1;
        isPaused = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null) playerController.SetLookEnabled(true);
    }
    public void Quit()
    {
        Application.Quit();
    }
    public void Title()
    {
        SceneFade.Instance.ChangeScene("Title");
        Time.timeScale = 1;
        isPaused = false;

        // 커서 보이기 및 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void Map()
    {
        SceneFade.Instance.ChangeScene("Map");
        Time.timeScale = 1;
        isPaused = false;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        //if (playerController != null) playerController.SetLookEnabled(true);
    }
    public void GameOver()
    {
        SceneFade.Instance.ChangeScene("GameOver");
        Time.timeScale = 1;
        isPaused = false;

        // 커서 보이기 및 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void GameClear()
    {
        Time.timeScale = 0;
        ClearPanel.SetActive(true);
        isPaused = true;
        isGaming = false;

        // 커서 보이기 및 잠금 해제
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 카메라 입력 비활성화
        if (playerController != null) playerController.SetLookEnabled(false);
    }

    public void CutSceneSFX1()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.homehitSound, 0.35f, 1f); //사운드 재생(음량, 높이)
    }    
    public void CutSceneSFX2()
    {
        SoundManager.Instance.PlaySFX(SoundManager.Instance.swingSound, 0.5f, 1f); //사운드 재생(음량, 높이)
        SoundManager.Instance.PlaySFX(SoundManager.Instance.popSound, 0.25f, 1f); //사운드 재생(음량, 높이)
        SoundManager.Instance.PlaySFX(SoundManager.Instance.hitSound, 0.25f, 1f); //사운드 재생(음량, 높이)
    }


}
