using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLightProbes
{
    [ExecuteInEditMode]
    public class MLPPortal : MonoBehaviour
    {
        public float yStep;
        public float zStep;

        public int yCounter;
        public int zCounter;

        public bool calculate;
        
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            if (calculate)
            {
                calculate = false; 
                DrawRayX(Vector3.zero);
            }
        }

        private void DrawRayX(Vector3 dir)
        {
            Vector3 currentDirX;
            
            if (dir == Vector3.zero)
            {
                currentDirX = Vector3.forward;
            }
            else
            {
                currentDirX = dir;
            }            

            if (yCounter < (int) (360 / yStep))
            {
                yCounter++;
                Debug.DrawRay(transform.position, currentDirX, Color.blue, 1000000000);
                DrawRayX(Quaternion.AngleAxis(-yStep, Vector3.up) * currentDirX);
                DrawRayZ(Vector3.zero);
            }
        }

        private void DrawRayZ(Vector3 dir)
        {
            Vector3 currentDirZ;

            if (dir == Vector3.zero)
            {
                currentDirZ = Vector3.forward;
            }
            else
            {
                currentDirZ = dir;
            }

            if (zCounter < (int) (360 / yStep))
            {
                zCounter++;
                Debug.DrawRay(transform.position, currentDirZ, Color.blue, 1000000000);
                DrawRayZ(Quaternion.AngleAxis(-zStep, Vector3.left) * currentDirZ);
            }
        }
    }
}
