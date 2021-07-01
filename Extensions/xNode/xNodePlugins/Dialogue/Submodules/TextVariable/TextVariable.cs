using System.Diagnostics;
using UnityEngine;

namespace Dialogue.Submodules.TextVariable
{
    public abstract class TextVariable
    {
        /// <summary>
        /// 检测文本变量
        /// </summary>
        /// <param name="originText"></param>
        /// <returns></returns>
        internal abstract bool Detect(string originText);
        /// <summary>
        /// 处理文本变量
        /// </summary>
        /// <param name="originText"></param>
        /// <returns></returns>
        internal abstract string Process(string originText);
    }
}
