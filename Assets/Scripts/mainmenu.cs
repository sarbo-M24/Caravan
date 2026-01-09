using UnityEngine;

public class mainmenu : MonoBehaviour
{
    
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
    }
}
