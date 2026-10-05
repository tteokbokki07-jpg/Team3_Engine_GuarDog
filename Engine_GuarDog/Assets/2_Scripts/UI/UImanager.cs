using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UImanager : MonoBehaviour
{
    public GameObject PausePanel;
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
        if (Input.GetKeyDown(KeyCode.Escape) && isPaused == false)
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
        else if (Input.GetKeyDown(KeyCode.Escape) && isPaused == true)
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
        SceneManager.LoadScene("Title");
        Time.timeScale = 1;
        isPaused = false;

        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;

        //if (playerController != null) playerController.SetLookEnabled(true);
    }

 
}
