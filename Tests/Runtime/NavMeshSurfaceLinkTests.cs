#if UNITY_EDITOR || UNITY_STANDALONE

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    class NavMeshSurfaceLinkTests
    {
        GameObject plane1, plane2;
        NavMeshLink link;
        NavMeshSurface surface;
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        NavMeshAgent agent;
#endif

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            plane1 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane2 = GameObject.CreatePrimitive(PrimitiveType.Plane);
            plane1.transform.position = 11.0f * Vector3.right;

            surface = new GameObject("Surface").AddComponent<NavMeshSurface>();
            surface.BuildNavMesh();

            Assume.That(HasPathConnecting(plane1, plane2), Is.False);
            Assume.That(HasPathConnecting(plane2, plane1), Is.False);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            agent = new GameObject("Agent").AddComponent<NavMeshAgent>();
            agent.acceleration *= 10f;
            agent.speed *= 10f;
#endif
        }

        [SetUp]
        public void SetUp()
        {
            link = new GameObject("Link").AddComponent<NavMeshLink>();
            link.startPoint = plane1.transform.position;
            link.endPoint = plane2.transform.position;

            Assume.That(HasPathConnecting(plane1, plane2), Is.True);
            Assume.That(HasPathConnecting(plane2, plane1), Is.True);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            agent.ResetPath();
            agent.Warp(plane2.transform.position - Vector3.back);
#endif
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(link.gameObject);
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(surface.gameObject);
            Object.DestroyImmediate(plane1);
            Object.DestroyImmediate(plane2);
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            Object.DestroyImmediate(agent);
#endif
        }

        [Test]
        public void NavMeshLinkCanConnectTwoSurfaces()
        {
            Assert.IsTrue(HasPathConnecting(plane1, plane2));
        }

        [Test]
        public void DisablingBidirectionalMakesTheLinkOneWay()
        {
            link.bidirectional = false;
            Assert.IsTrue(HasPathConnecting(plane1, plane2));
            Assert.IsFalse(HasPathConnecting(plane2, plane1));
        }

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        [UnityTest]
        public IEnumerator NavMeshLinkIsOccupiedOnlyDuringAgentTraversal()
        {
            agent.SetDestination(plane1.transform.position + Vector3.back);

            while (!agent.isOnOffMeshLink)
            {
                Assert.IsFalse(link.occupied, "Link is occupied, but the agent hasn't arrived to the link yet.");
                yield return null;
            }

            while (agent.isOnOffMeshLink)
            {
                Assert.IsTrue(link.occupied, "Link is not occupied, but the agent is on the link.");
                yield return null;
            }

            Assert.IsFalse(link.occupied, "Link is occupied, but agent has left the link.");
        }
#endif
        [Test]
        public void ChangingAreaTypeCanBlockPath()
        {
            var areaMask = ~(1 << 4);
            Assert.IsTrue(HasPathConnecting(plane1, plane2, areaMask));

            link.area = 4;
            Assert.IsFalse(HasPathConnecting(plane1, plane2, areaMask));
        }

        [Test]
        public void EndpointsMoveRelativeToLinkOnUpdate()
        {
            link.transform.position += Vector3.forward;
            Assert.IsFalse(HasPathConnectingViaPoint(plane1, plane2, plane1.transform.position + Vector3.forward));
            Assert.IsFalse(HasPathConnectingViaPoint(plane1, plane2, plane2.transform.position + Vector3.forward));

            link.UpdateLink();

            Assert.IsTrue(HasPathConnectingViaPoint(plane1, plane2, plane1.transform.position + Vector3.forward));
            Assert.IsTrue(HasPathConnectingViaPoint(plane1, plane2, plane2.transform.position + Vector3.forward));
        }

        [UnityTest]
        public IEnumerator EndpointsMoveRelativeToLinkNextFrameWhenAutoUpdating()
        {
            link.transform.position += Vector3.forward;
            link.autoUpdate = true;

            Assert.IsFalse(HasPathConnectingViaPoint(plane1, plane2, plane1.transform.position + Vector3.forward));
            Assert.IsFalse(HasPathConnectingViaPoint(plane1, plane2, plane2.transform.position + Vector3.forward));

            yield return null;

            Assert.IsTrue(HasPathConnectingViaPoint(plane1, plane2, plane1.transform.position + Vector3.forward));
            Assert.IsTrue(HasPathConnectingViaPoint(plane1, plane2, plane2.transform.position + Vector3.forward));
        }

        [Test]
        public void ChangingCostModifierAffectsRoute()
        {
            var link1 = link;
            link1.startPoint = plane1.transform.position;
            link1.endPoint = plane2.transform.position + Vector3.forward;

            var link2 = link.gameObject.AddComponent<NavMeshLink>();
            link2.startPoint = plane1.transform.position;
            link2.endPoint = plane2.transform.position - Vector3.forward;

            link1.costModifier = -1;
            link2.costModifier = 100;
            Assert.IsTrue(HasPathConnectingViaPoint(plane1, plane2, link1.endPoint));
            Assert.IsFalse(HasPathConnectingViaPoint(plane1, plane2, link2.endPoint));

            link1.costModifier = 100;
            link2.costModifier = -1;
            Assert.IsFalse(HasPathConnectingViaPoint(plane1, plane2, link1.endPoint));
            Assert.IsTrue(HasPathConnectingViaPoint(plane1, plane2, link2.endPoint));
        }

        static bool HasPathConnecting(GameObject a, GameObject b, int areaMask = NavMesh.AllAreas, int agentTypeID = 0)
        {
            var path = new NavMeshPath();
            var filter = new NavMeshQueryFilter();
            filter.areaMask = areaMask;
            filter.agentTypeID = agentTypeID;
            NavMesh.CalculatePath(a.transform.position, b.transform.position, filter, path);
            return path.status == NavMeshPathStatus.PathComplete;
        }

        static bool HasPathConnectingViaPoint(GameObject a, GameObject b, Vector3 point, int areaMask = NavMesh.AllAreas, int agentTypeID = 0)
        {
            var path = new NavMeshPath();
            var filter = new NavMeshQueryFilter();
            filter.areaMask = areaMask;
            filter.agentTypeID = agentTypeID;
            NavMesh.CalculatePath(a.transform.position, b.transform.position, filter, path);
            if (path.status != NavMeshPathStatus.PathComplete)
                return false;

            for (int i = 0; i < path.corners.Length; ++i)
                if (Vector3.Distance(path.corners[i], point) < 0.1f)
                    return true;
            return false;
        }
    }
}
#endif
