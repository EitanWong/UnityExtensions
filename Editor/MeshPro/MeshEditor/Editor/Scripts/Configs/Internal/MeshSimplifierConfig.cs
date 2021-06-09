using System.Collections;
using System.Collections.Generic;
using MeshEditor.Editor.Scripts.Base;
using UnityEngine;

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
}
