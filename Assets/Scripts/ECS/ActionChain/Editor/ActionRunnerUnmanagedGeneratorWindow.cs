using System;
using UnityEditor;
using UnityEngine;

public sealed class ActionRunnerUnmanagedGeneratorWindow : EditorWindow
{
    [SerializeField] private ActionMapBase actionMap;

    [MenuItem("Tools/Action Chain/Generate Unmanaged Action Runner")]
    private static void Open()
    {
        GetWindow<ActionRunnerUnmanagedGeneratorWindow>("Action Runner Generator");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Generate ActionRunnerUnmanagedSystem", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Pick an ActionMapBase instance. The generator will build managed subaction states in a temporary ECS world, " +
            "rewrite their methods into the unmanaged template, and save the result into ActionRunnerUnmanagedSystem.cs.",
            MessageType.Info);

        actionMap = (ActionMapBase)EditorGUILayout.ObjectField("Action Map", actionMap, typeof(ActionMapBase), true);

        using (new EditorGUI.DisabledScope(actionMap == null))
        {
            if (GUILayout.Button("Generate"))
            {
                Generate();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Template", ActionRunnerUnmanagedGenerate.TemplateAssetPath);
        EditorGUILayout.LabelField("Output", ActionRunnerUnmanagedGenerate.OutputAssetPath);

        if (GUILayout.Button("Use Selected Action Map"))
        {
            actionMap = Selection.activeObject as ActionMapBase;

            if (actionMap == null && Selection.activeGameObject != null)
            {
                actionMap = Selection.activeGameObject.GetComponent<ActionMapBase>();
            }
        }
    }

    private void Generate()
    {
        try
        {
            ActionRunnerUnmanagedGenerate.GenerateToFile(actionMap);
            Debug.Log($"Generated unmanaged runner: {ActionRunnerUnmanagedGenerate.OutputAssetPath}", actionMap);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorUtility.DisplayDialog("Generation failed", exception.Message, "OK");
        }
    }
}
