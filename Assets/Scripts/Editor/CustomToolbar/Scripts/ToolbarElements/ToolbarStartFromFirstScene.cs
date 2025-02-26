using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

[Serializable]
internal class ToolbarStartFromFirstScene : BaseToolbarElement
{
    private static GUIContent startFromFirstSceneBtn;
    public override string NameInList => "[Button] Start from first scene";

    public override void Init()
    {
        // EditorApplication.playModeStateChanged += LogPlayModeState;
        startFromFirstSceneBtn = new GUIContent((Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/CustomToolbar/Icons/LookDevSingle1@2x.png", typeof(Texture2D)),
            "Play Splash Scene");
    }

    protected override void OnDrawInList(Rect position)
    {
    }

    protected override void OnDrawInToolbar()
    {
        if (GUILayout.Button(startFromFirstSceneBtn, ToolbarStyles.commandButtonStyle))
        {
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene("Assets/_Scenes/HeroesScenes/SplashScreen.unity");
            }

            EditorApplication.isPlaying = !EditorApplication.isPlaying;
        }
    }

    private static void LogPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.OpenScene("Assets/_Scenes/HeroesScenes/SplashScreen.unity");
        }
    }
}
