using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityToolbarExtender;

[Serializable]
internal class ToolbarReloadScene : BaseToolbarElement
{
    private static GUIContent reloadSceneBtn;

    public override string NameInList => "[Button] Reload scene";

    public override void Init()
    {
        reloadSceneBtn = new GUIContent((Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Editor/CustomToolbar/Icons/LookDevResetEnv@2x.png", typeof(Texture2D)), "Reload scene");
    }

    protected override void OnDrawInList(Rect position)
    {
    }

    protected override void OnDrawInToolbar()
    {
        EditorGUIUtility.SetIconSize(new Vector2(18, 18));
        if (GUILayout.Button(reloadSceneBtn, ToolbarStyles.commandButtonStyle))
        {
            if (EditorApplication.isPlaying)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
