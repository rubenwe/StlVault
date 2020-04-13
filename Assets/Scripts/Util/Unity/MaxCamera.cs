using StlVault.Views;
using UnityEngine;
using UnityEngine.EventSystems;

#pragma warning disable 0649

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
        [SerializeField] private float _startX;
        [SerializeField] private float _startY;
        [SerializeField] private ViewPort _viewPort;

        private float _xDeg;
        private float _yDeg;
        private float _currentDistance;
        private float _desiredDistance;
        private Quaternion _currentRotation;
        private Quaternion _desiredRotation;
        private Quaternion _rotation;
        private Vector3 _position;
        private Camera _camera;
        private EventSystem _eventSystem;

        public float XDeg => _xDeg;
        public float YDeg => _yDeg;

        private void Start() => Init();
        private void OnEnable() => Init();

        private void Init()
        {
            _currentDistance = Vector3.Distance(transform.position, _target.position);
            _desiredDistance = _distance;
 
            //be sure to grab the current rotations as starting points.
            _position = transform.position;
            _rotation = transform.rotation;
            _currentRotation = transform.rotation;
            _desiredRotation = transform.rotation;

            _xDeg = _startX;
            _yDeg = _startY;
            _camera = GetComponentInChildren<Camera>();
        }
 
        private void LateUpdate()
        {
            if (_viewPort.ContainsMousePointer)
            {
                // If Control and Alt and Middle button? ZOOM!
                if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
                {
                    _desiredDistance -= Input.GetAxis("Mouse Y") * Time.deltaTime * _zoomRate * 0.125f * Mathf.Abs(_desiredDistance);
                }
                // If middle mouse and left alt are selected? ORBIT
                else if (Input.GetMouseButton(2) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.LeftShift)))
                {
                    _xDeg += Input.GetAxis("Mouse X") * _xSpeed * 0.02f;
                    _yDeg -= Input.GetAxis("Mouse Y") * _ySpeed * 0.02f;
                }
                // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
                else if (Input.GetMouseButton(2))
                {
                    //grab the rotation of the camera so we can move in a psuedo local XY space
                    _target.rotation = transform.rotation;
                    _target.Translate(-Input.GetAxis("Mouse X") * _panSpeed * Vector3.right);
                    _target.Translate(-Input.GetAxis("Mouse Y") * _panSpeed * transform.up, Space.World);
                }

                // affect the desired Zoom distance if we roll the scrollwheel
                _desiredDistance -= Input.GetAxis("Mouse ScrollWheel") * Time.deltaTime * _zoomRate * Mathf.Abs(_desiredDistance);
            }

            ////////OrbitAngle
 
            //Clamp the vertical axis for the orbit
            _yDeg = ClampAngle(_yDeg, _yMinLimit, _yMaxLimit);
            // set camera rotation 
            _desiredRotation = Quaternion.Euler(_yDeg, _xDeg, 0);
            _currentRotation = transform.rotation;
 
            _rotation = Quaternion.Lerp(_currentRotation, _desiredRotation, Time.deltaTime * _zoomDampening);
            transform.rotation = _rotation;
            
            ////////Orbit Position
 
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

        public void SetRotation(float x, float y)
        {
            _xDeg = x;
            _yDeg = y;
        }
    }
}