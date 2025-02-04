using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEditor.Search;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine.Assertions;

namespace Unity.AI.Navigation.Editor.Converter
{
    // Status for each row item to say in which state they are in.
    // This will make sure they are showing the correct icon
    [Serializable]
    enum Status
    {
        Pending,
        Warning,
        Error,
        Success
    }

    // This is the serialized class that stores the state of each item in the list of items to convert
    [Serializable]
    class ConverterItemState
    {
        public bool isActive;

        // Message that will be displayed on the icon if warning or failed.
        public string message;

        // Status of the converted item, Pending, Warning, Error or Success
        public Status status;

        internal bool hasConverted = false;
    }

    // Each converter uses the active bool
    // Each converter has a list of active items/assets
    // We do this so that we can use the binding system of the UI Elements
    [Serializable]
    class ConverterState
    {
        // This is the enabled state of the whole converter
        public bool isEnabled;
        public bool isActive;
        public bool isLoading; // to name
        public bool isInitialized;
        public List<ConverterItemState> items = new List<ConverterItemState>();

        public int pending;
        public int warnings;
        public int errors;
        public int success;
        internal int index;

        public bool isActiveAndEnabled => isEnabled && isActive;
        public bool requiresInitialization => !isInitialized && isActiveAndEnabled;
    }

    [Serializable]
    internal struct ConverterItems
    {
        public List<ConverterItemDescriptor> itemDescriptors;
    }

    [Serializable]
    internal class SystemConvertersEditor : EditorWindow
    {
        public VisualTreeAsset converterEditorAsset;
        public VisualTreeAsset converterListAsset;
        public VisualTreeAsset converterItem;

        ScrollView m_ScrollView;

        List<SystemConverter> m_CoreConvertersList = new List<SystemConverter>();

        private bool convertButtonActive = false;

        // This list needs to be as long as the amount of converters
        List<ConverterItems> m_ItemsToConvert = new List<ConverterItems>();
        SerializedObject m_SerializedObject;

        List<string> m_ContainerChoices = new List<string>();
        List<SystemConverterContainer> m_Containers = new List<SystemConverterContainer>();
        int m_ContainerChoiceIndex = 0;

        // This is a list of Converter States which holds a list of which converter items/assets are active
        // There is one for each Converter.
        [SerializeField] List<ConverterState> m_ConverterStates = new List<ConverterState>();

        TypeCache.TypeCollection m_ConverterContainers;

        // Name of the index file
        string m_ConverterIndex = "SystemConverterIndex";

        public void DontSaveToLayout(EditorWindow wnd)
        {
            // Making sure that the window is not saved in layouts.
            Assembly assembly = typeof(EditorWindow).Assembly;
            var editorWindowType = typeof(EditorWindow);
            var hostViewType = assembly.GetType("UnityEditor.HostView");
            var containerWindowType = assembly.GetType("UnityEditor.ContainerWindow");
            var parentViewField = editorWindowType.GetField("m_Parent", BindingFlags.Instance | BindingFlags.NonPublic);
            var parentViewValue = parentViewField.GetValue(wnd);
            // window should not be saved to layout
            var containerWindowProperty =
                hostViewType.GetProperty("window", BindingFlags.Instance | BindingFlags.Public);
            var parentContainerWindowValue = containerWindowProperty.GetValue(parentViewValue);
            var dontSaveToLayoutField =
                containerWindowType.GetField("m_DontSaveToLayout", BindingFlags.Instance | BindingFlags.NonPublic);
            dontSaveToLayoutField.SetValue(parentContainerWindowValue, true);
        }

        void OnEnable()
        {
            InitIfNeeded();
        }

        void InitIfNeeded()
        {
            if (m_CoreConvertersList.Any())
                return;
            m_CoreConvertersList = new List<SystemConverter>();

            // This is the drop down choices.
            m_ConverterContainers = TypeCache.GetTypesDerivedFrom<SystemConverterContainer>();
            foreach (var containerType in m_ConverterContainers)
            {
                var container = (SystemConverterContainer)Activator.CreateInstance(containerType);
                m_Containers.Add(container);
                m_ContainerChoices.Add(container.name);
            }

            if (m_ConverterContainers.Any())
            {
                GetConverters();
            }
            else
            {
                ClearConverterStates();
            }
        }

        void ClearConverterStates()
        {
            m_CoreConvertersList.Clear();
            m_ConverterStates.Clear();
            m_ItemsToConvert.Clear();
        }

