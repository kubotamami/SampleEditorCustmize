using System;
using UnityEngine;

namespace TMPro
{
    /// <summary>
    /// ルビが使える TextMeshProUGUI
    /// </summary>
    public class RubyText : RubyTextMeshProUGUI
    {
        /// <summary>
        /// テキスト設定関数
        /// </summary>
        /// <param name="text">設定したいテキスト</param>
        /// <param name="isForceUpdate">テキストを必ず反映させるか</param>
        public new void SetText(string text, bool isForceUpdate = false)
        {
            if (UnditedText == text && !isForceUpdate)
            {
                return;
            }

            UnditedText = text;
            m_isLayoutDirty = true;
            SetTextCustom(UnditedText);
            m_isLayoutDirty = false;
        }

        /// <summary>
        /// Localization 用のテキスト設定関数
        /// </summary>
        /// <param name="text">設定したいテキスト</param>
        public void SetTextForLocalization(string text)
        {
            SetText(text, true);
        }
    }
}