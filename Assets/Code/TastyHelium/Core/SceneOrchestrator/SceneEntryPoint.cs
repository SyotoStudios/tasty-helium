using System.Threading.Tasks;
using UnityEngine;

namespace TastyHelium
{
    public class SceneEntryPoint : MonoBehaviour
    {
        public SceneReference SceneReference;
        public Transform SceneRoot;

        // Called once Unity has finished "loading" the scene.
        public virtual async Task OnSceneEnter(LoadingBatch batch) {
            await Task.CompletedTask;
        }

        // Called right before the scene is unloaded.
        public virtual async Task OnSceneExit() {
            await Task.CompletedTask;
        }

        // Called after the scene has fully loaded.
        public virtual async Task OnReady()
        {
            // "Start" the scene.
            if (SceneRoot != null) {
                SceneRoot?.gameObject.SetActive(true);
            }

            await Task.CompletedTask;
        }

        // Called right after the scene is marked as the active scene.
        public virtual async Task OnActivate() {
            await Task.CompletedTask;
        }

        // Called right after the scene's active status is removed.
        public virtual async Task OnDeactivate() {
            await Task.CompletedTask;
        }
    }
}