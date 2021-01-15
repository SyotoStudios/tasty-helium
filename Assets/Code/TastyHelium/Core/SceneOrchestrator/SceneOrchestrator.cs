using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TastyHelium
{
    public static class SceneOrchestrator
    {
        // TODO: Implement Live Progress Feedback
        // The general consensus is that the progress Unity provides only
        // accounts for loading resources referenced in the inspector.
        // Inspector resources are being loaded while the progress is
        // less-than or equal-to 0.9. Once the progress hits 0.9, the scene
        // is loaded, and your Awake functions call. Therefore, if you have
        // a long-running Awake, the scene will appear to load fast as the
        // progress jumps from 0.0 to 0.9, but will freeze at 0.9 until Awake
        // finishes.
        // The probable solution is to map the unity-provided progress from
        // 0-0.9 to 0-0.5. This covers inspector resources. Then map your
        // own progress to the remaining 0.5-1.0. Rather than tracking Awake,
        // consider tracking SceneEntryPoint.OnSceneEnter().
        // Thanks Unity, (╯°□°）╯︵ ┻━┻ 🙃
        // Read more: https://gamedev.stackexchange.com/a/169981

        // Progess is only implemented partially as described above.
        // Unity-provided progress is entirely ignored.
        public static async Task LoadScene(LoadSceneMode mode, SceneReference sceneRef, Action<LoadingBatch.LoadProgress> progressCallback = null)
        {
            // Collect the scenes that need to be unloaded if mode is Single.
            List<Scene> scenesToClose = new List<Scene>();
            if (mode == LoadSceneMode.Single)
            {
                int sceneCount = SceneManager.sceneCount;
                for (int i = 0; i < sceneCount; i++)
                {
                    scenesToClose.Add(SceneManager.GetSceneAt(i));
                }
            }

            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

            LoadingBatch loadingBatch = new LoadingBatch();
            SceneEntryPoint loadedEntryPoint = null;

            // Load the scene additively regardless of the provided mode.
            // This is to ensure other scenes can be notified prior to being unloaded.
            AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneRef, LoadSceneMode.Additive);
            loadOp.completed += (loadOp) =>
            {
                // Notify and close the old scenes.
                scenesToClose.ForEach(x =>
                {
                    SceneEntryPoint ep = GetComponentInSceneRoot<SceneEntryPoint>(x);
                    Debug.Log($"Exited Scene: {x.name}");
                    ep?.OnSceneExit(); // TODO: await?
                    AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(x);
                });

                tsc.SetResult(true);
            };

            // Wait for Unity to "load" the scene.
            await Task.WhenAll(tsc.Task);

            // Notify the new scene that it has loaded.
            loadedEntryPoint = GetComponentInSceneRoot<SceneEntryPoint>(sceneRef);
            Debug.Log($"Entered Scene: {SceneManager.GetSceneByPath(sceneRef).name}");

            await loadedEntryPoint?.OnSceneEnter(loadingBatch);

            // Wait for the actual loading to take place.
            await loadingBatch.ProcessBatch(progressCallback);

            if (SceneManager.sceneCount == 1)
            {
                await loadedEntryPoint?.OnActivate();
            }

            await loadedEntryPoint?.OnReady();
        }

        public static async Task<T> LoadScene<T>(LoadSceneMode mode, SceneReference sceneRef, Action<LoadingBatch.LoadProgress> progressCallback = null) where T : Component
        {
            await LoadScene(mode, sceneRef, progressCallback);
            return GetComponentInRoot<T>(sceneRef);
        }

        public static async Task MakeActive(SceneReference sceneRef)
        {
            Scene newActiveScene = SceneManager.GetSceneByPath(sceneRef);
            SceneManager.SetActiveScene(newActiveScene);

            int sceneCount = SceneManager.sceneCount;
            for (int i = 0; i < sceneCount; i++)
            {
                Scene currScene = SceneManager.GetSceneAt(i);
                SceneEntryPoint ep = GetComponentInSceneRoot<SceneEntryPoint>(currScene);
                if (currScene == newActiveScene)
                {
                    Debug.Log($"Activated Scene: {currScene.name}");
                    await ep?.OnActivate();
                    continue;
                }
                Debug.Log($"Deactivated Scene: {currScene.name}");
                await ep?.OnDeactivate();
            }
        }

        public static async Task<bool> RequestClosure(SceneReference sceneRef)
        {
            if (!SceneManager.GetSceneByPath(sceneRef).isLoaded)
            {
                Debug.Log("Scene is already unloaded");
                return true;
            }
            if (SceneManager.sceneCount <= 1)
            {
                Debug.Log("Cannot unload last scene");
                return false;
            }

            // Find the next scene to be made active.
            SceneEntryPoint ep = GetComponentInSceneRoot<SceneEntryPoint>(sceneRef);
            if (SceneManager.GetActiveScene().Equals(sceneRef))
            {
                Debug.Log($"Deactivated Scene: {SceneManager.GetSceneByPath(sceneRef).name}");
                await ep?.OnDeactivate();

                int sceneCount = SceneManager.sceneCount;
                for (int i = 0; i < sceneCount; i++)
                {
                    Scene currScene = SceneManager.GetSceneAt(i);
                    if (currScene.path.Equals(sceneRef)) continue;

                    SceneEntryPoint newActiveEP = GetComponentInSceneRoot<SceneEntryPoint>(currScene);
                    SceneManager.SetActiveScene(currScene);
                    Debug.Log($"Activated Scene: {currScene.name}");
                    await newActiveEP?.OnActivate();
                    break;
                }
            }

            Debug.Log($"Exited Scene: {SceneManager.GetSceneByPath(sceneRef).name}");
            await ep?.OnSceneExit();

            TaskCompletionSource<bool> tsc = new TaskCompletionSource<bool>();

            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(sceneRef);
            unloadOp.completed += (unloadOp) =>
            {
                tsc.SetResult(true);
            };

            return await tsc.Task;
        }

        public static T GetComponentInSceneRoot<T>(SceneReference sceneRef) where T : Component
        {
            Scene scene = SceneManager.GetSceneByPath(sceneRef);
            return GetComponentInSceneRoot<T>(scene);
        }

        public static T GetComponentInSceneRoot<T>(Scene scene) where T : Component
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject go in rootObjects)
                {
                    T component = go.GetComponent<T>();
                    if (component != null) return component;
                }
            }

            return null;
        }

        public static T GetComponentInRoot<T>(SceneReference sceneRef) where T : Component
        {
            Scene scene = SceneManager.GetSceneByPath(sceneRef);
            return GetComponentInRoot<T>(scene);
        }

        public static T GetComponentInRoot<T>(Scene scene) where T : Component
        {
            if (scene.IsValid() && scene.isLoaded)
            {
                GameObject[] rootObjects = scene.GetRootGameObjects();
                foreach (GameObject go in rootObjects)
                {
                    T component = go.GetComponentInChildren<T>();
                    if (component != null) return component;
                }
            }

            return null;
        }
    }
}
