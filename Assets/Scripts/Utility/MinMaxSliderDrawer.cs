#if UNITY_EDITOR

//Based off of https://gist.github.com/frarees/9791517#file-minmaxsliderattribute-cs-L13

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MinMaxSliderAttribute))]
public class MinMaxSliderDrawer : PropertyDrawer
{

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{

		if (property.propertyType == SerializedPropertyType.Vector2)
		{
			Vector2 range = property.vector2Value;
			float min = range.x;
			float max = range.y;
			MinMaxSliderAttribute attr = attribute as MinMaxSliderAttribute;
			EditorGUI.BeginChangeCheck();
            EditorGUI.MinMaxSlider(position, label, ref min, ref max, attr.min, attr.max);
            EditorGUI.LabelField(new Rect(position.x + position.width - 200, position.y + 15, 200, 15), min.ToString());
			EditorGUI.LabelField(new Rect(position.x + position.width - 60, position.y + 15, 60, 15), max.ToString());

			if (EditorGUI.EndChangeCheck())
			{
				range.x = min;
				range.y = max;
				property.vector2Value = range;
			}
		}
		else
		{
			EditorGUI.LabelField(position, label, "Use only with Vector2");
		}
	}

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 30f;
    }
}

#endif