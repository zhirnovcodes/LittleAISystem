using UnityEditor;
using UnityEngine;
using Unity.Mathematics;

[CustomPropertyDrawer(typeof(HermiteCurveNormalizedAttribute))]
public class HermiteCurveNormalizedDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Calculate the width for buttons (let's use 15% of total width)
        float buttonsWidth = position.width * 0.15f;
        float buttonWidth = buttonsWidth / 3f;
        float curveWidth = position.width - buttonsWidth - 5f; // 5f for spacing

        // Create AnimationCurve from HermiteCurve
        AnimationCurve animCurve = ConvertToAnimationCurve(property);

        // Draw the curve field
        Rect curveRect = new Rect(position.x, position.y, curveWidth, position.height);
        EditorGUI.BeginChangeCheck();
        AnimationCurve newCurve = EditorGUI.CurveField(curveRect, label, animCurve);
        if (EditorGUI.EndChangeCheck())
        {
            // Ensure the curve has exactly 2 keys
            if (newCurve.length == 2)
            {
                ConvertFromAnimationCurve(property, newCurve);
            }
        }

        // Draw the buttons
        float buttonX = position.x + curveWidth + 5f;
        Rect button1Rect = new Rect(buttonX, position.y, buttonWidth, position.height);
        Rect button2Rect = new Rect(buttonX + buttonWidth, position.y, buttonWidth, position.height);
        Rect button3Rect = new Rect(buttonX + buttonWidth * 2, position.y, buttonWidth, position.height);

        // Button 1: Normalize time (|| button)
        if (GUI.Button(button1Rect, "||"))
        {
            NormalizeTime(property);
        }

        // Button 2: Clamp values (= button)
        if (GUI.Button(button2Rect, "="))
        {
            ClampValues(property);
        }

        // Button 3: Revert horizontally (<- .-> button)
        if (GUI.Button(button3Rect, "<-->"))
        {
            RevertHorizontally(property);
        }

        EditorGUI.EndProperty();
    }

    private AnimationCurve ConvertToAnimationCurve(SerializedProperty property)
    {
        // Get points and tangents
        SerializedProperty pointsProp = property.FindPropertyRelative("points");
        SerializedProperty tangentsProp = property.FindPropertyRelative("tangents");

        float x0 = pointsProp.FindPropertyRelative("x").floatValue;
        float y0 = pointsProp.FindPropertyRelative("y").floatValue;
        float x1 = pointsProp.FindPropertyRelative("z").floatValue;
        float y1 = pointsProp.FindPropertyRelative("w").floatValue;
        float outTan = tangentsProp.FindPropertyRelative("x").floatValue;
        float inTan = tangentsProp.FindPropertyRelative("y").floatValue;

        AnimationCurve curve = new AnimationCurve();
        Keyframe key0 = new Keyframe(x0, y0, 0, outTan);
        Keyframe key1 = new Keyframe(x1, y1, inTan, 0);
        curve.AddKey(key0);
        curve.AddKey(key1);

        return curve;
    }

    private void ConvertFromAnimationCurve(SerializedProperty property, AnimationCurve curve)
    {
        if (curve.length != 2) return;

        SerializedProperty pointsProp = property.FindPropertyRelative("points");
        SerializedProperty tangentsProp = property.FindPropertyRelative("tangents");

        Keyframe key0 = curve[0];
        Keyframe key1 = curve[1];

        pointsProp.FindPropertyRelative("x").floatValue = key0.time;
        pointsProp.FindPropertyRelative("y").floatValue = key0.value;
        pointsProp.FindPropertyRelative("z").floatValue = key1.time;
        pointsProp.FindPropertyRelative("w").floatValue = key1.value;
        tangentsProp.FindPropertyRelative("x").floatValue = key0.outTangent;
        tangentsProp.FindPropertyRelative("y").floatValue = key1.inTangent;

        property.serializedObject.ApplyModifiedProperties();
    }

    private void NormalizeTime(SerializedProperty property)
    {
        SerializedProperty pointsProp = property.FindPropertyRelative("points");
        
        // Set x0 = 0 and x1 = 1
        pointsProp.FindPropertyRelative("x").floatValue = 0f;
        pointsProp.FindPropertyRelative("z").floatValue = 1f;

        property.serializedObject.ApplyModifiedProperties();
    }

    private void ClampValues(SerializedProperty property)
    {
        SerializedProperty pointsProp = property.FindPropertyRelative("points");
        
        // Clamp y0 and y1 between 0 and 1
        float y0 = pointsProp.FindPropertyRelative("y").floatValue;
        float y1 = pointsProp.FindPropertyRelative("w").floatValue;

        pointsProp.FindPropertyRelative("y").floatValue = Mathf.Clamp01(y0);
        pointsProp.FindPropertyRelative("w").floatValue = Mathf.Clamp01(y1);

        property.serializedObject.ApplyModifiedProperties();
    }

    private void RevertHorizontally(SerializedProperty property)
    {
        SerializedProperty pointsProp = property.FindPropertyRelative("points");
        SerializedProperty tangentsProp = property.FindPropertyRelative("tangents");

        // Get current values
        float x0 = pointsProp.FindPropertyRelative("x").floatValue;
        float y0 = pointsProp.FindPropertyRelative("y").floatValue;
        float x1 = pointsProp.FindPropertyRelative("z").floatValue;
        float y1 = pointsProp.FindPropertyRelative("w").floatValue;
        float outTan = tangentsProp.FindPropertyRelative("x").floatValue;
        float inTan = tangentsProp.FindPropertyRelative("y").floatValue;

        // Swap positions and values
        pointsProp.FindPropertyRelative("x").floatValue = x0;
        pointsProp.FindPropertyRelative("y").floatValue = y1;
        pointsProp.FindPropertyRelative("z").floatValue = x1;
        pointsProp.FindPropertyRelative("w").floatValue = y0;

        // Reverse and swap tangents
        tangentsProp.FindPropertyRelative("x").floatValue = -inTan;
        tangentsProp.FindPropertyRelative("y").floatValue = -outTan;

        property.serializedObject.ApplyModifiedProperties();
    }
}

