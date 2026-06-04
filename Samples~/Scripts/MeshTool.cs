using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine.InputSystem;

namespace Unity.AI.Navigation.Samples
{
    /// <summary>
    /// Manipulation tool for displacing the vertices in a list of meshes
    /// </summary>
    [DefaultExecutionOrder(-101)]
    public class MeshTool : MonoBehaviour
    {
        NavMeshSurface m_Surface;

        public enum ExtrudeMethod
        {
            Vertical,
            MeshNormal
        }

        public List<MeshFilter> m_Filters = new List<MeshFilter>();
        public float m_Radius = 1.5f;
        public float m_Power = 2.0f;
        public ExtrudeMethod m_Method = ExtrudeMethod.Vertical;

        RaycastHit m_HitInfo = new RaycastHit();
        AsyncOperation m_LastNavMeshUpdate;

        void OnEnable()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            m_Surface = GetComponent<NavMeshSurface>();
            if (m_Surface != null)
            {
                m_Surface.BuildNavMesh();
                m_LastNavMeshUpdate = m_Surface.UpdateNavMesh(m_Surface.navMeshData);
            }
        }

        void Update()
        {
            var mouse = Mouse.current;
            var keyboard = Keyboard.current;

            var leftPressed = mouse != null && mouse.leftButton.isPressed;
            var rightPressed = mouse != null && mouse.rightButton.isPressed;
            var spacePressed = keyboard != null && keyboard.spaceKey.isPressed;
            var shiftPressed = keyboard != null && keyboard.leftShiftKey.isPressed;
            var leftReleased = mouse != null && mouse.leftButton.wasReleasedThisFrame;
            var rightReleased = mouse != null && mouse.rightButton.wasReleasedThisFrame;
            var spaceReleased = keyboard != null && keyboard.spaceKey.wasReleasedThisFrame;

            var ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
            if (Physics.Raycast(ray.origin, ray.direction, out m_HitInfo))
            {
                Debug.DrawRay(m_HitInfo.point, m_HitInfo.normal, Color.red);
                Vector3 displacement = (m_Method == ExtrudeMethod.Vertical) ? Vector3.up : m_HitInfo.normal;

                if (leftPressed || (spacePressed && !shiftPressed))
                {
                    ModifyMesh(m_Power * displacement, m_HitInfo.point);
                    if (m_Surface != null)
                    {
                        if (m_LastNavMeshUpdate.isDone)
                            m_LastNavMeshUpdate = m_Surface.UpdateNavMesh(m_Surface.navMeshData);
                    }

                }
                else if (rightPressed || (spacePressed && shiftPressed))
                {
                    ModifyMesh(-m_Power * displacement, m_HitInfo.point);
                    if (m_Surface != null)
                    {
                        if (m_LastNavMeshUpdate.isDone)
                            m_LastNavMeshUpdate = m_Surface.UpdateNavMesh(m_Surface.navMeshData);
                    }
                }
                else if (leftReleased || rightReleased || spaceReleased)
                {
                    if (m_Surface != null)
                    {
                        if (!m_LastNavMeshUpdate.isDone)
                            NavMeshBuilder.Cancel(m_Surface.navMeshData);

                        m_LastNavMeshUpdate = m_Surface.UpdateNavMesh(m_Surface.navMeshData);
                    }
                }
            }
        }

        void ModifyMesh(Vector3 displacement, Vector3 center)
        {
            foreach (var filter in m_Filters)
            {
                Mesh mesh = filter.mesh;
                Vector3[] vertices = mesh.vertices;

                for (int i = 0; i < vertices.Length; ++i)
                {
                    Vector3 v = filter.transform.TransformPoint(vertices[i]);
                    vertices[i] = vertices[i] + displacement * Gaussian(v, center, m_Radius);
                }

                mesh.vertices = vertices;
                mesh.RecalculateBounds();

                var col = filter.GetComponent<MeshCollider>();
                if (col != null)
                {
                    var colliMesh = new Mesh();
                    colliMesh.vertices = mesh.vertices;
                    colliMesh.triangles = mesh.triangles;
                    col.sharedMesh = colliMesh;
                }
            }
        }

        static float Gaussian(Vector3 pos, Vector3 mean, float dev)
        {
            float x = pos.x - mean.x;
            float y = pos.y - mean.y;
            float z = pos.z - mean.z;
            float n = 1.0f / (2.0f * Mathf.PI * dev * dev);
            return n * Mathf.Pow(2.718281828f, -(x * x + y * y + z * z) / (2.0f * dev * dev));
        }
    }
}
