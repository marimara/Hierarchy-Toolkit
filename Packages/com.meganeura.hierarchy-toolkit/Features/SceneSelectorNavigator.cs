using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Meganeura.HierarchyToolkit
{
    internal static class SceneSelectorNavigator
    {
        internal static bool Open(SceneCatalog.Entry entry)
        {
            return TryOpen(entry,
                (guid, path) => AssetDatabase.GUIDToAssetPath(guid) == path
                    && AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null,
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo,
                path => EditorSceneManager.OpenScene(path, OpenSceneMode.Single));
        }

        internal static bool TryOpen(SceneCatalog.Entry entry, Func<string, string, bool> isValid,
            Func<bool> confirmSave, Action<string> openSingle)
        {
            if (entry == null || EditorApplication.isPlayingOrWillChangePlaymode || !isValid(entry.Guid, entry.Path)) return false;
            for (var i = 0; i < SceneManager.sceneCount; ++i)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded || scene.path != entry.Path) continue;
                return SceneManager.SetActiveScene(scene);
            }
            if (!confirmSave()) return false;
            openSingle(entry.Path);
            return true;
        }
    }
}
