using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArmisDev
{
	[RequireComponent(typeof(LineRenderer))]
	public class BulletLine : MonoBehaviour
	{
		public LineRenderer lineRenderer;
		public float speed = .2f;

		public void SetPositions(Vector3 p1, Vector3 p2)
		{
			lineRenderer.positionCount = 2;
			lineRenderer.SetPosition(0, p1);
			lineRenderer.SetPosition(1, p2);
		}

		private void OnEnable()
		{
			if (lineRenderer == null)
			{
				lineRenderer = GetComponent<LineRenderer>();
			}

			lineRenderer.widthMultiplier = 1;
		}

		private void Update()
		{
			if (lineRenderer.widthMultiplier <= 0)
			{
				gameObject.SetActive(false);
			}
			else
			{
				lineRenderer.widthMultiplier -= Time.deltaTime / speed;
			}
		}
	}
}
