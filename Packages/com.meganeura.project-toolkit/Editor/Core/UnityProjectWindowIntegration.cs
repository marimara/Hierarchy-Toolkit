using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal sealed class UnityProjectWindowIntegration : IProjectWindowIntegration
    {
        private bool isDisposed;

        internal UnityProjectWindowIntegration()
        {
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
            EditorApplication.projectChanged += OnProjectChanged;
        }

        public event Action<string, Rect> ItemGUI;

        public event Action ProjectChanged;

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemGUI;
            EditorApplication.projectChanged -= OnProjectChanged;
            ItemGUI = null;
            ProjectChanged = null;
        }

        private void OnProjectWindowItemGUI(string assetGuid, Rect rect)
        {
            ItemGUI?.Invoke(assetGuid, rect);
        }

        private void OnProjectChanged()
        {
            ProjectChanged?.Invoke();
        }
    }
}
