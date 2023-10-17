#if UNITY_EDITOR || UNITY_STANDALONE

using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using Object = UnityEngine.Object;

namespace Unity.AI.Navigation.Tests
{
    [TestFixture]
    class NavMeshSurfaceLinkTests
    {
        GameObject m_PlaneAtOrigin;
        GameObject m_PlaneOnTheSide;
        NavMeshLink m_Link;
        NavMeshSurface m_Surface;
        readonly Vector3 m_OffsetX = new(11f, 0f, 0f);
        readonly Vector3 m_OffsetZ = new(0f, 0f, 11f);
        readonly Vector3 m_DefaultEndpointOffset = new(0f, 0f, 2.5f);
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        readonly int m_PandaTypeID = NavMesh.CreateSettings().agentTypeID;
        const int k_AreaTypeForPanda = 3;
        NavMeshAgent m_Agent;
        GameObject m_ExtraNavMesh;
        GameObject m_FarFromNavMesh;
        GameObject m_TempGO;
        GameObject m_PathfindingStart;
        GameObject m_PathfindingEnd;
        NavMeshDataInstance m_NavMeshClone;
        NavMeshDataInstance m_NavMeshForPanda;
        NavMeshSurface m_SurfaceForPanda;
#endif

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            m_PlaneAtOrigin = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_PlaneOnTheSide = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_PlaneOnTheSide.transform.position = m_OffsetX;
            m_PlaneOnTheSide.transform.localScale = 0.4f * Vector3.one;
            m_PlaneAtOrigin.transform.localScale = 0.4f * Vector3.one;

            m_Surface = new GameObject("Surface").AddComponent<NavMeshSurface>();
            m_Surface.BuildNavMesh();

            Assume.That(HasPathConnecting(m_PlaneAtOrigin, m_PlaneOnTheSide), Is.False);
            Assume.That(HasPathConnecting(m_PlaneOnTheSide, m_PlaneAtOrigin), Is.False);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            m_Agent = new GameObject("Agent").AddComponent<NavMeshAgent>();
            m_Agent.transform.position = m_PlaneOnTheSide.transform.position - 2f * Vector3.back;
            m_Agent.enabled = false;
            m_Agent.acceleration = 100f;
            m_Agent.speed = 10f;
            Assume.That(m_Agent.speed, Is.LessThan(m_OffsetX.x),
                "Too high of a speed causes the agent to jump straight to the path's end.");

            m_PathfindingStart = new GameObject("Path Start");
            m_PathfindingEnd = new GameObject("Path End");
            m_ExtraNavMesh = new GameObject("Origin of Additional NavMesh")
            {
                transform =
                {
                    position = m_PlaneAtOrigin.transform.position + m_OffsetZ
                }
            };
            m_FarFromNavMesh = new GameObject("Position Far From NavMesh")
            {
                transform =
                {
                    position = m_ExtraNavMesh.transform.position - 2f * m_OffsetX
                }
            };

            m_NavMeshClone = NavMesh.AddNavMeshData(m_Surface.navMeshData, m_ExtraNavMesh.transform.position, Quaternion.identity);

            m_SurfaceForPanda = m_Surface.gameObject.AddComponent<NavMeshSurface>();
            m_SurfaceForPanda.agentTypeID = m_PandaTypeID;
            m_SurfaceForPanda.defaultArea = k_AreaTypeForPanda;
            m_SurfaceForPanda.BuildNavMesh();
            m_SurfaceForPanda.enabled = false;
#endif
        }

