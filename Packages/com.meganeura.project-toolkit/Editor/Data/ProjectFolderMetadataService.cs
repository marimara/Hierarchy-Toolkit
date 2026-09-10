using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    [InitializeOnLoad]
    internal static class ProjectFolderMetadataService
    {
        internal const string StoreAssetPath =
            "Assets/Editor/MeganeuraProjectToolkit/ProjectToolkitFolderMetadata.asset";

        private const string EditorFolderPath = "Assets/Editor";
        private const string ToolkitFolderPath = "Assets/Editor/MeganeuraProjectToolkit";
        private static FolderMetadataRepository repository;

        static ProjectFolderMetadataService()
        {
            EditorApplication.delayCall += Initialize;
            AssemblyReloadEvents.beforeAssemblyReload += Shutdown;
            EditorApplication.quitting += Shutdown;
        }

        internal static FolderMetadataRepository Repository
        {
            get
            {
                Initialize();
                return repository;
            }
        }

        private static void Initialize()
        {
            EditorApplication.delayCall -= Initialize;
            if (repository != null)
            {
                return;
            }

            ProjectFolderMetadataStore store = AssetDatabase.LoadAssetAtPath<ProjectFolderMetadataStore>(StoreAssetPath);
            if (store == null)
            {
                EnsureFolder(EditorFolderPath, "Editor");
                EnsureFolder(ToolkitFolderPath, "MeganeuraProjectToolkit");
                store = ScriptableObject.CreateInstance<ProjectFolderMetadataStore>();
                AssetDatabase.CreateAsset(store, StoreAssetPath);
                AssetDatabase.SaveAssetIfDirty(store);
            }

            repository = new FolderMetadataRepository(store);
        }

        private static void EnsureFolder(string path, string folderName)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = path.Substring(0, path.Length - folderName.Length - 1);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static void Shutdown()
        {
            EditorApplication.delayCall -= Initialize;
            repository?.Dispose();
            repository = null;
        }
    }
}
