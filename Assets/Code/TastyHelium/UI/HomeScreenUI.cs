using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace TastyHelium
{
    public class HomeScreenUI : MonoBehaviour
    {
        public Button DanLevelBtn;
        public Button WIP1LevelBtn;
        public Button WIP2LevelBtn;

        public SceneReference DanLevel;
        public SceneReference WIP1Level;
        public SceneReference WIP2Level;

        private async Task OpenLevel(SceneReference scene) {
            await SceneOrchestrator.LoadScene(UnityEngine.SceneManagement.LoadSceneMode.Single, scene);
        }

        private void OnEnable() {
            // TODO: Await open level
            DanLevelBtn.onClick.AddListener(() => OpenLevel(DanLevel).ConfigureAwait(false));
            WIP1LevelBtn.onClick.AddListener(() => OpenLevel(WIP1Level).ConfigureAwait(false));
            WIP2LevelBtn.onClick.AddListener(() => OpenLevel(WIP2Level).ConfigureAwait(false));
        }

        private void OnDisable() {
            DanLevelBtn.onClick.RemoveAllListeners();
            WIP1LevelBtn.onClick.RemoveAllListeners();
            WIP2LevelBtn.onClick.RemoveAllListeners();
        }
    }
}
