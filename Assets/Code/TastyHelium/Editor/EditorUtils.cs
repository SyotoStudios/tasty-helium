using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TastyHelium.Editor
{
    [InitializeOnLoad]
    static class EditorUtils
    {
        // Properties are remembered as editor preferences.
        private const string cEditorPrefLoadMasterOnPlay = "EditorUtils.LoadMasterOnPlay";
        private const string cEditorPrefMasterScene = "EditorUtils.MasterScene";
        private const string cEditorPrefPreviousScene = "EditorUtils.PreviousScene";
        private const string cEditorPrefBetterCanvas = "EditorUtils.BetterCanvas";

        private static bool LoadMasterOnPlay
        {
            get { return EditorPrefs.GetBool(cEditorPrefLoadMasterOnPlay, true); }
            set { EditorPrefs.SetBool(cEditorPrefLoadMasterOnPlay, value); }
        }

        private static string MasterScene
        {
            get { return EditorPrefs.GetString(cEditorPrefMasterScene, "Assets/Scenes/_preload.unity"); }
            set { EditorPrefs.SetString(cEditorPrefMasterScene, value); }
        }

        private static string PreviousScene
        {
            get { return EditorPrefs.GetString(cEditorPrefPreviousScene, EditorSceneManager.GetActiveScene().path); }
            set { EditorPrefs.SetString(cEditorPrefPreviousScene, value); }
        }

        private static bool BetterCanvas
        {
            get { return EditorPrefs.GetBool(cEditorPrefBetterCanvas, true); }
            set { EditorPrefs.SetBool(cEditorPrefBetterCanvas, value); }
        }

        // [InitializeOnLoad] above makes sure this gets executed.
        static EditorUtils()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            Selection.selectionChanged += OnSelectionChanged;
        }

        [ShortcutAttribute("Syoto Studios/Load Level Container", KeyCode.F1)]
        [MenuItem("Syoto Studios/Load Level Container")]
        private static void LoadLevelContainer()
        {
            if (!EditorSceneManager.GetSceneByName("LevelContainer").IsValid())
            {
                EditorSceneManager.OpenScene("Assets/Data/Scenes/LevelContainer.unity", OpenSceneMode.Additive);
            }
            else
            {
                EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByName("LevelContainer"), true);
            }
        }

        [ShortcutAttribute("Syoto Studios/Open Code Folder", KeyCode.F2)]
        [MenuItem("Syoto Studios/Open Code Folder")]
        private static void OpenCodeFolder()
        {
            PingFolderOrFirstAsset("Assets/Code/ConcernedLithium");
        }

        [ShortcutAttribute("Syoto Studios/Open Scenes Folder", KeyCode.F3)]
        [MenuItem("Syoto Studios/Open Scenes Folder")]
        private static void OpenScenesFolder()
        {
            PingFolderOrFirstAsset("Assets/Data/Scenes");
        }

        [ShortcutAttribute("Syoto Studios/Open Prefabs Folder", KeyCode.F4)]
        [MenuItem("Syoto Studios/Open Prefabs Folder")]
        private static void OpenPrefabsFolder()
        {
            PingFolderOrFirstAsset("Assets/Data/Prefabs");
        }

        [MenuItem("Syoto Studios/Better Canvas/Enable Better Canvas", true)]
        private static bool ShowEnableBetterCanvas()
        {
            return !BetterCanvas;
        }
        [MenuItem("Syoto Studios/Better Canvas/Enable Better Canvas")]
        private static void EnableEnableBetterCanvas()
        {
            BetterCanvas = true;
        }

        [MenuItem("Syoto Studios/Better Canvas/Disable Better Canvas", true)]
        private static bool ShowDisableBetterCanvas()
        {
            return BetterCanvas;
        }
        [MenuItem("Syoto Studios/Better Canvas/Disable Better Canvas")]
        private static void DisableDisableBetterCanvas()
        {
            BetterCanvas = false;
        }

        // Menu items to select the "master" scene and control whether or not to load it.
        [MenuItem("Syoto Studios/Scene Autoload/Select Master Scene...")]
        private static void SelectMasterScene()
        {
            string masterScene = EditorUtility.OpenFilePanel("Select Master Scene", Application.dataPath, "unity");
            masterScene = masterScene.Replace(Application.dataPath, "Assets");
            if (!string.IsNullOrEmpty(masterScene))
            {
                MasterScene = masterScene;
                LoadMasterOnPlay = true;
            }
        }

        [MenuItem("Syoto Studios/Scene Autoload/Load Master On Play", true)]
        private static bool ShowLoadMasterOnPlay()
        {
            return !LoadMasterOnPlay;
        }
        [MenuItem("Syoto Studios/Scene Autoload/Load Master On Play")]
        private static void EnableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = true;
        }

        [MenuItem("Syoto Studios/Scene Autoload/Don't Load Master On Play", true)]
        private static bool ShowDontLoadMasterOnPlay()
        {
            return LoadMasterOnPlay;
        }
        [MenuItem("Syoto Studios/Scene Autoload/Don't Load Master On Play")]
        private static void DisableLoadMasterOnPlay()
        {
            LoadMasterOnPlay = false;
        }

        private static void PingFolderOrFirstAsset(string folderPath)
        {
            string path = GetFirstAssetPathInFolder(folderPath);
            if (string.IsNullOrEmpty(path))
                path = folderPath;
            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(obj);
        }

        private static string GetFirstAssetPathInFolder(string folder, bool includeFolders = true)
        {
            if (includeFolders)
            {
                string path = GetFirstValidAssetPath(System.IO.Directory.GetDirectories(folder));
                if (path != null)
                    return path;
            }
            return GetFirstValidAssetPath(System.IO.Directory.GetFiles(folder));
        }

        private static string GetFirstValidAssetPath(string[] paths)
        {
            for (int i = 0; i < paths.Length; ++i)
            {
                if (!string.IsNullOrEmpty(AssetDatabase.AssetPathToGUID(paths[i])))
                    return paths[i];
            }
            return null;
        }
          

        private static void OnSelectionChanged()
        {
            if (!BetterCanvas) return;
            GameObject selectGo = Selection.activeGameObject;
            if (!selectGo) return;

            if (selectGo.GetComponent<RectTransform>()) {
                SceneView.lastActiveSceneView.sceneViewState.showSkybox = false;
                SceneView.lastActiveSceneView.sceneViewState.showFog = false;
            } else {
                SceneView.lastActiveSceneView.sceneViewState.showSkybox = true;
                SceneView.lastActiveSceneView.sceneViewState.showFog = true;
            }
        }

        // Play mode change callback handles the scene load/reload.
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (!LoadMasterOnPlay)
            {
                return;
            }

            if (!EditorApplication.isPlaying && EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed play -- autoload master scene.
                PreviousScene = EditorSceneManager.GetActiveScene().path;
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    try
                    {
                        EditorSceneManager.OpenScene(MasterScene);
                    }
                    catch
                    {
                        Debug.LogError(string.Format("error: scene not found: {0}", MasterScene));
                        EditorApplication.isPlaying = false;

                    }
                }
                else
                {
                    // User cancelled the save operation -- cancel play as well.
                    EditorApplication.isPlaying = false;
                }
            }

            // isPlaying check required because cannot OpenScene while playing
            if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // User pressed stop -- reload previous scene.
                try
                {
                    EditorSceneManager.OpenScene(PreviousScene);
                }
                catch
                {
                    Debug.LogError(string.Format("error: scene not found: {0}", PreviousScene));
                }
            }
        }
    }
}