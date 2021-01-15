using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TastyHelium
{
    public class PreloadEntryPoint : SceneEntryPoint
    {
        public SceneReference NextScene;

        // Note: This is an unconventional but neccessary use of Awake
        // within a SceneEntryPoint. Preload is the first scene to load,
        // therefore Awake is needed to kick-off of the game.
        private async void Awake() {
            await OnReady();
        }
        
        public override async Task OnReady()
        {
            await base.OnReady();

            await SceneOrchestrator.LoadScene(UnityEngine.SceneManagement.LoadSceneMode.Single, NextScene);
        }
    }
}
