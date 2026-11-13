using UnityEngine;

namespace MultiverseArcade.Syafiq
{
    /// <summary>
    /// Kamera top-down bersudut miring (55 derajat) yang mengikuti pemain dengan smooth damp
    /// dan sedikit look-ahead ke arah pointer mouse.
    /// Sesuai GDD Bagian 5.10.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        [Header("Target & Follow")]
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _offset = new Vector3(0f, 13f, -9f);
        [SerializeField] private float _smoothTime = 0.15f;
        [SerializeField] private float _lookAheadFactor = 0.15f;
        [SerializeField] private float _cameraAngleX = 55f;

        private Vector3 _currentVelocity;
        private Camera _cam;
        private Plane _groundPlane;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _groundPlane = new Plane(Vector3.up, Vector3.zero);
            transform.rotation = Quaternion.Euler(_cameraAngleX, 0f, 0f);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
            if (_target != null)
            {
                transform.position = _target.position + _offset;
            }
        }

        private void LateUpdate()
        {
            if (_target == null) return;

            Vector3 targetPos = _target.position + _offset;

            // Look-ahead menuju kursor mouse
            if (_cam != null)
            {
                Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
                if (_groundPlane.Raycast(ray, out float enter))
                {
                    Vector3 mouseFloorPos = ray.GetPoint(enter);
                    Vector3 lookAheadVec = (mouseFloorPos - _target.position) * _lookAheadFactor;
                    lookAheadVec.y = 0f;
                    targetPos += lookAheadVec;
                }
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _currentVelocity, _smoothTime);
        }
    }
}
