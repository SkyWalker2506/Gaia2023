using System;
using PWCommon5;
using UnityEngine;
using UnityEditor;
namespace ProceduralWorlds.GTS
{
    public static class GTSEditorExtensionMethods
    {
        public static bool DrawHeader(this EditorUtils editorUtils, string nameKey, GTSProfileSettings profileSettings, bool helpEnabled, Action resetAction = null, Action removeAction = null)
        {
            Rect backgroundRect = GUILayoutUtility.GetRect(2f, 17f);
            Rect labelRect = backgroundRect;
            labelRect.xMin += 32f;
            labelRect.xMax -= 20f;
            Rect foldoutRect = backgroundRect;
            foldoutRect.y += 2f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;
            Rect toggleRect = backgroundRect;
            toggleRect.x += 16f;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;
            Texture2D menuIcon = PWStyles.paneOptionsIcon;
#if UNITY_2019_3_OR_NEWER
            Rect menuRect = new Rect(labelRect.xMax + 4f, labelRect.y, menuIcon.width, menuIcon.height);
#else
            var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y + 4f, menuIcon.width, menuIcon.height);
#endif

            // Background rect should be full-width
            // backgroundRect.xMin = 0f;
            // backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, PWStyles.headerBackground);

            // Title
            using (new EditorGUI.DisabledScope(!profileSettings.enabled))
            {
                EditorGUI.LabelField(labelRect, editorUtils.GetContent(nameKey), EditorStyles.boldLabel);
            }

            // Help
            editorUtils.InlineHelp(nameKey, helpEnabled);

            // foldout
            // group.serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            {
                profileSettings.isExpanded = GUI.Toggle(foldoutRect, profileSettings.isExpanded, GUIContent.none, EditorStyles.foldout);
            }
            if (EditorGUI.EndChangeCheck())
                GUI.changed = true;
            // group.serializedObject.ApplyModifiedProperties();

            // Active checkbox
            // activeField.serializedObject.Update();
            EditorGUI.BeginChangeCheck();
            {
                profileSettings.enabled = GUI.Toggle(toggleRect, profileSettings.enabled, GUIContent.none, PWStyles.smallTickbox);
            }
            if (EditorGUI.EndChangeCheck())
                GUI.changed = true;
            // activeField.serializedObject.ApplyModifiedProperties();

            // Dropdown menu
            if (resetAction != null || removeAction != null)
            {
                // Dropdown menu icon
                GUI.DrawTexture(menuRect, menuIcon);

                // Handle events
                Event e = Event.current;
                if (e.type == EventType.MouseDown)
                {
                    if (menuRect.Contains(e.mousePosition))
                    {
                        editorUtils.ShowHeaderContextMenu(new Vector2(menuRect.x, menuRect.yMax), resetAction, removeAction);
                        e.Use();
                    }
                    else if (labelRect.Contains(e.mousePosition))
                    {
                        if (e.button == 0)
                        {
                            profileSettings.isExpanded = !profileSettings.isExpanded;
                        }
                        else
                            editorUtils.ShowHeaderContextMenu(e.mousePosition, resetAction, removeAction);
                        e.Use();
                    }
                }
            }
            return profileSettings.isExpanded;
        }
        public static void ShowHeaderContextMenu(this EditorUtils editorUtils, Vector2 position, Action resetAction = null, Action removeAction = null)
        {
            GenericMenu menu = new GenericMenu();
            if (resetAction != null)
                menu.AddItem(editorUtils.GetContent("Reset"), false, () => resetAction());
            if (removeAction != null)
                menu.AddItem(editorUtils.GetContent("Remove"), false, () => removeAction());
            if (menu.GetItemCount() > 0)
                menu.DropDown(new Rect(position, Vector2.zero));
        }
    }
}