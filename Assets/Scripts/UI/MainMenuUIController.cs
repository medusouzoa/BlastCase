using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI
{
    public class MainMenuUIController : MonoBehaviour
    {
        public void OpenGameScene()
        {
            SceneManager.LoadScene("Scenes/ObstacleGameScene");
        }
    }
}