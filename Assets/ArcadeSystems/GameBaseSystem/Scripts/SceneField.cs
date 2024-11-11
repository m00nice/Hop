using System;
using UnityEngine;
using Object = UnityEngine.Object;
using UnityEngine.SceneManagement;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameBaseSystem
{
    [System.Serializable]
    public class SceneField
    {
        public string identifier;
        [SerializeField] private Object sceneAsset;
        [SerializeField] private string sceneName = "";

        public string ScenePath
        {
            get { return sceneName; }
        }

        public string SceneName
        {
            get 
            {
                string[] splits = ScenePath.Split('/');
                return splits[splits.Length-1]; 
            }
        }

        public Scene Scene
        {
            get 
            {
                //Debug.Log("Scene: " + ScenePath);
                return SceneManager.GetSceneByName(ScenePath); 
            }
        }

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneField sceneField)
        {
            return sceneField.ScenePath;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneField))]
    public class SceneFieldPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, GUIContent.none, property);
            var sceneAsset = property.FindPropertyRelative("sceneAsset");
            var sceneName = property.FindPropertyRelative("sceneName");
            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            if (sceneAsset != null)
            {
                EditorGUI.BeginChangeCheck();
                var value = EditorGUI.ObjectField(position, sceneAsset.objectReferenceValue, typeof(SceneAsset), false);
                if (EditorGUI.EndChangeCheck())
                {
                    sceneAsset.objectReferenceValue = value;
                    if (sceneAsset.objectReferenceValue != null)
                    {
                        var scenePath = AssetDatabase.GetAssetPath(sceneAsset.objectReferenceValue);
                        var assetsIndex = scenePath.IndexOf("Assets", StringComparison.Ordinal) + 7;
                        var extensionIndex = scenePath.LastIndexOf(".unity", StringComparison.Ordinal);
                        scenePath = scenePath.Substring(assetsIndex, extensionIndex - assetsIndex);
                        sceneName.stringValue = scenePath;
                        Debug.Log("Scene name: " + scenePath);
                    }
                }
            }
            EditorGUI.EndProperty();
        }
    }
#endif
}