#if ENABLE_NAVIGATION_PACKAGE_RELEASE_FEATURES
using Unity.AI.Navigation.Editor.Converter;

namespace Unity.AI.Navigation.Updater
{
    internal sealed class BuiltInToNavMeshSurfaceConverterContainer : SystemConverterContainer
    {
        public override string name => "NavMesh updater";
        public override string info => "The NavMesh updater performs the following tasks:\n* Converts scene baked with the Built-in NavMesh to the component-based version.\n* Replace game object using NavMesh static values with NavMeshModifiers.";
    }
}
#endif
