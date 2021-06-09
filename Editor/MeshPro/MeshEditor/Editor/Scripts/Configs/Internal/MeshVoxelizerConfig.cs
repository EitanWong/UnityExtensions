using MeshEditor.Editor.Scripts.Base;

namespace MeshEditor.Editor.Scripts.Configs.Internal
{
    public class MeshVoxelizerConfig : MEDR_Config
    {
        // ReSharper disable once InconsistentNaming
        public int MEDR_MeshVoxelizer_PixelSize
        {
            get
            {
                int value = 16;
                GetConfig(nameof(MEDR_MeshVoxelizer_PixelSize), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshVoxelizer_PixelSize), value); }
        }

        // ReSharper disable once InconsistentNaming
        public bool MEDR_MeshVoxelizer_CustomVoxelMat
        {
            get
            {
                bool value = false;
                GetConfig(nameof(MEDR_MeshVoxelizer_CustomVoxelMat), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshVoxelizer_CustomVoxelMat), value); }
        }
    }
}