        [SetUp]
        public void SetUp()
        {
            // move m_Link to OneTimeSetup
            m_Link = new GameObject("Link").AddComponent<NavMeshLink>();
            m_Link.transform.position = Vector3.zero;
            m_Link.startPoint = m_PlaneAtOrigin.transform.position;
            m_Link.endPoint = m_PlaneOnTheSide.transform.position;
            m_Link.UpdateLink();

            Assume.That(HasPathConnecting(m_PlaneAtOrigin, m_PlaneOnTheSide), Is.True);
            Assume.That(HasPathConnecting(m_PlaneOnTheSide, m_PlaneAtOrigin), Is.True);

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            if (m_Agent.isActiveAndEnabled)
            {
                m_Agent.ResetPath();
                m_Agent.Warp(m_PlaneOnTheSide.transform.position - 2f * Vector3.back);
            }

            m_PathfindingStart.transform.position = m_Link.transform.position + m_Link.startPoint;
            m_PathfindingEnd.transform.position = m_Link.transform.position + m_Link.endPoint;
#endif
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(m_Link.gameObject);
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            if (m_TempGO != null)
                Object.DestroyImmediate(m_TempGO);
            if (m_NavMeshForPanda.valid)
                NavMesh.RemoveNavMeshData(m_NavMeshForPanda);
            m_Agent.enabled = false;
#endif
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Object.DestroyImmediate(m_Surface.gameObject);
            Object.DestroyImmediate(m_PlaneAtOrigin);
            Object.DestroyImmediate(m_PlaneOnTheSide);
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
            Object.DestroyImmediate(m_Agent);
            Object.DestroyImmediate(m_ExtraNavMesh);
            Object.DestroyImmediate(m_FarFromNavMesh);
            Object.DestroyImmediate(m_PathfindingStart);
            Object.DestroyImmediate(m_PathfindingEnd);
            if (m_NavMeshClone.valid)
                NavMesh.RemoveNavMeshData(m_NavMeshClone);
#endif
        }

        [Test]
        public void Link_WhenCreated_HasDefaultEndpointOffsets()
        {
            var link = m_Link.gameObject.AddComponent<NavMeshLink>();
            Assert.That(link.startPoint, Is.EqualTo(-m_DefaultEndpointOffset).Using(Vector3EqualityComparer.Instance),
                "Newly created NavMeshLink should have the start point located at an offset from the game object.");
            Assert.That(link.endPoint, Is.EqualTo(m_DefaultEndpointOffset).Using(Vector3EqualityComparer.Instance),
                "Newly created NavMeshLink should have the end point located at an offset from the game object.");
        }

