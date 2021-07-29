using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace Lovatto.EditorTools.ScriptCreator
{
    public class TemplateCreator : EditorWindow
    {
        private string windowTitle = "Template Creator";
        private Color backgroundColor = new Color(0.07058824f, 0.07058824f, 0.07058824f, 1);
        private Color headerColor = new Color(0.1529412f, 0.1529412f, 0.1529412f, 1);
        private float headerHeight = 40;
        private float footerHeight = 30;

        public string templateText;
        public string templateName = "";
        private Vector2 scroll;
        private GUIStyle textBoxStyle;
        private List<string> snippets = new List<string>() { "SCRIPTNAME", "MENUITEM", "FILENAME"};
        private TextEditor editorTextArea;
        private List<bl_ScriptCreatorSettings.TemplateParameter> toAddSnippets;
        private int windowID = 0;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent(windowTitle);
            minSize = new Vector2(500, 500);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
            if (textBoxStyle == null) InitGUI();
            Header();
            Content();
            Footer();
        }

        /// <summary>
        /// 
        /// </summary>
        void Header()
        {
            var r = EditorGUILayout.BeginHorizontal(GUILayout.Height(headerHeight * 0.5f));
            {
                //EditorGUI.DrawRect(r, headerColor);
                GUILayout.Space(10);
                GUILayout.Label("Template Name:");
                GUILayout.Space(10);
                templateName = EditorGUILayout.TextField(templateName, GUILayout.Width(200));
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginHorizontal(GUILayout.Height(headerHeight * 0.5f));
            {
               // EditorGUI.DrawRect(r, headerColor);
                GUILayout.Space(10);
                SnipetsList();
                GUILayout.FlexibleSpace();
                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 
        /// </summary>
        void SnipetsList()
        {
            for (int i = 0; i < snippets.Count; i++)
            {
                var sni = $"#{snippets[i]}#";
                if (GUILayout.Button(sni, EditorStyles.miniButton, GUILayout.ExpandHeight(true)))
                {
                    if(editorTextArea != null && editorTextArea.hasSelection)
                    {
                        editorTextArea.ReplaceSelection(sni);
                        templateText = editorTextArea.text;
                        return;
                    }
                    EditorGUIUtility.systemCopyBuffer = sni;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void Content()
        {
            if (windowID == 0)
            {
                var size = textBoxStyle.CalcSize(new GUIContent(templateText));
                size.y += 100;
                scroll = GUILayout.BeginScrollView(scroll);
                GUI.SetNextControlName("EditorTextBox");
                templateText = EditorGUILayout.TextArea(templateText, textBoxStyle, GUILayout.MinHeight(50), GUILayout.Height(size.y));
                editorTextArea = typeof(EditorGUI).GetField("activeEditor", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null) as TextEditor;
                GUILayout.EndScrollView();
            }
            else if (windowID == 1)
            {
                GUILayout.Space(20);
                var r = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
                {
                    GUILayout.Space(10);
                    if (GUILayout.Button("Back")) { windowID = 0; }
                    GUILayout.FlexibleSpace();
                    GUILayout.Space(10);
                }
                EditorGUILayout.EndHorizontal();
                for (int i = 0; i < toAddSnippets.Count; i++)
                {
                    var tas = toAddSnippets[i];
                    tas.Name = EditorGUILayout.TextField("Name", tas.Name);
                    tas.Placeholder = EditorGUILayout.TextField("Mark", tas.Placeholder);
                    tas.Default = EditorGUILayout.TextField("Default", tas.Default);
                    GUILayout.Space(20);

                }
                if (GUILayout.Button("ADD"))
                {
                    if (string.IsNullOrEmpty(templateName))
                    {
                        Debug.Log("Template name can't be empty");
                        return;
                    }
                    var textFile = CreateTemplateFile();

                    var template = new bl_ScriptCreatorSettings.Template();
                    template.Name = templateName;
                    template.Parameters = toAddSnippets.ToArray();
                    template.TextAsset = textFile;

                    bl_ScriptCreatorSettings.Instance.AddTemplate(template);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        TextAsset CreateTemplateFile()
        {
            var localPath = AssetDatabase.GetAssetPath(bl_ScriptCreatorSettings.Instance.scriptTemplates[0].TextAsset);
            localPath = Path.GetDirectoryName(localPath);
            localPath = $"{localPath}/{templateName}.txt";

            File.WriteAllText(localPath, templateText);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var asset = AssetDatabase.LoadAssetAtPath(localPath, typeof(TextAsset)) as TextAsset;
            return asset;
        }

        /// <summary>
        /// 
        /// </summary>
        void Footer()
        {
            Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(footerHeight));
            {
                EditorGUI.DrawRect(r, headerColor);
                GUILayout.Space(10);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Create", GUILayout.ExpandHeight(true)))
                {
                  toAddSnippets = FindAllSnippetsInText();
                    windowID = 1;
                }
                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 
        /// </summary>
        List<bl_ScriptCreatorSettings.TemplateParameter> FindAllSnippetsInText()
        {
            var list = new List<bl_ScriptCreatorSettings.TemplateParameter>();
            var matches = Regex.Matches(templateText, @"#(.*)#", RegexOptions.Multiline);
            foreach (Match item in matches)
            {
                if (list.Exists(x => x.Name == item.Groups[1].ToString())) continue;

                list.Add(new bl_ScriptCreatorSettings.TemplateParameter()
                {
                    Name = item.Groups[1].ToString(),
                    Placeholder = item.Groups[0].ToString()
                });
            }
            return list;
        }

        /// <summary>
        /// 
        /// </summary>
        void InitGUI()
        {
            textBoxStyle = new GUIStyle(EditorStyles.textArea);
            textBoxStyle.wordWrap = false;
        }

        [MenuItem("Lovatto/Tools/ScriptCreator/Template")]
        static void Open()
        {
            GetWindow<TemplateCreator>();
        }
    }
}