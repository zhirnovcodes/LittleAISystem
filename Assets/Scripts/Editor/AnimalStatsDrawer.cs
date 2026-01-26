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

            // Energy (c0.x)
            SerializedProperty energyProp = c0.FindPropertyRelative("x");
            EditorGUI.BeginChangeCheck();
            float energy = EditorGUI.Slider(fieldRect, "Energy", energyProp.floatValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                energyProp.floatValue = Mathf.Clamp(energy, 0f, 100f);
            }
            fieldRect.y += lineHeight + spacing;

            // Fullness (c0.y)
            SerializedProperty fullnessProp = c0.FindPropertyRelative("y");
            EditorGUI.BeginChangeCheck();
            float fullness = EditorGUI.Slider(fieldRect, "Fullness", fullnessProp.floatValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                fullnessProp.floatValue = Mathf.Clamp(fullness, 0f, 100f);
            }
            fieldRect.y += lineHeight + spacing;

            // Toilet (c0.z)
            SerializedProperty toiletProp = c0.FindPropertyRelative("z");
            EditorGUI.BeginChangeCheck();
            float toilet = EditorGUI.Slider(fieldRect, "Toilet", toiletProp.floatValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                toiletProp.floatValue = Mathf.Clamp(toilet, 0f, 100f);
            }
            fieldRect.y += lineHeight + spacing;

            // Social (c0.w)
            SerializedProperty socialProp = c0.FindPropertyRelative("w");
            EditorGUI.BeginChangeCheck();
            float social = EditorGUI.Slider(fieldRect, "Social", socialProp.floatValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                socialProp.floatValue = Mathf.Clamp(social, 0f, 100f);
            }
            fieldRect.y += lineHeight + spacing;

            // Safety (c1.x)
            SerializedProperty safetyProp = c1.FindPropertyRelative("x");
            EditorGUI.BeginChangeCheck();
            float safety = EditorGUI.Slider(fieldRect, "Safety", safetyProp.floatValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                safetyProp.floatValue = Mathf.Clamp(safety, 0f, 100f);
            }
            fieldRect.y += lineHeight + spacing;

            // Health (c1.y)
            SerializedProperty healthProp = c1.FindPropertyRelative("y");
            EditorGUI.BeginChangeCheck();
            float health = EditorGUI.Slider(fieldRect, "Health", healthProp.floatValue, 0f, 100f);
            if (EditorGUI.EndChangeCheck())
            {
                healthProp.floatValue = Mathf.Clamp(health, 0f, 100f);
            }

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
}

