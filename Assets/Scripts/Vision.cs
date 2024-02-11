using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vision : MonoBehaviour
{

    [SerializeField] private float fov;
    [SerializeField] private int rayCount;
    [SerializeField] private float viewDistance;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float startingAngle;
    private Mesh mesh;
    [SerializeField] private Vector3 origin = Vector3.zero;

    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    
    //Much of this code was created with the help of the following youtube tutorial: https://www.youtube.com/watch?v=CSeUMTaNFYk
    private void LateUpdate()
    {
        float angle = startingAngle;
        float angleIncrease = fov / rayCount;

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i <= rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycasthit2D = Physics2D.Raycast(origin, AngleToVector3(angle), viewDistance, ~playerLayer);

            if (raycasthit2D.collider == null)
            {
                // didn't hit anything
                vertex = origin + AngleToVector3(angle) * viewDistance;
            }
            else
            {
                // ruh roh we hit something
                vertex = raycasthit2D.point;
            }
            vertices[vertexIndex] = vertex;

            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }

            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }



    private Vector3 AngleToVector3(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180f);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

    private float Vector3ToAngle(Vector3 dir)
    {
        dir = dir.normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        if (angle < 0) angle += 360;

        return angle;
    }

    public void SetOrigin(Vector3 origin)
    {
        this.origin = origin;
    }

    public void SetAim(Vector3 aimDirection)
    {
        float AngleRad = Mathf.Atan2(aimDirection.y - origin.y, aimDirection.x - origin.x);

        float AngleDeg = (180 / Mathf.PI) *AngleRad;

        startingAngle = AngleDeg + fov / 2f;
    }
}
