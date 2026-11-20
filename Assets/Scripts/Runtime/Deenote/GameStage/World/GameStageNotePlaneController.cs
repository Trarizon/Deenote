using Deenote.Core.GameStage;
using UnityEngine;

namespace Deenote.GameStage.World
{
    public sealed class GameStageNotePlaneController : MonoBehaviour
    {
        [SerializeField] GameStageController _stage;

        public Transform ContentTransform => transform;

        public Plane Plane => new Plane(transform.up, transform.position);

        public (float X, float Z) Origin => (0f, 0f);

        public Vector3 GetWorldPosition(float x, float z)
        {
            return new Vector3(x, 0f, z);
        }

        public bool TryRaycast(Ray ray, out Vector3 hitPoint)
        {
            if (Plane.Raycast(ray, out var distance)) {
                hitPoint = ray.GetPoint(distance);
                return true;
            }
            else {
                hitPoint = default;
                return false;
            }
        }
    }
}