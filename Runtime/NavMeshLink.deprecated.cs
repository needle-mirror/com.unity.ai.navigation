using System;

#pragma warning disable IDE1006 // Naming Styles
#if ENABLE_NAVIGATION_OFFMESHLINK_TO_NAVMESHLINK
namespace Unity.AI.Navigation
{
    public partial class NavMeshLink
    {
        [Obsolete("autoUpdatePositions has been deprecated. Use autoUpdate instead. (UnityUpgradable) -> autoUpdate")]
        public bool autoUpdatePositions { get; set; }

        [Obsolete("biDirectional has been deprecated. Use bidirectional instead. (UnityUpgradable) -> bidirectional")]
        public bool biDirectional { get; set; }

        [Obsolete("costOverride has been deprecated. Use costModifier instead. (UnityUpgradable) -> costModifier")]
        public float costOverride { get; set; }

        [Obsolete("UpdatePositions() has been deprecated. Use UpdateLink() instead. (UnityUpgradable) -> UpdateLink(*)")]
        public void UpdatePositions() { }
    }
}
#endif
#pragma warning restore IDE1006 // Naming Styles
