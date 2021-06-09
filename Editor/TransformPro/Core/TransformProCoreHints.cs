namespace TransformPro.Scripts
{
    using System;
    using UnityEngine;

    public partial class TransformPro
    {
        private Vector3 hintEulerLocal;
        private Vector3 hintEulerWorld;

        public Vector3 HintEuler
        {
            get
            {
                switch (TransformPro.Space)
                {
                    default:
                        throw new Exception(string.Format("[<color=red>TransformPro</color>] Space mode {0} not handled!", TransformPro.space));
                    case TransformProSpace.Local:
                        return this.HintEulerLocal;
                    case TransformProSpace.World:
                        return this.HintEulerLocal;
                }
            }
        }

        public Vector3 HintEulerLocal
        {
            get
            {
                if (!this.IsHintEulerLocalValid)
                {
                    this.hintEulerLocal = this.Transform.localEulerAngles;
                }
                return this.hintEulerLocal;
            }
            private set
            {
                if (Mathf.Abs(value.x) < 0.0001f)
                {
                    value.x = 0;
                }
                if (Mathf.Abs(value.y) < 0.0001f)
                {
                    value.y = 0;
                }
                if (Mathf.Abs(value.z) < 0.0001f)
                {
                    value.z = 0;
                }
                this.hintEulerLocal = value;
                this.Transform.localEulerAngles = value;
            }
        }

        public Vector3 HintEulerWorld
        {
            get
            {
                if (!this.IsHintEulerWorldValid)
                {
                    this.hintEulerWorld = this.Transform.eulerAngles;
                }
                return this.hintEulerWorld;
            }
            private set
            {
                if (Mathf.Abs(value.x) < 0.0001f)
                {
                    value.x = 0;
                }
                if (Mathf.Abs(value.y) < 0.0001f)
                {
                    value.y = 0;
                }
                if (Mathf.Abs(value.z) < 0.0001f)
                {
                    value.z = 0;
                }
                this.hintEulerWorld = value;
                this.Transform.eulerAngles = value;
            }
        }

        public bool IsHintEulerLocalValid
        {
            get
            {
                Quaternion eulerHint = Quaternion.Euler(this.hintEulerLocal);
                Quaternion eulerActual = Quaternion.Euler(this.Transform.localEulerAngles);
                float angle = Quaternion.Angle(eulerHint, eulerActual);
                return Mathf.Abs(angle) < 0.04f;
            }
        }

        public bool IsHintEulerWorldValid
        {
            get
            {
                Quaternion eulerHint = Quaternion.Euler(this.hintEulerWorld);
                Quaternion eulerActual = Quaternion.Euler(this.Transform.eulerAngles);
                float angle = Quaternion.Angle(eulerHint, eulerActual);
                return Mathf.Abs(angle) < 0.04f;
            }
        }

        public void ReadHints()
        {
            this.hintEulerLocal = this.Transform.localEulerAngles;
            this.hintEulerWorld = this.Transform.eulerAngles;
        }
    }
}