        [Test]
        public void Link_WithValidParameters_ConnectsTwoSurfaces()
        {
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_OnUpdateLinkWhileDisabled_DoesNotEnableConnection()
        {
            m_Link.gameObject.SetActive(false);
            VerifyLinkConnection(ResultsIn.NoPath);

            m_Link.UpdateLink();
            VerifyLinkConnection(ResultsIn.NoPath);
        }

        [Test]
        public void Link_WhenPropertyAreaChanges_UpdatesConnectionImmediately()
        {
            Assume.That(HasPathConnecting(m_PlaneAtOrigin, m_PlaneOnTheSide), Is.True);

            m_Link.area = 4;
            var anyAreaExceptTheLinkArea = ~(1 << m_Link.area);
            Assert.IsFalse(HasPathConnecting(m_PlaneAtOrigin, m_PlaneOnTheSide, anyAreaExceptTheLinkArea));
        }

        [Test]
        public void Link_WhenPropertyBidirectionalSwitchedOff_UpdatesConnectionImmediatelyToOneWay()
        {
            Assume.That(HasPathConnecting(m_PlaneOnTheSide, m_PlaneAtOrigin), Is.True);

            m_Link.bidirectional = false;
            VerifyLinkConnection(ResultsIn.PathOnlyForward);
        }

        [Test]
        public void Link_WhenPropertyBidirectionalSwitchedOn_UpdatesConnectionImmediatelyToBothWays()
        {
            m_Link.bidirectional = false;
            Assume.That(HasPathConnecting(m_PlaneOnTheSide, m_PlaneAtOrigin), Is.False);

            m_Link.bidirectional = true;
            VerifyLinkConnection(ResultsIn.PathBothWays);
        }

        [Test]
        public void Link_WhenPropertyCostModifierChanges_UpdatesConnectionImmediately()
        {
            m_Link.gameObject.SetActive(false);
            var fartherLink = m_Link.gameObject.AddComponent<NavMeshLink>();
            fartherLink.startPoint = m_PlaneAtOrigin.transform.position + Vector3.forward;
            fartherLink.endPoint = m_PlaneOnTheSide.transform.position + Vector3.forward;
            m_Link.gameObject.SetActive(true);

            Assume.That(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_Link.endPoint), Is.True);

            m_Link.costModifier = 1000f;

            Assert.IsFalse(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_Link.endPoint),
                "A path should not go through the connection with the higher cost, even if it's closer.");
            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, fartherLink.endPoint),
                "A path should go through the connection with the lower cost, even if it's farther away.");
        }

        [Test]
        public void Link_WhenPropertyStartPointChanged_UpdatesConnectionImmediately()
        {
            m_Link.startPoint = m_OffsetZ;

            m_PathfindingStart.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertyEndPointChanged_UpdatesConnectionImmediately()
        {
            m_Link.endPoint = m_OffsetZ;

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertyWidthChanges_UpdatesConnectionImmediately()
        {
            m_Link.transform.position = 3f * Vector3.forward;
            m_Link.UpdateLink();
            Assume.That(HasPathConnecting(m_PlaneAtOrigin, m_PlaneOnTheSide), Is.False);

            m_Link.width = 6f;
            Assert.IsTrue(HasPathConnecting(m_PlaneAtOrigin, m_PlaneOnTheSide));
        }

#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
        [Test]
        public void Link_WhenPropertyActivatedChanges_UpdatesConnectionImmediately([Values(false, true)] bool activated)
        {
            m_Link.activated = activated;
            var asExpected = activated ? ResultsIn.PathForward : ResultsIn.NoPath;
            VerifyLinkConnection(asExpected);
        }

        [Test]
        [Description("This behavior needed to match the legacy OffMeshLink behavior.")]
        public void Link_WhenStartTransformAssigned_SetsStartPointOffsetToZero()
        {
            m_TempGO = new GameObject("Link");
            var link = m_TempGO.AddComponent<NavMeshLink>();
            Assume.That(link.startPoint, Is.Not.EqualTo(Vector3.zero));

            link.startTransform = m_ExtraNavMesh.transform;
            link.UpdateLink();
            Assert.That(link.startPoint, Is.EqualTo(Vector3.zero),
                "NavMeshLink should have a zero start point after a start transform has been assigned to it.");
            Assume.That(link.endPoint, Is.Not.EqualTo(Vector3.zero), "End point should keep a default offset.");
        }

        [Test]
        [Description("This behavior needed to match the legacy OffMeshLink behavior.")]
        public void Link_WhenEndTransformAssigned_SetsEndPointOffsetToZero()
        {
            m_TempGO = new GameObject("Link");
            var link = m_TempGO.AddComponent<NavMeshLink>();
            Assume.That(link.endPoint, Is.Not.EqualTo(Vector3.zero));

            link.endTransform = m_ExtraNavMesh.transform;
            link.UpdateLink();
            Assert.That(link.endPoint, Is.EqualTo(Vector3.zero),
                "NavMeshLink should have a zero end point after a end transform has been assigned to it.");
            Assume.That(link.startPoint, Is.Not.EqualTo(Vector3.zero), "Start point should keep a default offset.");
        }

        [Test]
        [Description("This behavior is different than the legacy OffMeshLink behavior.")]
        public void Link_WhenTransformRemovedAtStart_KeepsStartPointOffsetUnchanged()
        {
            m_TempGO = new GameObject("Link");
            var pointBefore = new Vector3(1f, 2f, 3f);
            var link = m_TempGO.AddComponent<NavMeshLink>();
            link.startTransform = m_TempGO.transform;
            link.startPoint = pointBefore;

            link.startTransform = null;

            Assert.That(link.startPoint, Is.EqualTo(pointBefore),
                "NavMeshLink should retain the same start point after the start transform has been unassigned.");
            Assume.That(link.endPoint, Is.EqualTo(m_DefaultEndpointOffset),
                "End point should keep a default offset.");
        }

        [Test]
        [Description("This behavior is different than the legacy OffMeshLink behavior.")]
        public void Link_WhenTransformRemovedAtEnd_KeepsEndPointOffsetUnchanged()
        {
            m_TempGO = new GameObject("Link");
            var pointBefore = new Vector3(1f, 2f, 3f);
            var link = m_TempGO.AddComponent<NavMeshLink>();
            link.endTransform = m_TempGO.transform;
            link.endPoint = pointBefore;

            link.endTransform = null;

            Assert.That(link.endPoint, Is.EqualTo(pointBefore),
                "NavMeshLink should retain the same end point after the end transform has been unassigned.");
            Assume.That(link.startPoint, Is.EqualTo(-m_DefaultEndpointOffset),
                "Start point should keep a default offset.");
        }

        [UnityTest]
        public IEnumerator Link_DuringAgentTraversal_ReportsIsOccupied_OtherwiseNotOccupied()
        {
            m_Agent.enabled = true;
            m_Agent.SetDestination(m_PlaneAtOrigin.transform.position + Vector3.back);

            while (!m_Agent.isOnOffMeshLink)
            {
                Assert.IsFalse(m_Link.occupied, "Link is occupied, but the agent hasn't arrived to the link yet.");
                yield return null;
            }

            var framesUntilComplete = 3;
            while (m_Agent.isOnOffMeshLink)
            {
                Assert.IsTrue(m_Link.occupied, "Link is not occupied, but the agent is on the link.");

                if (--framesUntilComplete == 0)
                    m_Agent.CompleteOffMeshLink();
                yield return null;
            }

            Assert.IsFalse(m_Link.occupied, "Link is occupied, but agent has left the link.");
        }

        [Test]
        public void Link_WhenPropertyStartTransformAssigned_UpdatesConnectionImmediately()
        {
            m_Link.startTransform = m_ExtraNavMesh.transform;
            m_PathfindingStart.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertyEndTransformAssigned_UpdatesConnectionImmediately()
        {
            m_Link.endTransform = m_ExtraNavMesh.transform;
            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertyStartTransformRemoved_UpdatesConnectionImmediately()
        {
            m_TempGO = Object.Instantiate(m_ExtraNavMesh);
            m_Link.startTransform = m_TempGO.transform;
            m_Link.UpdateLink();

            m_PathfindingStart.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);

            m_Link.startTransform = null;

            m_PathfindingStart.transform.position = m_Link.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertyEndTransformRemoved_UpdatesConnectionImmediately()
        {
            m_TempGO = Object.Instantiate(m_ExtraNavMesh);
            m_Link.endTransform = m_TempGO.transform;
            m_Link.startTransform = null;
            m_Link.startPoint = m_PlaneOnTheSide.transform.position - m_Link.transform.position;
            m_Link.UpdateLink();

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            m_PathfindingStart.transform.position = m_PlaneOnTheSide.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);

            m_Link.endTransform = null;

            m_PathfindingEnd.transform.position = m_Link.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertiesTransformAndPointAtStartChange_AppliesStartPointRelativeToStartTransform()
        {
            m_Link.startTransform = m_FarFromNavMesh.transform;
            m_Link.startPoint = 2f * m_OffsetX;

            m_PathfindingStart.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_WhenPropertiesTransformAndPointAtEndChange_AppliesStartPointRelativeToStartTransform()
        {
            m_Link.endTransform = m_FarFromNavMesh.transform;
            m_Link.endPoint = 2f * m_OffsetX;

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator Link_WhenAutoUpdateSwitchedOn_UpdatesOnNextFrame_AndNotBefore()
        {
            Assume.That(m_Link.autoUpdate, Is.False);

            m_Link.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;

            m_Link.autoUpdate = true;

            VerifyLinkConnection(ResultsIn.NoPath);

            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator Link_WhenAutoUpdateSwitchedOff_DoesNotApplyQueuedOrFutureChanges()
        {
            m_Link.autoUpdate = true;
            yield return null;

            m_Link.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            Assume.That(m_PathfindingEnd.transform.position, Is.EqualTo(m_PlaneOnTheSide.transform.position).Using(Vector3EqualityComparer.Instance));
            VerifyLinkConnection(ResultsIn.PathForward);

            m_Link.autoUpdate = false;
            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);

            m_Link.transform.SetPositionAndRotation(m_ExtraNavMesh.transform.position, Quaternion.identity);

            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [Test]
        public void Link_OnUpdateLink_AppliesChangesImmediately()
        {
            m_Link.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            m_Link.UpdateLink();

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward, m_Link.area, m_Link.agentTypeID);
        }

        [Test]
        public void Link_WhenEnabled_AppliesChangesImmediately()
        {
            m_Link.enabled = false;
            VerifyLinkConnection(ResultsIn.NoPath);

            AddNavMeshForPanda();
            ReconfigureLinkForPanda(m_Link);

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            VerifyLinkConnection(ResultsIn.NoPath, m_Link.area, m_Link.agentTypeID);

            m_Link.enabled = true;
            VerifyLinkConnection(ResultsIn.PathOnlyForward, m_Link.area, m_Link.agentTypeID);
        }

        void ReconfigureLinkForPanda(NavMeshLink link)
        {
            var wasEnabled = link.enabled;
            link.enabled = false;
            link.agentTypeID = m_PandaTypeID;
            link.area = k_AreaTypeForPanda;
            link.bidirectional = false;
            link.costModifier = 3f;
            link.width = 6f;
            link.startTransform = m_ExtraNavMesh.transform;
            link.endTransform = m_ExtraNavMesh.transform;
            link.startPoint = -m_OffsetZ + 3f * Vector3.right;
            link.endPoint = Vector3.zero + 3f * Vector3.right;
            link.enabled = wasEnabled;
        }

        void AddNavMeshForPanda()
        {
            m_NavMeshForPanda = NavMesh.AddNavMeshData(m_SurfaceForPanda.navMeshData,
                m_ExtraNavMesh.transform.position + 0.05f * Vector3.up, Quaternion.Euler(0f, 90f, 0f));
        }
#endif
        [Test]
        public void Link_WhenGameObjectTransformMoves_EndpointsMoveRelativeToLinkOnUpdate()
        {
            m_Link.transform.position += Vector3.forward;
            Assert.IsFalse(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneAtOrigin.transform.position + Vector3.forward));
            Assert.IsFalse(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneOnTheSide.transform.position + Vector3.forward));

            m_Link.UpdateLink();

            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneAtOrigin.transform.position + Vector3.forward));
            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneOnTheSide.transform.position + Vector3.forward));
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOn_WhenGameObjectMoves_UpdatesConnectionNextFrame()
        {
            m_Link.autoUpdate = true;
            m_Link.transform.position += Vector3.forward;

            Assert.IsFalse(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneAtOrigin.transform.position + Vector3.forward));
            Assert.IsFalse(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneOnTheSide.transform.position + Vector3.forward));

            yield return null;

            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneAtOrigin.transform.position + Vector3.forward));
            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneOnTheSide.transform.position + Vector3.forward));
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOff_WhenGameObjectMoves_KeepsConnectionUnchanged()
        {
            m_Link.autoUpdate = false;

            Assume.That(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneAtOrigin.transform.position), Is.True);
            Assert.That(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneOnTheSide.transform.position), Is.True);

            m_Link.transform.position += Vector3.forward;
            // Skip a few frames
            yield return null;
            yield return null;
            yield return null;

            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneAtOrigin.transform.position));
            Assert.IsTrue(HasPathConnectingViaPoint(m_PlaneAtOrigin, m_PlaneOnTheSide, m_PlaneOnTheSide.transform.position));
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOn_WhenGameObjectRotates_UpdatesConnectionNextFrame()
        {
            m_Link.autoUpdate = true;
            m_Link.transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;

            VerifyLinkConnection(ResultsIn.NoPath);

            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOff_WhenGameObjectRotates_KeepsConnectionUnchanged()
        {
            m_Link.autoUpdate = false;

            m_Link.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
            // Skip a few frames
            yield return null;
            yield return null;
            yield return null;

            Assume.That(m_PathfindingEnd.transform.position, Is.EqualTo(m_PlaneOnTheSide.transform.position).Using(Vector3EqualityComparer.Instance));
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOn_WhenTransformAtStartMoves_UpdatesConnectionNextFrame()
        {
            m_TempGO = Object.Instantiate(m_FarFromNavMesh);
            m_Link.startTransform = m_TempGO.transform;
            m_Link.UpdateLink();
            m_Link.autoUpdate = true;

            m_TempGO.transform.position += 2f * m_OffsetX;
            m_PathfindingStart.transform.position = m_TempGO.transform.position;

            VerifyLinkConnection(ResultsIn.NoPath);

            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOn_WhenTransformAtEndMoves_UpdatesConnectionNextFrame()
        {
            m_TempGO = Object.Instantiate(m_FarFromNavMesh);
            m_Link.endTransform = m_TempGO.transform;
            m_Link.UpdateLink();
            m_Link.autoUpdate = true;

            m_TempGO.transform.position += 2f * m_OffsetX;
            m_PathfindingEnd.transform.position = m_TempGO.transform.position;

            VerifyLinkConnection(ResultsIn.NoPath);

            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOn_WhenTransformAtStartDestroyed_UpdatesConnectionNextFrame()
        {
            m_Link.autoUpdate = true;

            m_TempGO = Object.Instantiate(m_ExtraNavMesh);
            m_Link.startTransform = m_TempGO.transform;
            m_Link.UpdateLink();

            m_PathfindingStart.transform.position = m_TempGO.transform.position;

            Object.DestroyImmediate(m_TempGO);

            VerifyLinkConnection(ResultsIn.PathForward);

            yield return null;

            VerifyLinkConnection(ResultsIn.NoPath);

            m_PathfindingStart.transform.position = m_Link.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOn_WhenTransformAtEndDestroyed_UpdatesConnectionNextFrame()
        {
            m_Link.autoUpdate = true;

            m_TempGO = Object.Instantiate(m_ExtraNavMesh);
            m_Link.endTransform = m_TempGO.transform;
            m_Link.startTransform = null;
            m_Link.startPoint = m_PlaneOnTheSide.transform.position - m_Link.transform.position;
            m_Link.UpdateLink();

            m_PathfindingEnd.transform.position = m_ExtraNavMesh.transform.position;
            m_PathfindingStart.transform.position = m_PlaneOnTheSide.transform.position;

            Object.DestroyImmediate(m_TempGO);

            VerifyLinkConnection(ResultsIn.PathForward);

            yield return null;

            VerifyLinkConnection(ResultsIn.NoPath);

            m_PathfindingEnd.transform.position = m_Link.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOff_WhenTransformAtStartMoves_KeepsConnectionUnchanged()
        {
            m_Link.autoUpdate = false;

            m_TempGO = Object.Instantiate(m_ExtraNavMesh);
            m_Link.startTransform = m_TempGO.transform;
            m_Link.UpdateLink();

            m_PathfindingStart.transform.position = m_TempGO.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);

            m_TempGO.transform.position += m_OffsetX;
            yield return null;
            yield return null;
            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);

            m_PathfindingStart.transform.position = m_TempGO.transform.position;
            VerifyLinkConnection(ResultsIn.NoPath);
        }

        [UnityTest]
        public IEnumerator LinkWithAutoUpdateOff_WhenTransformAtEndMoves_KeepsConnectionUnchanged()
        {
            m_Link.autoUpdate = false;

            m_TempGO = Object.Instantiate(m_ExtraNavMesh);
            m_Link.endTransform = m_TempGO.transform;
            m_Link.UpdateLink();

            m_PathfindingEnd.transform.position = m_TempGO.transform.position;
            VerifyLinkConnection(ResultsIn.PathForward);

            m_TempGO.transform.position += m_OffsetX;
            yield return null;
            yield return null;
            yield return null;

            VerifyLinkConnection(ResultsIn.PathForward);

            m_PathfindingEnd.transform.position = m_TempGO.transform.position;
            VerifyLinkConnection(ResultsIn.NoPath);
        }

        internal enum ResultsIn
        {
            PathForward,
            PathOnlyForward,
            PathBothWays,
            NoPath
        }

        void VerifyLinkConnection(ResultsIn expected,
            int areaType = 0,
            int agentTypeID = 0)
        {
            var forwardWanted = expected != ResultsIn.NoPath;
            try
            {
                Assert.That(HasPathConnecting(m_PathfindingStart, m_PathfindingEnd, 1 << areaType, agentTypeID), Is.EqualTo(forwardWanted),
                    forwardWanted ? "The NavMesh patches should be connected." : "The NavMesh patches should not be connected.");
            }
            catch (AssertionException)
            {
                Debug.Log($"The NavMesh patches at {m_PathfindingStart.transform.position} and {m_PathfindingEnd.transform.position} should {(forwardWanted ? "" : "not ")}be connected (agent={NavMesh.GetSettingsNameFromID(agentTypeID)}, area={areaType}).");

                throw;
            }

            if (expected != ResultsIn.PathForward && expected != ResultsIn.NoPath)
            {
                var backwardWanted = expected == ResultsIn.PathBothWays;
                try
                {
                    Assert.That(HasPathConnecting(m_PathfindingEnd, m_PathfindingStart, 1 << areaType, agentTypeID), Is.EqualTo(backwardWanted),
                        backwardWanted ? "The NavMesh patches should be connected backward." : "The NavMesh patches should not be connected backward.");
                }
                catch (AssertionException)
                {
                    Debug.Log($"The NavMesh patches at {m_PathfindingStart.transform.position} and {m_PathfindingEnd.transform.position} should {(backwardWanted ? "" : "not ")}be connected backward (agent={NavMesh.GetSettingsNameFromID(agentTypeID)}, area={areaType}).");

                    throw;
                }
            }
        }

        static bool HasPathConnecting(GameObject a, GameObject b, int areaMask = NavMesh.AllAreas, int agentTypeID = 0)
        {
            var path = new NavMeshPath();
            var filter = new NavMeshQueryFilter
            {
                areaMask = areaMask,
                agentTypeID = agentTypeID
            };
            NavMesh.CalculatePath(a.transform.position, b.transform.position, filter, path);
            return path.status == NavMeshPathStatus.PathComplete;
        }

        static bool HasPathConnectingViaPoint(GameObject a, GameObject b, Vector3 point, int areaMask = NavMesh.AllAreas, int agentTypeID = 0)
        {
            var path = new NavMeshPath();
            var filter = new NavMeshQueryFilter
            {
                areaMask = areaMask,
                agentTypeID = agentTypeID
            };
            NavMesh.CalculatePath(a.transform.position, b.transform.position, filter, path);
            if (path.status != NavMeshPathStatus.PathComplete)
                return false;

            var pathCorners = path.corners;
            for (var i = 1; i < pathCorners.Length - 1; i++)
            {
                var corner = pathCorners[i];
                if (Vector3.Distance(corner, point) < 0.1f)
                    return true;
            }

            return false;
        }
    }
}
#endif