        void GetConverters()
        {
            ClearConverterStates();
            var converterList = TypeCache.GetTypesDerivedFrom<SystemConverter>();

            for (var i = 0; i < converterList.Count; ++i)
            {
                // Iterate over the converters that are used by the current container
                var conv = (SystemConverter)Activator.CreateInstance(converterList[i]);
                if (conv.container == m_ConverterContainers[m_ContainerChoiceIndex])
                {
                    m_CoreConvertersList.Add(conv);
                }
            }

            // this need to be sorted by Priority property
            m_CoreConvertersList = m_CoreConvertersList
                .OrderBy(o => o.priority).ToList();

            for (var i = 0; i < m_CoreConvertersList.Count; i++)
            {
                // Create a new ConvertState which holds the active state of the converter
                var converterState = new ConverterState
                {
                    isEnabled = m_CoreConvertersList[i].isEnabled,
                    isActive = true,
                    isInitialized = false,
                    items = new List<ConverterItemState>(),
                    index = i,
                };
                m_ConverterStates.Add(converterState);

                // This just creates empty entries in the m_ItemsToConvert.
                // This list need to have the same amount of entries as the converters
                var converterItemInfos = new List<ConverterItemDescriptor>();
                //m_ItemsToConvert.Add(converterItemInfos);
                m_ItemsToConvert.Add(new ConverterItems { itemDescriptors = converterItemInfos });
            }
        }

        public void CreateGUI()
        {
            InitIfNeeded();
            if (m_ConverterContainers.Any())
            {
                m_SerializedObject = new SerializedObject(this);
                converterEditorAsset.CloneTree(rootVisualElement);

                RecreateUI();

                var button = rootVisualElement.Q<Button>("convertButton");
                button.RegisterCallback<ClickEvent>(Convert);
                button.SetEnabled(false);

                var initButton = rootVisualElement.Q<Button>("initializeButton");
                initButton.RegisterCallback<ClickEvent>(InitializeAllActiveConverters);
            }
        }

