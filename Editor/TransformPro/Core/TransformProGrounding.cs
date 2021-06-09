namespace TransformPro.Scripts
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    // TODO: Replace all the custom vector maths with Matrix4x4 transformations. This will fix a lot of issues with offset colliders.
    public partial class TransformPro
    {
        private static float dropLift = 0.1f;

        private static bool DropBox(Collider[] colliders, Vector3 position, Vector3 size, Quaternion rotation, out RaycastHit[] hits)
        {
            position += Vector3.up * TransformPro.dropLift;
#if UNITY_5_3_OR_NEWER
            hits = Physics.BoxCastAll(position, size / 2f, Vector3.down, rotation);
#else
            hits = Physics.RaycastAll(new Ray(position, Vector3.down));
            for(int hitIndex = 0; hitIndex < hits.Length; hitIndex++) 
            {
                hits[hitIndex].distance -= size.y / 2f;
            }
#endif
            return TransformPro.TryGetRealHits(hits, colliders, out hits);
        }

        private static bool DropCapsule(Collider[] colliders, Vector3 center, float radius, float height, Quaternion rotation, out RaycastHit[] hits)
        {
            center += Vector3.up * TransformPro.dropLift;
            Vector3 halfCapsule = TransformPro.GetCapsuleHalfOffset(radius, height, rotation);
            hits = Physics.CapsuleCastAll(center + halfCapsule, center - halfCapsule, radius, Vector3.down);
            return TransformPro.TryGetRealHits(hits, colliders, out hits);
        }

        private static bool DropComponent(bool alignToGround, Collider[] colliders, Component component, out RaycastHit[] hits)
        {
            if (component == null)
            {
                hits = new RaycastHit[0];
                return false;
            }

            Collider collider = component as Collider;
            if ((collider != null) && !collider.isTrigger && collider.enabled)
            {
                SphereCollider sphereCollider = collider as SphereCollider;
                if ((sphereCollider != null) && TransformPro.ResolveSphereCollider(alignToGround, colliders, sphereCollider, out hits))
                {
                    return true;
                }

                BoxCollider boxCollider = collider as BoxCollider;
                if ((boxCollider != null) && TransformPro.ResolveBoxCollider(alignToGround, colliders, boxCollider, out hits))
                {
                    return true;
                }

                CapsuleCollider capsuleCollider = collider as CapsuleCollider;
                if ((capsuleCollider != null) && TransformPro.ResolveCapsuleCollider(alignToGround, colliders, capsuleCollider, out hits))
                {
                    return true;
                }

                WheelCollider wheelCollider = collider as WheelCollider;
                if ((wheelCollider != null) && TransformPro.ResolveWheelCollider(alignToGround, colliders, wheelCollider, out hits))
                {
                    return true;
                }

                MeshCollider meshCollider = collider as MeshCollider;
                if ((meshCollider != null) &&
                    TransformPro.SolveBounds(alignToGround,
                                             colliders,
                                             meshCollider.transform,
                                             new Bounds(meshCollider.bounds.center,
                                                        new Vector3(
                                                                    meshCollider.bounds.size.x / meshCollider.transform.lossyScale.x,
                                                                    meshCollider.bounds.size.y / meshCollider.transform.lossyScale.y,
                                                                    meshCollider.bounds.size.z / meshCollider.transform.lossyScale.z)),
                                             TransformProSpace.World,
                                             out hits))
                {
                    return true;
                }
            }

            Renderer renderer = component as Renderer;
            if ((renderer != null) && renderer.enabled)
            {
                MeshRenderer meshRenderer = renderer as MeshRenderer;
                if ((meshRenderer != null) && TransformPro.ResolveMeshRenderer(alignToGround, colliders, meshRenderer, out hits))
                {
                    return true;
                }
            }

            hits = new RaycastHit[0];
            return false;
        }

        private static bool DropRay(Collider[] colliders, Vector3 position, out RaycastHit[] hits)
        {
            position += Vector3.up * TransformPro.dropLift;
            hits = Physics.RaycastAll(position, Vector3.down);
            return TransformPro.TryGetRealHits(hits, colliders, out hits);
        }

        private static bool DropSphere(Collider[] colliders, Vector3 position, float radius, out RaycastHit[] hits)
        {
            position += Vector3.up * TransformPro.dropLift;
            hits = Physics.SphereCastAll(position, radius, Vector3.down);
            return TransformPro.TryGetRealHits(hits, colliders, out hits);
        }

        private static bool DropWheel(Collider[] colliders, Vector3 position, float wheelRadius, out RaycastHit[] hits)
        {
            position += Vector3.up * TransformPro.dropLift;
            hits = Physics.RaycastAll(position, Vector3.down);
            if (!TransformPro.TryGetRealHits(hits, colliders, out hits))
            {
                return false;
            }

            for (int hitIndex = 0; hitIndex < hits.Length; hitIndex++)
            {
                hits[hitIndex].distance -= wheelRadius;
            }
            return true;
        }

        private static Vector3 GetCapsuleHalfOffset(float radius, float height, Quaternion rotation)
        {
            float halfHeight = height / 2f;
            float length = Mathf.Max(0, halfHeight - radius);
            return rotation * Vector3.down * length;
        }

        private static bool ResolveBoxCollider(bool alignToGround, Collider[] colliders, BoxCollider boxCollider, out RaycastHit[] hits)
        {
            return TransformPro.SolveBounds(alignToGround, colliders, boxCollider.transform, new Bounds(boxCollider.center, boxCollider.size), TransformProSpace.Local, out hits);
        }

        private static bool ResolveCapsuleCollider(bool alignToGround, Collider[] colliders, CapsuleCollider capsuleCollider, out RaycastHit[] hits)
        {
            return TransformPro.SolveCapsule(alignToGround, colliders, capsuleCollider.transform, capsuleCollider.center, capsuleCollider.height, capsuleCollider.radius, TransformProSpace.Local, out hits);
        }

        private static bool ResolveMeshFilter(bool alignToGround, Collider[] colliders, MeshFilter meshFilter, out RaycastHit[] hits)
        {
            return TransformPro.SolveBounds(alignToGround, colliders, meshFilter.transform, meshFilter.sharedMesh.bounds, TransformProSpace.Local, out hits);
        }

        private static bool ResolveMeshRenderer(bool alignToGround, Collider[] colliders, MeshRenderer meshRenderer, out RaycastHit[] hits)
        {
            return TransformPro.SolveBounds(alignToGround, colliders, meshRenderer.transform, meshRenderer.bounds, TransformProSpace.Local, out hits);
        }

        private static bool ResolveSphereCollider(bool alignToGround, Collider[] colliders, SphereCollider sphereCollider, out RaycastHit[] hits)
        {
            return TransformPro.SolveSphere(alignToGround, colliders, sphereCollider.transform, sphereCollider.center, sphereCollider.radius, TransformProSpace.Local, out hits);
        }

        private static bool ResolveWheelCollider(bool alignToGround, Collider[] colliders, WheelCollider wheelCollider, out RaycastHit[] hits)
        {
            return TransformPro.SolveWheel(alignToGround, colliders, wheelCollider.transform, wheelCollider.center, wheelCollider.radius, TransformProSpace.Local, out hits);
        }

        private static bool SolveBounds(bool alignToGround, Collider[] colliders, Transform transform, TransformProBounds bounds, out RaycastHit[] hits)
        {
            if (bounds != null)
            {
                // Local bounds grounding allows proper overhang support for objects with complex rotational frames.
                return TransformPro.SolveBounds(alignToGround, colliders, transform, bounds.Local, TransformProSpace.Local, out hits);
                //return TransformPro.SolveBounds(alignToGround, colliders, transform, bounds.World, TransformProSpace.World, out hits);
            }

            hits = new RaycastHit[0];
            return false;
        }

        private static bool SolveBounds(bool alignToGround, Collider[] colliders, Transform transform, Bounds bounds, TransformProSpace space, out RaycastHit[] hits)
        {
            Quaternion rotationGameObject = transform.rotation;
            Vector3 scaleGameObject = transform.lossyScale;
            Vector3 scaleBounds = Vector3.Scale(bounds.size, scaleGameObject);

            float bearing = transform.localEulerAngles.y;
            Vector3 positionBounds = space == TransformProSpace.World ? bounds.center : transform.localToWorldMatrix.MultiplyPoint3x4(bounds.center);

            if (alignToGround)
            {
                if (!TransformPro.DropBox(colliders, positionBounds, scaleBounds, rotationGameObject, out hits))
                {
                    return false;
                }

                // Store the current rotation and set the new rotation.
                // TODO: Since refactoring this could easily be replaced with a custom matrix rather than setting and resetting the actual object.
                Quaternion rotationOriginal = rotationGameObject;
                transform.up = hits.FirstOrDefault().normal;
                transform.Rotate(Vector3.up, bearing, UnityEngine.Space.Self);

                // Get the rotation data for the alignment
                rotationGameObject = transform.rotation;
                positionBounds = space == TransformProSpace.World ? bounds.center : transform.localToWorldMatrix.MultiplyPoint3x4(bounds.center);

                // Reset the mesh filter object for now - this angle will be used later based on compound distance testing
                transform.rotation = rotationOriginal;
            }

            return TransformPro.DropBox(colliders, positionBounds, scaleBounds, rotationGameObject, out hits);
        }

        private static bool SolveCapsule(bool alignToGround, Collider[] colliders, Transform transform, Vector3 center, float height, float radius, TransformProSpace space, out RaycastHit[] hits)
        {
            Quaternion rotationGameObject = transform.rotation;
            float scaledHeight = height * transform.localScale.y;
            float scaledRadius = radius * Mathf.Max(transform.localScale.x, transform.localScale.z);

            float bearing = transform.localEulerAngles.y;
            Vector3 positionCapsule = space == TransformProSpace.World ? center : transform.localToWorldMatrix.MultiplyPoint3x4(center);

            if (alignToGround)
            {
                if (!TransformPro.DropCapsule(colliders, positionCapsule, scaledRadius, scaledHeight, rotationGameObject, out hits))
                {
                    return false;
                }

                // Store the current rotation and set the new rotation.
                // TODO: Since refactoring this could easily be replaced with a custom matrix rather than setting and resetting the actual object.
                Quaternion rotationOriginal = rotationGameObject;
                transform.up = hits.FirstOrDefault().normal;
                transform.Rotate(Vector3.up, bearing, UnityEngine.Space.Self);

                // Get the rotation data for the alignment
                rotationGameObject = transform.rotation;
                positionCapsule = space == TransformProSpace.World ? center : transform.localToWorldMatrix.MultiplyPoint3x4(center);

                // Reset the mesh filter object for now - this angle will be used later based on compound distance testing
                transform.rotation = rotationOriginal;
            }

            return TransformPro.DropCapsule(colliders, positionCapsule, scaledRadius, scaledHeight, rotationGameObject, out hits);
        }

        private static bool SolveRay(bool alignToGround, Collider[] colliders, Transform transform, out RaycastHit[] hits)
        {
            float bearing = transform.localEulerAngles.y;

            Vector3 position = transform.position;
            if (alignToGround)
            {
                if (!TransformPro.DropRay(colliders, position, out hits))
                {
                    return false;
                }

                // Store the current rotation and set the new rotation.
                // TODO: Since refactoring this could easily be replaced with a custom matrix rather than setting and resetting the actual object.
                // LOOK: We are leaving the rotation in place on sphere - evaluate impact
                transform.up = hits.FirstOrDefault().normal;
                transform.Rotate(Vector3.up, bearing, UnityEngine.Space.Self);
            }

            return TransformPro.DropRay(colliders, position, out hits);
        }

        private static bool SolveSphere(bool alignToGround, Collider[] colliders, Transform transform, Vector3 center, float radius, TransformProSpace space, out RaycastHit[] hits)
        {
            float scaledRadius = radius * Mathf.Max(transform.localScale.x, transform.localScale.y, transform.localScale.z);

            float bearing = transform.localEulerAngles.y;
            Vector3 positionSphere = space == TransformProSpace.World ? center : transform.localToWorldMatrix.MultiplyPoint3x4(center);

            if (alignToGround)
            {
                if (!TransformPro.DropSphere(colliders, positionSphere, scaledRadius, out hits))
                {
                    return false;
                }

                // Store the current rotation and set the new rotation.
                // TODO: Since refactoring this could easily be replaced with a custom matrix rather than setting and resetting the actual object.
                // LOOK: We are leaving the rotation in place on sphere - evaluate impact
                transform.up = hits.FirstOrDefault().normal;
                transform.Rotate(Vector3.up, bearing, UnityEngine.Space.Self);

                // Newly rotated and offset position for sphere drop.
                positionSphere = space == TransformProSpace.World ? center : transform.localToWorldMatrix.MultiplyPoint3x4(center);
            }

            return TransformPro.DropSphere(colliders, positionSphere, scaledRadius, out hits);
        }

        private static bool SolveWheel(bool alignToGround, Collider[] colliders, Transform transform, Vector3 center, float radius, TransformProSpace space, out RaycastHit[] hits)
        {
            float scaledRadius = radius * transform.localScale.y;

            float bearing = transform.localEulerAngles.y;
            Vector3 positionWheel = space == TransformProSpace.World ? center : transform.localToWorldMatrix.MultiplyPoint3x4(center);

            if (alignToGround)
            {
                if (!TransformPro.DropWheel(colliders, positionWheel, scaledRadius, out hits))
                {
                    return false;
                }

                // Store the current rotation and set the new rotation.
                // TODO: Since refactoring this could easily be replaced with a custom matrix rather than setting and resetting the actual object.
                transform.up = hits.FirstOrDefault().normal;
                transform.Rotate(Vector3.up, bearing, UnityEngine.Space.Self);

                // Newly rotated and offset position for sphere drop.
                positionWheel = space == TransformProSpace.World ? center : transform.localToWorldMatrix.MultiplyPoint3x4(center);
            }

            return TransformPro.DropWheel(colliders, positionWheel, scaledRadius, out hits);
        }

        private static bool TryGetRealHits(RaycastHit[] hits, Collider[] colliders, out RaycastHit[] finalHits)
        {
            IEnumerable<RaycastHit> selfHits = hits.Where(i => (i.collider != null) && colliders.Contains(i.collider));
            finalHits = hits.Except(selfHits)
                            .Where(x => (x.collider != null) && (x.collider.gameObject != null) && x.collider.enabled && x.collider.gameObject.activeSelf && (x.distance > TransformPro.dropLift))
                            .OrderBy(x => x.distance)
                            .ToArray();
            return finalHits.Any();
        }

        public bool Drop()
        {
            return this.Drop(false);
        }

        public bool Ground()
        {
            return this.Drop(true);
        }

        private bool Drop(bool alsoGround)
        {
            this.UpdateColliderBounds();
            this.UpdateRendererBounds();

            float bearing = this.Transform.localEulerAngles.y;
            Collider[] colliders = this.HasColliders ? this.Colliders.ToArray() : new Collider[0];

            RaycastHit[] finalHits;

            if (!this.HasColliders && !this.HasRenderers)
            {
                if (!TransformPro.SolveRay(alsoGround, colliders, this.Transform, out finalHits))
                {
                    return false;
                }
            }
            else
            {
                if (this.HasChildren)
                {
                    if (this.HasColliders && TransformPro.SolveBounds(alsoGround, colliders, this.Transform, this.ColliderBounds, out finalHits))
                    {
                        // Collider bounds used to drop sucessfully
                    }
                    else if (this.HasRenderers && TransformPro.SolveBounds(alsoGround, colliders, this.Transform, this.RendererBounds, out finalHits))
                    {
                        // Renderer bounds used to drop sucessfully
                    }
                    else
                    {
                        // TODO: Improve error reporting - this will report the floor could not be found, rather than a lack of data on the drop object.
                        return false;
                    }
                }
                else
                {
                    List<RaycastHit> realHits = new List<RaycastHit>();
                    IEnumerable<Component> components = this.HasColliders ? colliders : this.Renderers.Cast<Component>();
                    foreach (Component component in components)
                    {
                        if (component == null)
                        {
                            continue;
                        }

                        RaycastHit[] hits;
                        if (TransformPro.DropComponent(alsoGround, colliders, component, out hits))
                        {
                            realHits.AddRange(hits);
                        }
                    }
                    finalHits = realHits.ToArray();
                }
            }

            if (!finalHits.Any())
            {
                return false;
            }
            RaycastHit finalHit = finalHits.FirstOrDefault();

            if ((finalHit.distance <= 0) || (finalHit.collider == null))
            {
                return false;
            }
            finalHit.distance -= TransformPro.dropLift;
            if (finalHit.distance <= 0)
            {
                return true;
            }

            this.PositionWorld += Vector3.down * finalHit.distance;
            if (alsoGround)
            {
                this.Transform.up = finalHit.normal;
                this.Transform.Rotate(Vector3.up, bearing, UnityEngine.Space.Self);
            }

            // Reset the current object data for the current transformation space using the final position.
            //this.UpdateDisplayTransform();
            return true;
        }
    }
}
