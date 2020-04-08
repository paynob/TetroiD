using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tetroid {
    public class MainMenu : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.sceneLoaded += this.SceneManager_sceneLoaded;
            SceneManager.LoadScene( 1, LoadSceneMode.Additive );
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= this.SceneManager_sceneLoaded;
        }

        private void SceneManager_sceneLoaded( Scene arg0, LoadSceneMode arg1 )
        {
            Time.timeScale = 0;
        }

        public void StartGame()
        {
            SceneManager.UnloadSceneAsync( 0 );
            Time.timeScale = 1f;
        }
    }
}
