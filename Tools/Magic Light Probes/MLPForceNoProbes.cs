#if UNITY_EDITOR
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    public class MLPForceNoProbes : MonoBehaviour
    {
        private void OnEnable()
        {
            AddComponentToChildren(transform);
        }

        private void OnDestroy()
        {
            RemoveComponentFromChildren(transform);
        }

        private void AddComponentToChildren(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).GetComponent<MeshRenderer>() != null)
                {
                    if (parent.GetChild(i).childCount > 0)
                    {
                        if (parent.GetChild(i).GetComponent<MLPForceNoProbes>() == null)
                        {
                            parent.GetChild(i).gameObject.AddComponent<MLPForceNoProbes>();
                        }

                        AddComponentToChildren(parent.GetChild(i));
                    }
                    else
                    {
                        if (parent.GetChild(i).GetComponent<MLPForceNoProbes>() == null)
                        {
                            parent.GetChild(i).gameObject.AddComponent<MLPForceNoProbes>();
                        }
                    }
                }
            }
        }

        private void RemoveComponentFromChildren(Transform parent)
        {
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i).GetComponent<MeshRenderer>() != null)
                {
                    if (parent.GetChild(i).childCount > 0)
                    {
                        MLPForceNoProbes forceNoProbes = parent.GetChild(i).GetComponent<MLPForceNoProbes>();

                        if (forceNoProbes != null)
                        {
                            DestroyImmediate(forceNoProbes);
                        }

                        RemoveComponentFromChildren(parent.GetChild(i));
                    }
                    else
                    {
                        MLPForceNoProbes forceNoProbes = parent.GetChild(i).GetComponent<MLPForceNoProbes>();

                        if (forceNoProbes != null)
                        {
                            DestroyImmediate(forceNoProbes);
                        }
                    }
                }
            }
        }
    }
}
#endif
