using UnityEditor;
using UnityEngine;

namespace TK.Blast.Editor
{
    public static class LevelManagerEditor
    {
        [MenuItem("Game/Set Last Played Level")]
        private static void SetLevelWithInput()
        {
            var currentLevel = LevelManager.ReachedLevelNo;
            var totalLevels = LevelManager.TotalLevelCount;

            var result = EditorInputDialog.Show(
                "Set Last Played Level",
                $"Enter level number (1-{totalLevels}):",
                currentLevel.ToString()
            );

            if (string.IsNullOrEmpty(result)) return;

            if (int.TryParse(result, out var levelNo))
            {
                if (levelNo >= 1 && levelNo <= totalLevels)
                {
                    LevelManager.SetReachedLevel(levelNo);
                    Debug.Log($"Level set to {levelNo}");
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid Level", $"Please enter a level number between 1 and {totalLevels}.", "OK");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid Input", "Please enter a valid number.", "OK");
            }
        }
    }

    // Helper class for input dialog
    public class EditorInputDialog : EditorWindow
    {
        private string _title;
        private string _message;
        private string _inputText;
        private bool _shouldClose;
        private System.Action<string> _onComplete;

        public static string Show(string title, string message, string defaultText = "")
        {
            string result = null;
            var window = CreateInstance<EditorInputDialog>();
            window._title = title;
            window._message = message;
            window._inputText = defaultText;
            window._onComplete = (value) => result = value;
            
            window.titleContent = new GUIContent(title);
            var position = window.position;
            position.width = 300;
            position.height = 100;
            window.position = position;
            
            window.ShowModal();
            
            return result;
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField(_message);
            GUI.SetNextControlName("InputField");
            _inputText = EditorGUILayout.TextField(_inputText);
            
            // Focus the text field
            if (Event.current.type == EventType.Repaint)
            {
                GUI.FocusControl("InputField");
            }
            
            // Handle Enter key
            if (Event.current.type == EventType.KeyDown &&
                (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter))
            {
                _shouldClose = true;
            }

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                _shouldClose = true;
            }

            if (GUILayout.Button("Cancel"))
            {
                _inputText = null;
                _shouldClose = true;
            }

            EditorGUILayout.EndHorizontal();

            if (_shouldClose)
            {
                _onComplete?.Invoke(_inputText);
                Close();
            }
        }
    }
}