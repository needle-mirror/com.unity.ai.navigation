using Unity.AI.Navigation.Editor.Converter;

namespace Unity.AI.Navigation.Updater
{
    internal sealed class NavigationConverterContainer : SystemConverterContainer
    {
        public override string name => "Navigation Updater";

        public override string info =>
            "* Converts scenes baked with the built-in NavMesh to the component-based version.\n" +
            "* Replaces Navigation Static flags with NavMeshModifier components.\n" +
            "* Turns OffMesh Link components into NavMesh Link components and preserves their properties.";
    }
}
