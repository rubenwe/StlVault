using System.Collections;
using StlVault.Util.Unity;
using UnityEngine;

#pragma warning disable 0649

namespace RuntimeSceneGizmo
{
	public class CameraGizmoListener : MonoBehaviour
	{

		[SerializeField] private float _projectionTransitionSpeed = 2f;
		[SerializeField] private MaxCamera _maxCamera;

		private Camera _mainCamera;
		private IEnumerator _projectionChangeCoroutine;

		private void Awake()
		{
			_mainCamera = Camera.main;
		}

		private void OnDisable()
		{
			_projectionChangeCoroutine = null;
		}

		public void OnGizmoComponentClicked( GizmoComponent component )
		{
			if( component == GizmoComponent.Center )
				SwitchOrthographicMode();
			else if (component == GizmoComponent.XNegative)
				_maxCamera.SetRotation(90, 0);
			else if( component == GizmoComponent.XPositive )
				_maxCamera.SetRotation(-90, 0);
			else if( component == GizmoComponent.YNegative )
				_maxCamera.SetRotation(_maxCamera.XDeg, -90);
			else if( component == GizmoComponent.YPositive )
				_maxCamera.SetRotation(_maxCamera.XDeg, 90);
			else if( component == GizmoComponent.ZNegative )
				_maxCamera.SetRotation(0, 0);
			else
				_maxCamera.SetRotation(180, 0);
		}

		private void SwitchOrthographicMode()
		{
			if( _projectionChangeCoroutine != null ) return;

			_projectionChangeCoroutine = SwitchProjection();
			StartCoroutine( _projectionChangeCoroutine );
		}

		// Credit: https://forum.unity.com/threads/smooth-transition-between-perspective-and-orthographic-modes.32765/#post-212814
		private IEnumerator SwitchProjection()
		{
			var isOrthographic = _mainCamera.orthographic;

			Matrix4x4 dest, src = _mainCamera.projectionMatrix;
			if( isOrthographic )
				dest = Matrix4x4.Perspective( _mainCamera.fieldOfView, _mainCamera.aspect, _mainCamera.nearClipPlane, _mainCamera.farClipPlane );
			else
			{
				float orthographicSize = _mainCamera.orthographicSize;
				float aspect = _mainCamera.aspect;
				dest = Matrix4x4.Ortho( -orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, _mainCamera.nearClipPlane, _mainCamera.farClipPlane );
			}

			for( float t = 0f; t < 1f; t += Time.unscaledDeltaTime * _projectionTransitionSpeed )
			{
				float lerpModifier = isOrthographic ? t * t : Mathf.Pow( t, 0.2f );
				Matrix4x4 matrix = new Matrix4x4();
				for( int i = 0; i < 16; i++ )
					matrix[i] = Mathf.LerpUnclamped( src[i], dest[i], lerpModifier );

				_mainCamera.projectionMatrix = matrix;
				yield return null;
			}

			_mainCamera.orthographic = !isOrthographic;
			_mainCamera.ResetProjectionMatrix();

			_projectionChangeCoroutine = null;
		}
	}
}