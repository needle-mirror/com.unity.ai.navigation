namespace Unity.AI.Navigation.Editor.Converter
{
    /// <summary>
    /// A structure needed for the conversion part of the converter. <br />
    /// This holds the items that are being converted. <br />
    /// All arrays are the same length and the index of each array corresponds to the same item.
    /// </summary>
    internal struct RunItemContext
    {
        /// <summary> The items that will go through the conversion code. </summary>
        public ConverterItemInfo[] items { get; }

        /// <summary> A bool to set if these items failed to convert. </summary>
        public bool[] didFail { get; }

        /// <summary> Info to store data to be shown in the UI. </summary>
        public string[] info { get; }

        /// <summary> Constructor for the RunItemContext. </summary>
        public RunItemContext(ConverterItemInfo[] items)
        {
            this.items = items;
            didFail = new bool[this.items.Length];
            info = new string[this.items.Length];
        }
    }
}
