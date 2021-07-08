#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Base;
using MeshEditor.Editor.Scripts.Base;
using UnityEngine;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal
{
    public class MeshDeformerConfig : MEDR_Config
    {
        // ReSharper disable once InconsistentNaming
        public Color MEDR_MeshDeformer_HoverColor
        {
            get
            {
                var color = Color.green;
                GetConfig<Color>(nameof(MEDR_MeshDeformer_HoverColor), ref color);
                return color;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_HoverColor), value); }
        }
        // ReSharper disable once InconsistentNaming
        public Color MEDR_MeshDeformer_ClickColor
        {
            get
            {
                var color=Color.blue;
                GetConfig<Color>(nameof(MEDR_MeshDeformer_ClickColor), ref color);
                return color;
            }
            set
            {
                SaveConfig(nameof(MEDR_MeshDeformer_ClickColor),value);
            }
        }
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshDeformer_brushradius
        {
            get
            {
                float value = 1f;
                GetConfig(nameof(MEDR_MeshDeformer_brushradius), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_brushradius), value); }
        }
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshDeformer_brushMaxradius
        {
            get
            {
                float value = 10f;
                GetConfig(nameof(MEDR_MeshDeformer_brushMaxradius), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_brushMaxradius), value); }
        }
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshDeformer_brushRadiusStep
        {
            get
            {
                float value = 0.1f;
                GetConfig(nameof(MEDR_MeshDeformer_brushRadiusStep), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_brushRadiusStep), value); }
        }
        
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshDeformer_bushIntensity
        {
            get
            {
                float value = 1f;
                GetConfig(nameof(MEDR_MeshDeformer_bushIntensity), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_bushIntensity), value); }
        }
        
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshDeformer_brushMaxintensity
        {
            get
            {
                float value = 3f;
                GetConfig(nameof(MEDR_MeshDeformer_brushMaxintensity), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_brushMaxintensity), value); }
        }
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshDeformer_brushIntensityStep
        {
            get
            {
                float value = 0.1f;
                GetConfig(nameof(MEDR_MeshDeformer_brushIntensityStep), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_brushIntensityStep), value); }
        }
        
        // ReSharper disable once InconsistentNaming
        public int MEDR_MeshDeformer_directionType
        {
            get
            {
                int value = 0;
                GetConfig(nameof(MEDR_MeshDeformer_directionType), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshDeformer_directionType), value); }
        }
    }
}
#endif
