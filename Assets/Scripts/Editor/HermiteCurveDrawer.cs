using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(HermiteCurve))]
public class HermiteCurveDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Create AnimationCurve from HermiteCurve
        AnimationCurve animCurve = ConvertToAnimationCurve(property);

        // Draw the curve field
        EditorGUI.BeginChangeCheck();
        AnimationCurve newCurve = EditorGUI.CurveField(position, label, animCurve);
        if (EditorGUI.EndChangeCheck())
        {
            // Ensure the curve has exactly 2 keys
            if (newCurve.length == 2)
            {
                ConvertFromAnimationCurve(property, newCurve);
            }
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
}

