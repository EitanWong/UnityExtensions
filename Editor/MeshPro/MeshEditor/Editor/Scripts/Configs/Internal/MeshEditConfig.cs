using MeshEditor.Editor.Scripts.Base;
using UnityEngine;

namespace MeshEditor.Editor.Scripts.Configs.Internal
{
    public class MeshEditConfig : MEDR_Config
    {
        // ReSharper disable once InconsistentNaming
        public Color MEDR_MeshEdit_HoverColor
        {
            get
            {
                var color=Color.red;
                GetConfig<Color>(nameof(MEDR_MeshEdit_HoverColor), ref color);
                return color;
            }
            set
            {
                SaveConfig(nameof(MEDR_MeshEdit_HoverColor),value);
            }
        }
        public Color MEDR_MeshEdit_ClickColor
        {
            get
            {
                var color=Color.magenta;
                GetConfig<Color>(nameof(MEDR_MeshEdit_ClickColor), ref color);
                return color;
            }
            set
            {
                SaveConfig(nameof(MEDR_MeshEdit_ClickColor),value);
            }
        }

        public float MEDR_MeshEdit_VertexSize_Multiplier
        {
            get
            {
                var value = 1f;
                GetConfig(nameof(MEDR_MeshEdit_VertexSize_Multiplier), ref value);
                return value;
            }
            set
            {
                SaveConfig(nameof(MEDR_MeshEdit_VertexSize_Multiplier),value);
            }
        }
        
        public float MEDR_MeshEdit_EdgeSize_Multiplier
        {
            get
            {
                var value = 1f;
                GetConfig(nameof(MEDR_MeshEdit_EdgeSize_Multiplier), ref value);
                return value;
            }
            set
            {
                SaveConfig(nameof(MEDR_MeshEdit_EdgeSize_Multiplier),value);
            }
        }
    }
}
