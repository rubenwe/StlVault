using UnityEngine;

namespace StlVault.Util.Unity
{
    public class MaxCamera : MonoBehaviour
    {
        [SerializeField] private Transform _target;
        [SerializeField] private Vector3 _targetOffset;
        [SerializeField] private float _distance = 5.0f;
        [SerializeField] private float _maxDistance = 20;
        [SerializeField] private float _minDistance = .6f;
        [SerializeField] private float _xSpeed = 200.0f;
        [SerializeField] private float _ySpeed = 200.0f;
        [SerializeField] private int _yMinLimit = -80;
        [SerializeField] private int _yMaxLimit = 80;
        [SerializeField] private int _zoomRate = 40;
        [SerializeField] private float _panSpeed = 0.3f;
        [SerializeField] private float _zoomDampening = 5.0f;
 
        private float _xDeg = 0.0f;
        private float _yDeg = 0.0f;
        private float _currentDistance;
        private float _desiredDistance;
        private Quaternion _currentRotation;
        private Quaternion _desiredRotation;
        private Quaternion _rotation;
        private Vector3 _position;
        private Camera _camera;

        private void Start() => Init();
        private void OnEnable() => Init();

        private void Init()
        {
            _distance = Vector3.Distance(transform.position, _target.position);
            _currentDistance = _distance;
            _desiredDistance = _distance;
 
            //be sure to grab the current rotations as starting points.
            _position = transform.position;
            _rotation = transform.rotation;
            _currentRotation = transform.rotation;
            _desiredRotation = transform.rotation;
 
            _xDeg = Vector3.Angle(Vector3.right, transform.right );
            _yDeg = Vector3.Angle(Vector3.up, transform.up );
            _camera = GetComponent<Camera>();
        }
 
        private void LateUpdate()
        {
            // If Control and Alt and Middle button? ZOOM!
            if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
            {
                _desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * _zoomRate*0.125f * Mathf.Abs(_desiredDistance);
            }
            // If middle mouse and left alt are selected? ORBIT
            else if (Input.GetMouseButton(2) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftShift)))
            {
                _xDeg += Input.GetAxis("Mouse X") * _xSpeed * 0.02f;
                _yDeg -= Input.GetAxis("Mouse Y") * _ySpeed * 0.02f;
 
                ////////OrbitAngle
 
                //Clamp the vertical axis for the orbit
                _yDeg = ClampAngle(_yDeg, _yMinLimit, _yMaxLimit);
                // set camera rotation 
                _desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0);
                _currentRotation = transform.rotation;
 
                _rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * _zoomDampening);
                transform.rotation = _rotation;
            }
            // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
            else if (Input.GetMouseButton(2))
            {
                //grab the rotation of the camera so we can move in a psuedo local XY space
                _target.rotation = transform.rotation;
                _target.Translate(-Input.GetAxis("Mouse X") * _panSpeed * Vector3.right);
                _target.Translate(-Input.GetAxis("Mouse Y") * _panSpeed * transform.up, Space.World);
            }
 
            ////////Orbit Position
 
            // affect the desired Zoom distance if we roll the scrollwheel
            _desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _zoomRate * Mathf.Abs(_desiredDistance);
            //clamp the zoom min/max
            _desiredDistance = Mathf.Clamp(_desiredDistance, _minDistance, _maxDistance);
            // For smoothing of the zoom, lerp distance
            _currentDistance = Mathf.Lerp(_currentDistance, _desiredDistance, Time.deltaTime * _zoomDampening);
 
            // calculate position based on the new currentDistance 
            _position = _target.position - (_rotation * Vector3.forward * _currentDistance + _targetOffset);
            transform.position = _position;
            _camera.orthographicSize = _currentDistance / 2;

        }
 
        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360) angle += 360;
            if (angle > 360) angle -= 360;

            return Mathf.Clamp(angle, min, max);
        }
    }
}