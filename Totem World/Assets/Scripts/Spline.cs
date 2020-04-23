using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Spline : MonoBehaviour
{

	[AssetsOnly]
	public GameObject pointPrefab;

	[ToggleLeft]
	public bool generateOnStart;
	
	[HorizontalGroup("endpoints", LabelWidth = 60)]
	public Transform start;
	[HorizontalGroup("endpoints")]
	public Transform end;
	
	[Range(1, 50)]
	public int pointCount = 6;
	
	[ReadOnly]
	public List<GameObject> points = new List<GameObject>();

	LineRenderer _lineRenderer;


	void Start()
	{
		if (generateOnStart) Generate();
	}

	public void Generate()
	{
		UpdatePointsCount();
		InitializePointPositions();
		PrepSpringJointsForMovement();
	}


	// Update is called once per frame
	void Update ()
	{
		RenderSpline();
		UpdateEndpointPositions();
	}

	[Button()]
	void Refresh()
	{
		UpdatePointsCount();
		InitializePointPositions();
		RenderSpline();
	}

	void UpdatePointsCount()
	{

		for (int i = points.Count - 1; i >= 0; i--)
		{
			if (points[i] == null) continue;
			
			if (Application.isPlaying)
				Destroy(points[i]);
			else 
				DestroyImmediate(points[i]);
		}
		
		points.Clear();
	
		// add points
		for (int i = 0; i < pointCount; i++)
		{
			GameObject newPoint = Instantiate(pointPrefab, transform);
			newPoint.name = "point_" + i;
			points.Add(newPoint);

			// If this is an end point
			if (i == 0 || i == pointCount - 1)
			{
				Rigidbody2D rb = newPoint.GetComponent<Rigidbody2D>();
				rb.isKinematic = true;
			}

			if (i > 0)
			{
				SpringJoint2D sj = newPoint.GetComponent<SpringJoint2D>();
				sj.connectedBody = points[i - 1].GetComponent<Rigidbody2D>();
			}
		}
	}

	void InitializePointPositions()
	{
		float progress;
		for (int i = 0; i < points.Count; i++)
		{
			progress = (float) i / points.Count;

			points[i].transform.position = Vector3.Lerp(start.position, end.position, progress);
		}
	}

	void PrepSpringJointsForMovement()
	{
		foreach (var p in points)
		{
			SpringJoint2D sj = p.GetComponent<SpringJoint2D>();
			sj.autoConfigureDistance = false;
			sj.autoConfigureConnectedAnchor = false;
		}
	}

	void UpdateEndpointPositions()
	{
		points[0].transform.position = start.position;
		points[points.Count - 1].transform.position = end.position;
	}

	void RenderSpline()
	{
		if (!_lineRenderer) _lineRenderer = GetComponent<LineRenderer>();

		_lineRenderer.positionCount = points.Count;
		
		for (int i = 0; i < points.Count; i++)
		{
			_lineRenderer.SetPosition(i, points[i].transform.position);
		}
	}

	/// <summary>
	/// Returns a point on the curve, assuming you're lerping across the whole thing from 0 (start) to 1 (end)
	/// </summary>
	public Vector3 PointOnSpline(float progress)
	{
		progress = Mathf.Clamp01(progress);

		float segmentLength = 1 / ((float) pointCount - 1);
		
		// see how many segments the given progress is already passed 
		int startPoint = Mathf.FloorToInt(progress / segmentLength);
		int endPoint = Mathf.Clamp(startPoint + 1, startPoint, pointCount - 1);

		if (points.Count < endPoint) return transform.position;
		
		Vector3 start = points[startPoint].transform.position;
		Vector3 end = points[endPoint].transform.position;

		float remainder = progress % segmentLength;
		float segmentProgress = remainder / segmentLength;

		return Vector3.Lerp(start, end, segmentProgress);
	}

}
