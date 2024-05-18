using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerLoseMenu : MonoBehaviour
{
    public GameObject mainMenuHolder;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()  // Play버튼 누르면 PlayerLose Scene으로 전환
    {
        SceneManager.LoadScene("PlayerLose");
    }

    public void Quit()  // Quit버튼 누르면 종료
    {
        Application.Quit();
    }

    public void MainMenu()      // MainMenu 활성화
    {
        mainMenuHolder.SetActive(true);
    }
}
