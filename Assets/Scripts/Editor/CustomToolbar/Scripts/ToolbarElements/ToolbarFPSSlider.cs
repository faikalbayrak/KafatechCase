using System;
using UnityEditor;
using UnityEngine;

[Serializable]
internal class ToolbarFPSSlider : BaseToolbarElement
{
    private int minFPS;
    private int maxFPS;

    private int selectedFramerate;

    public ToolbarFPSSlider(int minFPS = 60, int maxFPS = 360) : base(150)
    {
        this.minFPS = minFPS;
        this.maxFPS = maxFPS;
    }

    public override string NameInList => "[Slider] FPS";

    public override void Init()
    {
        selectedFramerate = 144;
    }

    protected override void OnDrawInList(Rect position)
    {
        position.width = 70.0f;
        EditorGUI.LabelField(position, "Min FPS");
        position.x += position.width + FieldSizeSpace;
        position.width = 50.0f;
        minFPS = Mathf.RoundToInt(EditorGUI.IntField(position, "", minFPS));
        position.x += position.width + FieldSizeSpace;
        position.width = 70.0f;
        EditorGUI.LabelField(position, "Max FPS");
        position.x += position.width + FieldSizeSpace;
        position.width = 50.0f;
        maxFPS = Mathf.RoundToInt(EditorGUI.IntField(position, "", maxFPS));
    }

    protected override void OnDrawInToolbar()
    {
        EditorGUILayout.LabelField("Target FPS", GUILayout.Width(65));
        selectedFramerate = EditorGUILayout.IntSlider("", selectedFramerate, minFPS, maxFPS, GUILayout.Width(WidthInToolbar - 30.0f));
        if (EditorApplication.isPlaying && selectedFramerate != Application.targetFrameRate)
            Application.targetFrameRate = selectedFramerate;
    }
}
