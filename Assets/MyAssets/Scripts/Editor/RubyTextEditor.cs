using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Localization.Components;

namespace TMPro.EditorUtilities
{
    /// <summary>
    /// RubyText 用のエディタ拡張機能
    /// </summary>
    [CustomEditor(typeof(RubyText))]
    public class RubyTextEditor : RubyTMP_UiEditorPanel
    {
        private const int menuPriority = 1501;

        // 区切り線を入れる為に Priority 差を 11 以上つける必要がある
        [MenuItem("CONTEXT/RubyText/RubyLocalize", false, menuPriority)]
        private static void SampleMenu(MenuCommand menuCommand)
        {
            var rubyText = menuCommand.context as RubyText;
            if (rubyText == null)
            {
                return;
            }

            var serializedObject = new SerializedObject(rubyText);
            serializedObject.Update();
            var localizeStringEvent = rubyText.gameObject.AddComponent<LocalizeStringEvent>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(localizeStringEvent.OnUpdateString, rubyText.SetTextForLocalization);
            serializedObject.ApplyModifiedProperties();
        }

        // TextMeshPro → RubyText へ置き換え
        [MenuItem("Assets/EditorTool/TextMeshPro → RubyText")]
        private static void TextMeshProToRubyText()
        {
            var targets = Selection.objects;
            foreach (var target in targets)
            {
                var path = AssetDatabase.GetAssetOrScenePath(target);
                var prefab = PrefabUtility.LoadPrefabContents(path);
                if (prefab == null)
                {
                    continue;
                }
                TransformTextMeshProToRubyText(prefab, path);

                // prefab 編集終了
                PrefabUtility.UnloadPrefabContents(prefab);
            }
        }

        private static void TransformTextMeshProToRubyText(GameObject targetPrefab, string filePath)
        {
            // prefab の中にある prefab は無視するのでリストから抜く
            var textMeshPros = targetPrefab.GetComponentsInChildren<TextMeshProUGUI>(true);
            var targetTextMeshProList = new List<TextMeshProUGUI>();
            foreach (var tmp in textMeshPros)
            {
                var checkParent = PrefabUtility.GetOutermostPrefabInstanceRoot(tmp.gameObject);
                if (checkParent == null)
                {
                    targetTextMeshProList.Add(tmp);
                }
            }

            // TextMeshPro を入れ替える
            var hasTarget = targetTextMeshProList.Count > 0;
            if (!hasTarget)
            {
                return;
            }

            for (var i = 0; i < targetTextMeshProList.Count; i++)
            {
                var tmp = targetTextMeshProList[i];
                var targetGameObject = tmp.gameObject;
                var tempText = tmp.text;

                var tempObj = new GameObject("temp");
                var tempTmp = tempObj.AddComponent<TextMeshProUGUI>();
                SerializedPropertyCopy(tmp, tempTmp);
                DestroyImmediate(tmp, true);

                // RubyText 作成
                var newRubyText = targetGameObject.AddComponent<RubyText>();
                SerializedPropertyCopy(tempTmp, newRubyText);
                newRubyText.SetText(tempText);

                // temp 削除
                DestroyImmediate(tempObj, true);
            }

            // セーブ
            Debug.LogFormat("{0} [{1}個変換]", targetPrefab.name, targetTextMeshProList.Count);
            PrefabUtility.SaveAsPrefabAsset(targetPrefab, filePath);
        }

        private static void SerializedPropertyCopy(Object origin, Object to)
        {
            if (origin == null || to == null)
            {
                return;
            }
            var scriptPath = "m_Script";
            var toSerializedObject = new SerializedObject(to);
            var originSerializedObject = new SerializedObject(origin);
            var originIterator = originSerializedObject.GetIterator();
            var enterChildren = true;
            while (originIterator.NextVisible(enterChildren))
            {
                if (scriptPath == originIterator.propertyPath)
                {
                    continue;
                }
                var toProperty = toSerializedObject.FindProperty(originIterator.propertyPath);
                if (toProperty != null && toProperty.propertyType == originIterator.propertyType)
                {
                    toSerializedObject.CopyFromSerializedProperty(originIterator);
                }
                enterChildren = false;
            }
            toSerializedObject.ApplyModifiedProperties();
        }
    }
}