using System.Collections.Generic;
using UnityEngine;

public class Rope : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Camera mainCamera;
    public GameObject nectLineObj;
    private List<Vector3> linePoints = new List<Vector3>();
    public float distance;
    private bool isDrawing = false;
    private Vector3 lastPoint;
    private bool lineStraightened = false;

    public float maxDistance = 100f;
    public LayerMask connectorLayerMask;
    public LayerMask drawerLayerMask;

    // Variables to check if two wires are connected
    

    void Start()
    {
        // Initialize LineRenderer
        if (nectLineObj != null)
        {
            nectLineObj.SetActive(false);
        }
        lineRenderer.positionCount = 0;
         // Make sure the bulb is off at the start
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !lineStraightened)
        {
            Vector3 mousePos = GetMouseWorldPosition();

            if (CheckForDrawer(mousePos))
            {
                Vector3 drawerStartPoint = GetDrawerStartPosition(mousePos);
                StartDrawingFromDrawer(drawerStartPoint);
                return;
            }

            if (!isDrawing)
            {
                if (CheckForConnector(mousePos) || CheckForResistor(mousePos))
                {
                    FinalizeLineAtConnector(mousePos);
                    return;
                }

                if (linePoints.Count > 0 && lineStraightened)
                {
                    AddPointToLine(mousePos);
                }
                else
                {
                    AddPointToLine(mousePos);
                }

                lineStraightened = false;
            }
            isDrawing = true;
        }

        if (Input.GetMouseButton(0) && isDrawing && !lineStraightened)
        {
            Vector3 mousePos = GetMouseWorldPosition();

            if (CheckForConnector(mousePos) || CheckForResistor(mousePos))
            {
                FinalizeLineAtConnector(mousePos);
                return;
            }

            if (Vector3.Distance(linePoints[linePoints.Count - 1], mousePos) > 0.1f)
            {
                AddPointToLine(mousePos);
            }
        }

        if (Input.GetMouseButtonUp(0) && isDrawing && !lineStraightened)
        {
            Vector3 currentMousePos = GetMouseWorldPosition();

            if (linePoints.Count > 1)
            {
                AddPointToLine(currentMousePos);
            }

            isDrawing = false;
            lineStraightened = true;
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = distance;
        return mainCamera.ScreenToWorldPoint(mousePos);
    }

    private void AddPointToLine(Vector3 point)
    {
        linePoints.Add(point);
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    private bool CheckForConnector(Vector3 mousePos)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, connectorLayerMask))
        {
            if (hit.collider.CompareTag("Connector"))
            {
              //  Debug.Log("Connector hit! Stopping line drawing.");
                if (nectLineObj != null) { nectLineObj.gameObject.SetActive(true); }
              //  Debug.Log(hit.transform.parent.gameObject.name);
                return true;
            }
        }
        return false;
    }

    private bool CheckForResistor(Vector3 mousePos)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, connectorLayerMask))
        {
            if (hit.collider.CompareTag("Resistor"))
            {
                if (nectLineObj != null) { nectLineObj.gameObject.SetActive(true); }
               // Debug.Log("Resistor hit! Stopping line drawing.");
              
                return true;
            }
        }
        return false;
    }

    private bool CheckForDrawer(Vector3 mousePos)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, drawerLayerMask))
        {
            if (hit.collider.CompareTag("Drawer"))
            {
              //  Debug.Log("Drawer hit! Starting line drawing from drawer.");
                return true;
            }
        }
        return false;
    }

    private Vector3 GetDrawerStartPosition(Vector3 mousePos)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, drawerLayerMask))
        {
            if (hit.collider.CompareTag("Drawer"))
            {
                return hit.point;
            }
        }
        return Vector3.zero;
    }

    private void StartDrawingFromDrawer(Vector3 drawerStartPoint)
    {
        AddPointToLine(drawerStartPoint);
        isDrawing = true;
        lineStraightened = false;
    }

    private void FinalizeLineAtConnector(Vector3 connectorPoint)
    {
        Vector3 lastLinePoint = linePoints[linePoints.Count - 1];
        linePoints.Add(connectorPoint);

        List<Vector3> curvedLine = GetCurvedLine(lastLinePoint, connectorPoint);
        linePoints.RemoveAt(linePoints.Count - 1);
        linePoints.AddRange(curvedLine);

        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());

        isDrawing = false;
        lineStraightened = true;
    }

    private List<Vector3> GetCurvedLine(Vector3 start, Vector3 end)
    {
        List<Vector3> curvePoints = new List<Vector3>();

        Vector3 controlPoint1 = Vector3.Lerp(start, end, 0.3f) + Vector3.up * 0.1f;
        Vector3 controlPoint2 = Vector3.Lerp(start, end, 0.7f) + Vector3.down * 0.1f;

        int segments = 20;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 pointOnCurve = CalculateBezierPoint(t, start, controlPoint1, controlPoint2, end);
            curvePoints.Add(pointOnCurve);
        }

        return curvePoints;
    }

    private Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

   
}
