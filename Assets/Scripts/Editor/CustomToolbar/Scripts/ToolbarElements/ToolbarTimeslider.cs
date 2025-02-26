using System;
using UnityEditor;
using UnityEngine;

[Serializable]
internal class ToolbarTimeslider : BaseToolbarElement
{
    private float _minTime = 1;
    private float _maxTime = 5;

    public ToolbarTimeslider(float minTime = 0.0f, float maxTime = 5.0f) : base(150)
    {
        _minTime = minTime;
        _maxTime = maxTime;
    }

    public override string NameInList => "[Slider] Timescale";

    public override void Init()
    {
    }

    protected override void OnDrawInList(Rect position)
    {
        position.width = 70.0f;
        EditorGUI.LabelField(position, "Min Time");
        position.x += position.width + FieldSizeSpace;
        position.width = 50.0f;
        _minTime = EditorGUI.FloatField(position, "", _minTime);
        position.x += position.width + FieldSizeSpace;
        position.width = 70.0f;
        EditorGUI.LabelField(position, "Max Time");
        position.x += position.width + FieldSizeSpace;
        position.width = 50.0f;
        _maxTime = EditorGUI.FloatField(position, "", _maxTime);
    }

    protected override void OnDrawInToolbar()
    {
        EditorGUILayout.LabelField("TimeScale", GUILayout.Width(65));
        Time.timeScale = EditorGUILayout.Slider("", Time.timeScale, _minTime, _maxTime, GUILayout.Width(WidthInToolbar - 30.0f));
    }
}
