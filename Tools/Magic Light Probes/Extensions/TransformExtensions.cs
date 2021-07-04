using UnityEngine;

namespace MagicLightProbes
{
	public static class TransformExtensions
	{
		public static bool MLP_IsVisibleFrom(this Transform transform, Camera camera)
		{
			Bounds transformBounds = new Bounds(transform.position, transform.localScale);
			Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);

			return GeometryUtility.TestPlanesAABB(planes, transformBounds);
		}
	}
}