        void RecreateUI()
        {
            m_SerializedObject.Update();

            var currentContainer = m_Containers[m_ContainerChoiceIndex];
            rootVisualElement.Q<Label>("conversionName").text = currentContainer.name;
            rootVisualElement.Q<TextElement>("conversionInfo").text = currentContainer.info;

            rootVisualElement.Q<Image>("converterContainerHelpIcon").image = EditorStyles.iconHelp;

            // Getting the scrollview where the converters should be added
            m_ScrollView = rootVisualElement.Q<ScrollView>("convertersScrollView");
            m_ScrollView.Clear();
            for (int i = 0; i < m_CoreConvertersList.Count; ++i)
            {
                // Making an item using the converterListAsset as a template.
                // Then adding the information needed for each converter
                VisualElement item = new VisualElement();
                converterListAsset.CloneTree(item);
                var conv = m_CoreConvertersList[i];
                item.SetEnabled(conv.isEnabled);
                item.Q<Label>("converterName").text = conv.name;
                item.Q<Label>("converterInfo").text = conv.info;

                // setup the images
                item.Q<Image>("pendingImage").image = EditorStyles.iconPending;
                item.Q<Image>("pendingImage").tooltip = "Pending";
                var pendingLabel = item.Q<Label>("pendingLabel");
                item.Q<Image>("warningImage").image = EditorStyles.iconWarn;
                item.Q<Image>("warningImage").tooltip = "Warnings";
                var warningLabel = item.Q<Label>("warningLabel");
                item.Q<Image>("errorImage").image = EditorStyles.iconFail;
                item.Q<Image>("errorImage").tooltip = "Failed";
                var errorLabel = item.Q<Label>("errorLabel");
                item.Q<Image>("successImage").image = EditorStyles.iconSuccess;
                item.Q<Image>("successImage").tooltip = "Success";
                var successLabel = item.Q<Label>("successLabel");

                var converterEnabledToggle = item.Q<Toggle>("converterEnabled");
                converterEnabledToggle.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.isActive)}";
                pendingLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.pending)}";
                warningLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.warnings)}";
                errorLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.errors)}";
                successLabel.bindingPath =
                    $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.success)}";

                VisualElement child = item;
                ListView listView = child.Q<ListView>("converterItems");

                listView.showBoundCollectionSize = false;
                listView.bindingPath = $"{nameof(m_ConverterStates)}.Array.data[{i}].{nameof(ConverterState.items)}";

                int id = i;
                listView.makeItem = () =>
                {
                    var convertItem = converterItem.CloneTree();
                    // Adding the contextual menu for each item
                    convertItem.AddManipulator(new ContextualMenuManipulator(evt => AddToContextMenu(evt, id)));
                    return convertItem;
                };

                listView.bindItem = (element, index) =>
                {
                    m_SerializedObject.Update();
                    var property = m_SerializedObject.FindProperty($"{listView.bindingPath}.Array.data[{index}]");

                    // ListView doesn't bind the child elements for us properly, so we do that for it
                    // In the UXML our root is a BindableElement, as we can't bind otherwise.
                    var bindable = (BindableElement)element;
                    bindable.BindProperty(property);

                    // Adding index here to userData so it can be retrieved later
                    element.userData = index;

                    var status = (Status)property.FindPropertyRelative("status").enumValueIndex;
                    var info = property.FindPropertyRelative("message").stringValue;

                    // Update the amount of things to convert
                    child.Q<Label>("converterStats").text = $"{m_ItemsToConvert[id].itemDescriptors.Count} items";

                    var convItemDesc = m_ItemsToConvert[id].itemDescriptors[index];

                    element.Q<Label>("converterItemName").text = convItemDesc.name;
                    element.Q<Label>("converterItemPath").text = convItemDesc.info;

                    // Changing the icon here depending on the status.
                    Texture2D icon = null;

                    switch (status)
                    {
                        case Status.Pending:
                            icon = EditorStyles.iconPending;
                            break;
                        case Status.Error:
                            icon = EditorStyles.iconFail;
                            break;
                        case Status.Warning:
                            icon = EditorStyles.iconWarn;
                            break;
                        case Status.Success:
                            icon = EditorStyles.iconSuccess;
                            break;
                    }

                    element.Q<Image>("converterItemStatusIcon").image = icon;
                    element.Q<Image>("converterItemStatusIcon").tooltip = info;
                };
                listView.selectionChanged += obj => { m_CoreConvertersList[id].OnClicked(listView.selectedIndex); };
                listView.unbindItem = (element, index) =>
                {
                    var bindable = (BindableElement)element;
                    bindable.Unbind();
                };

                m_ScrollView.Add(item);
            }
            rootVisualElement.Bind(m_SerializedObject);
            var button = rootVisualElement.Q<Button>("convertButton");
            button.RegisterCallback<ClickEvent>(Convert);
            button.SetEnabled(convertButtonActive);

            var initButton = rootVisualElement.Q<Button>("initializeButton");
            initButton.RegisterCallback<ClickEvent>(InitializeAllActiveConverters);
        }

        void GetAndSetData(int i, Action onAllConvertersCompleted = null)
        {
            // This need to be in Init method
            // Need to get the assets that this converter is converting.
            // Need to return Name, Path, Initial info, Help link.
            // New empty list of ConverterItemInfos
            List<ConverterItemDescriptor> converterItemInfos = new List<ConverterItemDescriptor>();
            var initCtx = new InitializeConverterContext { items = converterItemInfos };

            var conv = m_CoreConvertersList[i];

            m_ConverterStates[i].isLoading = true;

            // This should also go to the init method
            // This will fill out the converter item infos list
            int id = i;
            conv.OnInitialize(initCtx, OnConverterCompleteDataCollection);

            void OnConverterCompleteDataCollection()
            {
                // Set the item infos list to to the right index
                m_ItemsToConvert[id] = new ConverterItems { itemDescriptors = converterItemInfos };
                m_ConverterStates[id].items = new List<ConverterItemState>(converterItemInfos.Count);

                // Default all the entries to true
                for (var j = 0; j < converterItemInfos.Count; j++)
                {
                    string message = string.Empty;
                    Status status;
                    bool active = true;
                    // If this data hasn't been filled in from the init phase then we can assume that there are no issues / warnings
                    if (string.IsNullOrEmpty(converterItemInfos[j].warningMessage))
                    {
                        status = Status.Pending;
                    }
                    else
                    {
                        status = Status.Warning;
                        message = converterItemInfos[j].warningMessage;
                        active = false;
                        m_ConverterStates[id].warnings++;
                    }

                    m_ConverterStates[id].items.Add(new ConverterItemState
                    {
                        isActive = active,
                        message = message,
                        status = status,
                        hasConverted = false,
                    });
                }

                m_ConverterStates[id].isLoading = false;
                m_ConverterStates[id].isInitialized = true;

                // Making sure that the pending amount is set to the amount of items needs converting
                m_ConverterStates[id].pending = m_ConverterStates[id].items.Count;

                EditorUtility.SetDirty(this);
                m_SerializedObject.ApplyModifiedProperties();

                CheckAllConvertersCompleted();
                convertButtonActive = true;
                // Make sure that the Convert Button is turned back on
                var button = rootVisualElement.Q<Button>("convertButton");
                button.SetEnabled(convertButtonActive);
            }

            void CheckAllConvertersCompleted()
            {
                int convertersToInitialize = 0;
                int convertersInitialized = 0;

                for (var j = 0; j < m_ConverterStates.Count; j++)
                {
                    var converter = m_ConverterStates[j];

                    // Skip inactive converters
                    if (!converter.isActiveAndEnabled)
                        continue;

                    if (converter.isInitialized)
                        convertersInitialized++;
                    else
                        convertersToInitialize++;
                }

                var sum = convertersToInitialize + convertersInitialized;

                Assert.IsFalse(sum == 0);

                // Show our progress so far
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayProgressBar($"Initializing converters", $"Initializing converters ({convertersInitialized}/{sum})...", (float)convertersInitialized / sum);

                // If all converters are initialized call the complete callback
                if (convertersToInitialize == 0)
                {
                    onAllConvertersCompleted?.Invoke();
                }
            }
        }

        void InitializeAllActiveConverters(ClickEvent evt)
        {
            // If we use search index, go async
            if (ShouldCreateSearchIndex())
            {
                CreateSearchIndex(m_ConverterIndex);
            }
            // Otherwise do everything directly
            else
            {
                ConverterCollectData(() => { EditorUtility.ClearProgressBar(); });
            }

            void CreateSearchIndex(string name)
            {
                // Create <guid>.index in the project
                var title = $"Building {name} search index";
                EditorUtility.DisplayProgressBar(title, "Creating search index...", -1f);

                // Private implementation of a file naming function which puts the file at the selected path.
                Type assetdatabase = typeof(AssetDatabase);
                var indexPath = (string)assetdatabase.GetMethod("GetUniquePathNameAtSelectedPath", BindingFlags.NonPublic | BindingFlags.Static).Invoke(assetdatabase, new object[] { $"Assets/{name}.index" });

                // Write search index manifest
                System.IO.File.WriteAllText(indexPath,
@"{
                ""roots"": [""Assets""],
                ""includes"": [],
                ""excludes"": [],
                ""options"": {
                    ""types"": true,
                    ""properties"": true,
                    ""extended"": true,
                    ""dependencies"": true
                    },
                ""baseScore"": 9999
                }");



                // Import the search index
                AssetDatabase.ImportAsset(indexPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.DontDownloadFromCacheServer);

                EditorApplication.delayCall += () =>
                {
                    // Create dummy request to ensure indexing has finished
                    var context = SearchService.CreateContext("asset", $"p: a=\"{name}\"");
                    SearchService.Request(context, (_, items) =>
                    {
                        OnSearchIndexCreated(name, indexPath, () =>
                        {
                            DeleteSearchIndex(context, indexPath);
                        });
                    });
                };
            }

            void OnSearchIndexCreated(string name, string path, Action onComplete)
            {
                EditorUtility.ClearProgressBar();

                ConverterCollectData(onComplete);
            }

            void ConverterCollectData(Action onConverterDataCollectionComplete)
            {
                EditorUtility.DisplayProgressBar($"Initializing converters", $"Initializing converters...", -1f);

                var convertersToInitialize = 0;
                for (var i = 0; i < m_ConverterStates.Count; ++i)
                {
                    if (m_ConverterStates[i].isEnabled)
                    {
                        GetAndSetData(i, onConverterDataCollectionComplete);
                        convertersToInitialize++;
                    }
                }

                // If we don't have any converters to initialize, call the complete callback directly.
                if (convertersToInitialize == 0)
                    onConverterDataCollectionComplete?.Invoke();
            }

            void DeleteSearchIndex(SearchContext context, string indexPath)
            {
                context?.Dispose();
                // Client code has finished with the created index. We can delete it.
                AssetDatabase.DeleteAsset(indexPath);
                EditorUtility.ClearProgressBar();
            }
        }

        bool ShouldCreateSearchIndex()
        {
            for (int i = 0; i < m_ConverterStates.Count; ++i)
            {
                if (m_ConverterStates[i].requiresInitialization)
                {
                    var converter = m_CoreConvertersList[i];
                    if (converter.needsIndexing)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void AddToContextMenu(ContextualMenuPopulateEvent evt, int coreConverterIndex)
        {
            var ve = (VisualElement)evt.target;
            // Checking if this context menu should be enabled or not
            var isActive = m_ConverterStates[coreConverterIndex].items[(int)ve.userData].isActive &&
                !m_ConverterStates[coreConverterIndex].items[(int)ve.userData].hasConverted;

            evt.menu.AppendAction("Run converter for this asset",
                e => { ConvertIndex(coreConverterIndex, (int)ve.userData); },
                isActive ? DropdownMenuAction.AlwaysEnabled : DropdownMenuAction.AlwaysDisabled);
        }

        void Convert(ClickEvent evt)
        {
            var activeConverterStates = new List<ConverterState>();
            // Get the names of the converters
            // Get the amount of them
            // Make the string "name x/y"

            // Getting all the active converters to use in the cancelable progressbar
            foreach (var state in m_ConverterStates)
            {
                if (state.isActive && state.isInitialized)
                    activeConverterStates.Add(state);
            }

            var converterCount = 0;
            var activeConvertersCount = activeConverterStates.Count;
            foreach (var activeConverterState in activeConverterStates)
            {
                if (activeConverterState.items.Count == 0)
                    continue;
                var hasItemsToConvert = false;
                foreach (var item in activeConverterState.items)
                {
                    if (item.isActive && !item.hasConverted)
                    {
                        hasItemsToConvert = true;
                        break;
                    }
                }
                if (!hasItemsToConvert)
                    continue;

                var converterIndex = activeConverterState.index;
                m_CoreConvertersList[converterIndex].OnPreRun();

                var converterName = m_CoreConvertersList[converterIndex].name;
                var progressTitle = $"{converterName}           Converter : {converterCount++}/{activeConvertersCount}";
                BatchConvert(activeConverterState, progressTitle);

                m_CoreConvertersList[converterIndex].OnPostRun();
                AssetDatabase.SaveAssets();
                EditorUtility.ClearProgressBar();
            }
        }

        void BatchConvert(ConverterState converterState, string progressBarTile)
        {
            var converterIndex = converterState.index;
            var itemsFound = converterState.items;
            var itemsToConvert = new List<ConverterItemInfo>(itemsFound.Count);
            for (var i = 0; i < itemsFound.Count; ++i)
            {
                if (itemsFound[i].isActive && !itemsFound[i].hasConverted)
                    itemsToConvert.Add(new ConverterItemInfo()
                    {
                        index = i,
                        descriptor = m_ItemsToConvert[converterIndex].itemDescriptors[i]
                    });
            }

            // Since this is a batched process, we don't have progress to show, so we stick it to 50%.
            EditorUtility.DisplayProgressBar(progressBarTile, $"Processing {itemsToConvert.Count} items.", 0.5f);

            var ctx = new RunItemContext(itemsToConvert.ToArray());
            m_CoreConvertersList[converterIndex].OnRun(ref ctx);
            UpdateInfo(converterIndex, ctx);
        }

        void ConvertIndex(int converterIndex, int index)
        {
            if (!m_ConverterStates[converterIndex].items[index].hasConverted)
            {
                m_ConverterStates[converterIndex].items[index].hasConverted = true;
                var item = new ConverterItemInfo()
                {
                    index = index,
                    descriptor = m_ItemsToConvert[converterIndex].itemDescriptors[index],
                };
                var ctx = new RunItemContext(new[] { item });
                m_CoreConvertersList[converterIndex].OnRun(ref ctx);
                UpdateInfo(converterIndex, ctx);
            }
        }

        void UpdateInfo(int converterIndex, RunItemContext ctx)
        {
            // Reset converter stats, so that they don't contain old data from previous runs.
            m_ConverterStates[converterIndex].warnings = 0;
            m_ConverterStates[converterIndex].errors = 0;
            m_ConverterStates[converterIndex].success = 0;

            var items = ctx.items;
            for (var i = 0; i < items.Length; ++i)
            {
                var itemIndex = items[i].index;
                if (ctx.didFail[i])
                {
                    m_ConverterStates[converterIndex].items[itemIndex].message = ctx.info[i];
                    m_ConverterStates[converterIndex].items[itemIndex].status = Status.Error;
                    m_ConverterStates[converterIndex].errors++;
                }
                else
                {
                    m_ConverterStates[converterIndex].items[itemIndex].status = Status.Success;
                    m_ConverterStates[converterIndex].success++;
                    m_ConverterStates[converterIndex].items[itemIndex].hasConverted = true;
                }

                // If the item was converted, we deselect it in the conversion list.
                m_ConverterStates[converterIndex].items[itemIndex].isActive = false;
            }

            if (m_ConverterStates[converterIndex].pending > 0)
                m_ConverterStates[converterIndex].pending--;

            var child = m_ScrollView[converterIndex];
            child.Q<ListView>("converterItems").Rebuild();
        }
    }
}
