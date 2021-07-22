using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

namespace Lovatto.EditorTools.ScriptCreator
{
    public class bl_ScriptCreator : EditorWindow
    {
        private string[] templateNames;
        private int selectedTemplate = 0;
        public string className = "NewClass";
        private int lastSelected = -1;
        public string[] tempParameters;
        private string destinationPath;
        public Object destinationFolder;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            minSize = new Vector2(300, 100);
            titleContent = new GUIContent("Script Creator");

            templateNames = bl_ScriptCreatorSettings.Instance.scriptTemplates.Select(x =>
            {
                if (x != null) return x.Name;
                return null;
            }).ToArray();
            CheckSelection();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnSelectionChange()
        {
            Repaint();
        }

        /// <summary>
        /// 
        /// </summary>
        void CheckSelection()
        {
            if (Selection.activeObject == null) return;

            string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            if (path.EndsWith(".cs"))
            {
                className = Path.GetFileNameWithoutExtension(path);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(20);
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawTemplateSelector();
                    GUILayout.Space(4);
                    var lw = EditorGUIUtility.labelWidth;
                    EditorGUIUtility.labelWidth = 100;
                    DrawParameters();
                    EditorGUIUtility.labelWidth = lw;
                }
                GUILayout.Space(20);
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                destinationPath = GetDestinationPath(false);
                destinationPath = $"{destinationPath}\\{className}.cs";
            }

            GUILayout.FlexibleSpace();

            EditorGUI.BeginChangeCheck();
            destinationFolder = EditorGUILayout.ObjectField("Folder", destinationFolder, typeof(Object), true);
            if (EditorGUI.EndChangeCheck())
            {
                var p = AssetDatabase.GetAssetPath(destinationFolder);
                if (!Directory.Exists(p)) destinationFolder = null;

                destinationPath = GetDestinationPath(false);
                destinationPath = $"{destinationPath}/{className}.cs";
            }

            EditorGUILayout.LabelField(destinationPath, EditorStyles.miniLabel);
            if (GUILayout.Button("Create"))
            {
                CreateScript();
                Close();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DrawTemplateSelector()
        {
            if (templateNames != null && templateNames.Length > 0)
            {
                selectedTemplate = EditorGUILayout.Popup("Template", selectedTemplate, templateNames, EditorStyles.toolbarPopup);
            }
            if (lastSelected != selectedTemplate)
            {
                BuildParameters();
                lastSelected = selectedTemplate;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void DrawParameters()
        {
            className = EditorGUILayout.TextField("Class Name", className);

            if (CurrentTemplate.Parameters == null || CurrentTemplate.Parameters.Length <= 0) return;
            Object tempRef = null;
            for (int i = 0; i < CurrentTemplate.Parameters.Length; i++)
            {
                var param = CurrentTemplate.Parameters[i];

                EditorGUILayout.BeginHorizontal();
                tempParameters[i] = EditorGUILayout.TextField(param.Name, tempParameters[i]);
                if (param.AllowDragReference)
                {
                    tempRef = EditorGUILayout.ObjectField(tempRef, typeof(Object), true, GUILayout.Width(55));
                    if(tempRef != null)
                    {
                        tempParameters[i] = tempRef.name;
                        tempRef = null;
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void CreateScript()
        {
            if (string.IsNullOrEmpty(className)) return;


            string path = GetDestinationPath();

            if (string.IsNullOrEmpty(path)) return;

            className = className.Replace(".cs", "");
            if (path.EndsWith("/")) path = path.Remove(path.Length - 1, 1);
            string filePath = $"{path}/{className}.cs";

            if (File.Exists(filePath)) { Debug.Log("A script with this name already exist in this path."); return; }

            string templateText = bl_ScriptCreatorSettings.Instance.scriptTemplates[selectedTemplate].TextAsset.text;
            templateText = ParseTemplate(templateText);
            using (StreamWriter outfile = new StreamWriter(filePath))
            {
                outfile.Write(templateText);
            }
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string ParseTemplate(string template)
        {
            if (string.IsNullOrEmpty(template)) return template;

            string parsed = template.Replace("#SCRIPTNAME#", className);
            if (CurrentTemplate.Parameters != null && CurrentTemplate.Parameters.Length > 0)
            {
                for (int i = 0; i < CurrentTemplate.Parameters.Length; i++)
                {
                    parsed = parsed.Replace(CurrentTemplate.Parameters[i].Placeholder, tempParameters[i]);
                }
            }
            return parsed;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private string GetDestinationPath(bool forced = true)
        {
            string path = Selection.activeObject == null ? "" : AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
            if(destinationFolder != null)
            {
                path = AssetDatabase.GetAssetPath(destinationFolder);
            }
            if (!string.IsNullOrEmpty(path))
            {
                if (!Directory.Exists(path))
                {
                    if (path.EndsWith(".cs"))
                    {
                        path = Path.GetDirectoryName(path);
                    }
                    else
                        path = GetFolderPath();
                }
            }
            else
            {
                if (forced) path = GetFolderPath();
                else path = "Assets";
            }
            return path;
        }

        /// <summary>
        /// 
        /// </summary>
        void BuildParameters()
        {
            if (CurrentTemplate.Parameters == null || CurrentTemplate.Parameters.Length <= 0) return;

            tempParameters = new string[CurrentTemplate.Parameters.Length];
            for (int i = 0; i < tempParameters.Length; i++)
            {
                tempParameters[i] = CurrentTemplate.Parameters[i].Default;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        string GetFolderPath()
        {
            return EditorUtility.OpenFolderPanel("Select folder", "Assets/", "Assets");
        }

        private bl_ScriptCreatorSettings.Template CurrentTemplate
        {
            get { return bl_ScriptCreatorSettings.Instance.scriptTemplates[selectedTemplate]; }
        }

        [MenuItem("Assets/Create/Create Script", false, 82)]
        public static void Open()
        {
            GetWindow<bl_ScriptCreator>(true);
        }
    }
}