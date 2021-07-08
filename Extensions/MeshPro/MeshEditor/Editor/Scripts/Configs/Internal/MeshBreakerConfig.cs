#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Base;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal
{
    public class MeshBreakerConfig : MEDR_Config
    {
        // ReSharper disable once InconsistentNaming
        public int MEDR_MeshBreaker_breakIterations
        {
            get
            {
                int value = 6;
                GetConfig(nameof(MEDR_MeshBreaker_breakIterations), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshBreaker_breakIterations), value); }
        }

        // ReSharper disable once InconsistentNaming
        public bool MEDR_MeshBreaker_CustomCutFaceMat
        {
            get
            {
                bool value = false;
                GetConfig(nameof(MEDR_MeshBreaker_CustomCutFaceMat), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshBreaker_CustomCutFaceMat), value); }
        }

        public Color MEDR_MeshBreaker_CustomCutColor
        {
            get
            {
                Color value = Color.red;
                GetConfig(nameof(MEDR_MeshBreaker_CustomCutColor), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshBreaker_CustomCutColor), value); }
        }

        public float MEDR_MeshBreaker_CustomCutLineSize
        {
            get
            {
                float value = 10;
                GetConfig(nameof(MEDR_MeshBreaker_CustomCutLineSize), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshBreaker_CustomCutLineSize), value); }
        }
    }
}
#endif