// DISABLED AFTER VR MODE SEPERATION WE CAN USE IN THE FUTURE FOR ANOTHER IMPLEMENT
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
// using UnityToolbarExtender;
//
// [Serializable]
// internal class ToolbarVrMode : BaseToolbarElement
// {
//     private GUIContent vrModeBtn;
//     private bool _currentVrMode = false;
//
//     public override string NameInList => "[Button] VR Mode";
//
//     public static readonly string[] SymbolsforVR = { "VR_ENABLED", "HVR_OCULUS" };
//
//     private static bool CurrentVrMode
//     {
//         get => EditorPrefs.GetBool("VR_MODE", false);
//         set => EditorPrefs.SetBool("VR_MODE", value);
//     }
//
//     public override void Init()
//     {
//         _currentVrMode = CurrentVrMode;
//         UpdateVrModeButton();
//     }
//
//     protected override void OnDrawInToolbar()
//     {
//         if (GUILayout.Button(vrModeBtn, ToolbarStyles.commandButtonStyle))
//         {
//             _currentVrMode = !_currentVrMode;
//             UpdateVrModeButton();
//             ToggleVRMode();
//         }
//     }
//
//     private void UpdateVrModeButton()
//     {
//         vrModeBtn = EditorGUIUtility.IconContent(_currentVrMode ? "d_greenLight" : "d_redLight");
//         vrModeBtn.tooltip = _currentVrMode ? "Disable VR Mode" : "Enable VR Mode";
//     }
//
//     private void ToggleVRMode()
//     {
//         Debug.LogError("VR Mode: " + _currentVrMode);
//
//         var existingSymbols = GetScriptingDefineSymbols(BuildTargetGroup.Android);
//
//         // Convert the list to a List for removal
//         var uniqueSymbols = new List<string>(existingSymbols);
//
//         // Remove or add vr specific symbols when switching to non vr mode
//         if (!_currentVrMode)
//         {
//             foreach (string symbol in SymbolsforVR)
//             {
//                 uniqueSymbols.Remove(symbol);
//             }
//         }
//         else
//         {
//             uniqueSymbols.AddRange(SymbolsforVR);
//         }
//
//         // Set the modified symbols
//         SetScriptingDefineSymbols(BuildTargetGroup.Android, uniqueSymbols.ToArray());
//
//         CurrentVrMode = _currentVrMode;
//         Debug.LogError("Current value saved!");
//     }
//
//     private static string[] GetScriptingDefineSymbols(BuildTargetGroup targetGroup)
//     {
//         string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);
//         return defineSymbols.Split(';');
//     }
//
//     private static void SetScriptingDefineSymbols(BuildTargetGroup targetGroup, string[] symbols)
//     {
//         string defineSymbols = string.Join(";", symbols);
//         PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, defineSymbols);
//     }
//
//     protected override void OnDrawInList(Rect position)
//     {
//     }
// }
