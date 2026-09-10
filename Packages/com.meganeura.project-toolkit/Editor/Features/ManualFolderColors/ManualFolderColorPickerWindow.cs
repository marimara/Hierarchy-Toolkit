using System;
using UnityEditor;
using UnityEngine;

namespace Meganeura.ProjectToolkit
{
    internal sealed class ManualFolderColorPickerWindow : EditorWindow
    {
        private static readonly GUIContent ColorLabel = new GUIContent("Folder color");
        private static readonly Vector2 WindowSize = new Vector2(280f, 86f);

        private Color color;
        private Action<Color> apply;

        internal static void Show(Vector2 screenPosition, Color initialColor, Action<Color> applyColor)
        {
            ManualFolderColorPickerWindow window = CreateInstance<ManualFolderColorPickerWindow>();
            window.titleContent = new GUIContent("Set Color");
            window.color = initialColor;
            window.apply = applyColor;
            window.ShowAsDropDown(new Rect(screenPosition, Vector2.zero), WindowSize);
        }

        private void OnGUI()
        {
            const float margin = 10f;
            Rect colorRect = new Rect(margin, margin, position.width - margin * 2f, EditorGUIUtility.singleLineHeight);
            color = EditorGUI.ColorField(colorRect, ColorLabel, color, true, true, false);

            float buttonY = colorRect.yMax + EditorGUIUtility.standardVerticalSpacing + 8f;
            float buttonWidth = 82f;
            Rect cancelRect = new Rect(position.width - margin - buttonWidth * 2f - 6f, buttonY, buttonWidth, 22f);
            Rect applyRect = new Rect(position.width - margin - buttonWidth, buttonY, buttonWidth, 22f);

            if (GUI.Button(cancelRect, "Cancel"))
            {
                Close();
            }

            if (GUI.Button(applyRect, "Apply"))
            {
                Action<Color> callback = apply;
                Close();
                callback?.Invoke(color);
            }
        }
    }
}
