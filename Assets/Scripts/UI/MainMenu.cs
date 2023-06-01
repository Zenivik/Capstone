using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] string SceneToLoad;
    [SerializeField] GameObject Menu;
    [SerializeField] GameObject Dialogue1;

    public void StartGame()
    {
        SceneManager.LoadScene(SceneToLoad);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
