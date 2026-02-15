using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AnimalStats))]
public class AnimalStatsDrawer : PropertyDrawer
{
    private bool Foldout = true;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Get the Stats property (float4x2)
        SerializedProperty statsProperty = property.FindPropertyRelative("Stats");

        // Calculate rects
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // We need to access the float4x2 columns
        SerializedProperty c0 = statsProperty.FindPropertyRelative("c0");
        SerializedProperty c1 = statsProperty.FindPropertyRelative("c1");

        // Draw foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        Foldout = EditorGUI.Foldout(foldoutRect, Foldout, label, true);

        if (Foldout)
        {
            // Increase indent for child properties
            EditorGUI.indentLevel++;

            // Start drawing fields below the foldout
            Rect fieldRect = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

            // Draw all stats using the helper method
            DrawStat(c0, "x", "Energy", ref fieldRect, lineHeight, spacing);
            DrawStat(c0, "y", "Fullness", ref fieldRect, lineHeight, spacing);
            DrawStat(c0, "z", "Toilet", ref fieldRect, lineHeight, spacing);
            DrawStat(c0, "w", "Social", ref fieldRect, lineHeight, spacing);
            DrawStat(c1, "x", "Safety", ref fieldRect, lineHeight, spacing);
            DrawStat(c1, "y", "Health", ref fieldRect, lineHeight, spacing);

            // Restore indent
            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        if (Foldout)
        {
            // Foldout + 6 stat fields
            return (lineHeight + spacing) * 7;
        }
        else
        {
            // Just the foldout line
            return lineHeight;
        }
    }

    private void DrawStat(SerializedProperty parentProperty, string component, string label, ref Rect fieldRect, float lineHeight, float spacing)
    {
        SerializedProperty statProp = parentProperty.FindPropertyRelative(component);
        EditorGUI.BeginChangeCheck();
        float value = EditorGUI.Slider(fieldRect, label, statProp.floatValue, 0f, 100f);
        if (EditorGUI.EndChangeCheck())
        {
            statProp.floatValue = Mathf.Clamp(value, 0f, 100f);
        }
        fieldRect.y += lineHeight + spacing;
    }
}

