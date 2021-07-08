#if UNITY_EDITOR
using Extensions.MeshPro.MeshEditor.Editor.Scripts.Base;

namespace Extensions.MeshPro.MeshEditor.Editor.Scripts.Configs.Internal
{
    public class MeshSimplifierConfig : MEDR_Config
    {
        // ReSharper disable once InconsistentNaming
        public float MEDR_MeshSimplifier_SimplifierRate
        {
            get
            {
                float value = 25;
                GetConfig(nameof(MEDR_MeshSimplifier_SimplifierRate), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshSimplifier_SimplifierRate), value); }
        }
        // ReSharper disable once InconsistentNaming
        public int MEDR_MeshSimplifier_DetailsCount
        {
            get
            {
                int value = 8;
                GetConfig(nameof(MEDR_MeshSimplifier_DetailsCount), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshSimplifier_DetailsCount), value); }
        }
    
        public float MEDR_MeshSimplifier_LODDistanceMultiplier
        {
            get
            {
                float value = 0.5f;
                GetConfig(nameof(MEDR_MeshSimplifier_LODDistanceMultiplier), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshSimplifier_LODDistanceMultiplier), value); }
        }
    
        // ReSharper disable once InconsistentNaming
        public int MEDR_MeshSimplifier_LODGroupFadeMode
        {
            get
            {
                int value = 0;
                GetConfig(nameof(MEDR_MeshSimplifier_LODGroupFadeMode), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshSimplifier_LODGroupFadeMode), value); }
        }
        // ReSharper disable once InconsistentNaming
        public string MEDR_MeshSimplifier_LODSavePath
        {
            get
            {
                string value = "Assets/LODs";
                GetConfig(nameof(MEDR_MeshSimplifier_LODSavePath), ref value);
                return value;
            }
            set { SaveConfig(nameof(MEDR_MeshSimplifier_LODSavePath), value); }
        }
    }
}
#endif
