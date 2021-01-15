using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

namespace TastyHelium.Editor
{
    [InitializeOnLoad]
    public static class MissingSceneEntryPointWarning
    {
        static MissingSceneEntryPointWarning()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            if (EditorApplication.isPlaying) return;
            ControlSceneRoot(scene, false);
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (EditorApplication.isPlaying) return;
            ControlSceneRoot(scene, true);
            EnsureEntryPoint(scene);
        }

        private static void ControlSceneRoot(Scene scene, bool enabled)
        {
            GameObject root = scene.GetRootGameObjects().FirstOrDefault(x => x.name.Equals("ROOT"));
            if (root)
            {
                root.SetActive(enabled);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        private static void EnsureEntryPoint(Scene scene)
        {
            SceneEntryPoint ep = null;
            GameObject[] rootObjects = scene.GetRootGameObjects();
            foreach (GameObject go in rootObjects)
            {
                ep = go.GetComponent<SceneEntryPoint>();
                if (ep) break;
            }

            if (!ep)
            {
                bool result = EditorUtility.DisplayDialog("Invalid Scene - Syoto Studios", "All scenes are required to have a SceneEntryPoint located on a root GameObject.", "Add SceneEntryPoint", "Dismiss; I know what I am doing");
                if (result)
                {
                    GameObject entryPointGO = new GameObject("ENTRY_POINT", typeof(SceneEntryPoint));

                    SceneEntryPoint entryPoint = entryPointGO.GetComponent<SceneEntryPoint>();
                    if (entryPointGO.scene != scene)
                    {
                        SceneManager.MoveGameObjectToScene(entryPointGO, scene);
                    }

                    entryPoint.SceneReference = new SceneReference { ScenePath = scene.path };
                    entryPointGO.transform.SetAsFirstSibling();

                    EditorSceneManager.MarkSceneDirty(scene);
                }
            }
        }
    }
}