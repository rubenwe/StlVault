using System.Collections;
using StlVault.Util.Unity;
using UnityEngine;

namespace RuntimeSceneGizmo
{
	public class CameraGizmoListener : MonoBehaviour
	{
#pragma warning disable 0649
		[SerializeField] private float cameraAdjustmentSpeed = 3f;
		[SerializeField] private float projectionTransitionSpeed = 2f;
		[SerializeField] private MaxCamera _maxCamera;
#pragma warning restore 0649

		private Camera mainCamera;
		private Transform mainCamParent;

		private IEnumerator cameraRotateCoroutine, projectionChangeCoroutine;

		private void Awake()
		{
			mainCamera = Camera.main;
			mainCamParent = mainCamera.transform.parent;
		}

		private void OnDisable()
		{
			cameraRotateCoroutine = projectionChangeCoroutine = null;
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

		public void SwitchOrthographicMode()
		{
			if( projectionChangeCoroutine != null )
				return;

			projectionChangeCoroutine = SwitchProjection();
			StartCoroutine( projectionChangeCoroutine );
		}

		

		// Credit: https://forum.unity.com/threads/smooth-transition-between-perspective-and-orthographic-modes.32765/#post-212814
		private IEnumerator SwitchProjection()
		{
			bool isOrthographic = mainCamera.orthographic;

			Matrix4x4 dest, src = mainCamera.projectionMatrix;
			if( isOrthographic )
				dest = Matrix4x4.Perspective( mainCamera.fieldOfView, mainCamera.aspect, mainCamera.nearClipPlane, mainCamera.farClipPlane );
			else
			{
				float orthographicSize = mainCamera.orthographicSize;
				float aspect = mainCamera.aspect;
				dest = Matrix4x4.Ortho( -orthographicSize * aspect, orthographicSize * aspect, -orthographicSize, orthographicSize, mainCamera.nearClipPlane, mainCamera.farClipPlane );
			}

			for( float t = 0f; t < 1f; t += Time.unscaledDeltaTime * projectionTransitionSpeed )
			{
				float lerpModifier = isOrthographic ? t * t : Mathf.Pow( t, 0.2f );
				Matrix4x4 matrix = new Matrix4x4();
				for( int i = 0; i < 16; i++ )
					matrix[i] = Mathf.LerpUnclamped( src[i], dest[i], lerpModifier );

				mainCamera.projectionMatrix = matrix;
				yield return null;
			}

			mainCamera.orthographic = !isOrthographic;
			mainCamera.ResetProjectionMatrix();

			projectionChangeCoroutine = null;
		}

		
	}
}