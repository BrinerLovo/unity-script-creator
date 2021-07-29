using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text.RegularExpressions;

public class bl_ScriptNamespaceCreator : EditorWindow
{
    private Color backgroundColor = new Color(0.07058824f, 0.07058824f, 0.07058824f, 1);
    private Color headerColor = new Color(0.1529412f, 0.1529412f, 0.1529412f, 1);
    private float headerHeight = 40;

    private string selectionPath;
    private string selectionCode;
    private string nameSpace = "";
    public bool saveNamespace = true;

    private readonly string[] textFiles = new string[] { ".cs", ".js", ".txt" };
    private Vector2 scroll;
    private List<string> templates = new List<string>();
    private int currentTemplate = 0;
    private int oldTemplate = 0;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        titleContent = new GUIContent("Title");
        minSize = new Vector2(400, 500);
        GetCurrentSelection();

        templates = new List<string>();
        templates.Add("None");
        templates.AddRange(bl_ScriptCreatorSettings.Instance.scriptNamespaces.ToArray());
    }

    /// <summary>
    /// 
    /// </summary>
    void GetCurrentSelection()
    {
        var selection = Selection.activeObject;
        if (selection == null)
        {
            selectionPath = selectionCode = string.Empty;
            return;
        }

        selectionPath = AssetDatabase.GetAssetPath(selection);
        var fileExtension = Path.GetExtension(selectionPath);
        if (!textFiles.Contains(fileExtension))
        {
            Debug.LogError("The selected file is not a text file.");
            return;
        }
        selectionCode = File.ReadAllText(selectionPath);
        nameSpace = GetCurrentNameSpace();
    }

    /// <summary>
    /// 
    /// </summary>
    private string GetCurrentNameSpace()
    {
        if (string.IsNullOrEmpty(selectionCode)) return "";

        string pattern = @"(?i)namespace\s+(.+?)\s+{";
        var matches = Regex.Matches(selectionCode, pattern, RegexOptions.Multiline);
        foreach (Match item in matches)
        {
            return item.Groups[1].ToString();
        }
        return "";
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
        Header();
        Content();
        Footer();
    }

    /// <summary>
    /// 
    /// </summary>
    void Header()
    {
        Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(headerHeight));
        {
            EditorGUI.DrawRect(r, headerColor);
            GUILayout.Space(10);

            GUILayout.FlexibleSpace();
            GUILayout.Space(10);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void Content()
    {
        GUILayout.Label(selectionPath);
        GUILayout.Space(10);
        currentTemplate = EditorGUILayout.Popup("Template", currentTemplate, templates.ToArray());
        if (currentTemplate != oldTemplate)
        {
            if (currentTemplate > 0) nameSpace = templates[currentTemplate];
            oldTemplate = currentTemplate;
        }
        nameSpace = EditorGUILayout.TextField("Namespace", nameSpace);
        GUILayout.Space(10);
        scroll = GUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(selectionCode);
        GUILayout.EndScrollView();
        GUILayout.Space(10);
        saveNamespace = EditorGUILayout.ToggleLeft("Save Namespace", saveNamespace, EditorStyles.toolbarButton);
        if (GUILayout.Button("APPLY", GUILayout.Height(32)))
        {
            ApplyNamespace();
        }
    }

    void ApplyNamespace()
    {
        if (string.IsNullOrEmpty(selectionCode)) return;

        if (selectionCode.Contains("namespace "))
        {
            string current = GetCurrentNameSpace();
            selectionCode = selectionCode.Replace(current, nameSpace);
        }
        else
        {
            int insertionLine = -1;
            var lines = selectionCode.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (!line.TrimStart().StartsWith("using "))
                {
                    insertionLine = i;
                    break;
                }
            }

            if (insertionLine == -1)
            {
                Debug.LogWarning("Opps! can't found the line to insert the namespace :/");
                return;
            }

            lines[insertionLine] += $"\nnamespace {nameSpace}\n{{";

            string final = "";
            foreach (var line in lines)
            {
                if (!line.TrimStart().StartsWith("using ")) final += "\t";
                final += $"{line}\n";
            }
            final += "}";
            selectionCode = final;
        }

        File.WriteAllText(selectionPath, selectionCode);
        AssetDatabase.ImportAsset(selectionPath);

        if (saveNamespace) bl_ScriptCreatorSettings.Instance.AddNamespace(nameSpace);
        Repaint();
    }

    /// <summary>
    /// 
    /// </summary>
    void Footer()
    {

    }

    [MenuItem("Assets/Create/Add Namespace", false, 84)]
    static void Open()
    {
        GetWindow<bl_ScriptNamespaceCreator>();
    }
}