using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Sceelix.Core.Procedures;
using Sceelix.Processors;

namespace LazyProcedural
{
    public class GraphWindow : UnityEditor.Experimental.GraphView.GraphViewEditorWindow
    {

        private VisualElement _root;
        private Graph graph;
        private Vector2 addMousePos;
        private GenerationManager generationManager;
        private ProcessorManager processorManager;
        private GeoGraphComponent graphComponent;

        //-----Header Area
        private Button _showContextButton;
        private Button _runButton;
        private Button _testPlusButton;
        private Button _testMinusButton;
        private Slider _testSlider;

        //-----Main Area
        private VisualElement _graph;


        //-----Footer Area
        private TextField _focusTarget;

        //Debug Messager
        private TextElement _debugMssg;


        //Settings
        private PopupField<string> _settingdDrop;
        private VisualElement _settingsIcon;

        //About
        private PopupField<string> _aboutDrop;
        private VisualElement _aboutIcon;

        //Windows
        private ContextWindow _contextWindow;

        //Messages

        const string SETTINGS_RESETPREF_MSG = "Reset Preferences";
        const string SETTINGS_CLEARTEMP_MSG = "Delete Temp Items";
        const string SETTINGS_CLEARSTORED_MSG = "Delete Stored Items";


        const string ABOUT_TOOL_REPO_MSG = "About Lazy Procedural";
        const string ABOUT_REPORT_MSG = "Report an error";
        const string ABOUT_DONATE_MSG = "Donate and support the development";

        //const string SETTING_RESETPREF_TITLE = "Reset Preferences";
        //const string SETTING_RESETPREF_DESC = "Are you sure you want to reset this window's preferences";
        //const string SETTING_DELETETEMP_TITLE = "Delete Temp Items";
        //const string SETTING_DELETETEMP_DESC = "Are you sure you want to delete all the fetched temporary items?";
        //const string SETTING_DELETESTORED_TITLE = "Delete Stored Items";
        //const string SETTING_DELETESTORED_DESC = "Are you sure you want to delete all the fetched built/stored items?";

        const string CONFIRM_MSG = "Go Ahead";
        const string CANCEL_MSG = "Cancel";

        private void OnEnable()
        {

            InitVariables();
            InitGraph();

            SetupBaseUI();
            SetupExtraWindows();


            //Blackboard b= new Blackboard(graph);
            //graph.Add(b);

            //Blackboard b2 = new Blackboard(graph);
            //graph.Add(b2);

            //Blackboard b3 = new Blackboard(graph);
            //graph.Add(b3);

            SetupBindings();
            SetupPatchedDropdowns();
            SetupIcons();

            SetupCallbacks();
            SetupInputCallbacks();

            SetupFooterMenuItems();


            FocusOnGraph();

            LogMessage("Setup finished");

        }

        private void InitVariables()
        {
            PathFactory.Init();

            generationManager = new GenerationManager();

            processorManager = new ProcessorManager();

            ProcedureInfoManager.Init();

            WindowManager.Init();

            graphComponent = GraphComponentFinder.FindComponent();

        }
        private void InitGraph()
        {
            graph = new Graph();

            graph.OnNodeSelected += OnNodeSelected;

            graph.OnGraphChanged += OnGraphChanged;

        }



        public void AddNode(ProcedureInfo nodeInfo)
        {

            var pos = addMousePos;
            pos = _root.ChangeCoordinatesTo(_root.parent, pos - this.position.position);
            graph.AddNode(nodeInfo, pos);

            RunGraph();
        }


        private void OnNodeSelected(Node node)
        {
            if (graph.selection.Count > 1) return;

            OpenContextWindow();
            _contextWindow.BuildNodeParameters(node.nodeData);
        }

        private void OnGraphChanged()
        {
            RunGraph();
        }

        private void OpenCloseContextWindow()
        {
            if (_contextWindow != null)
                CloseContextWindow();
            else
                OpenContextWindow();
        }
        private void OpenContextWindow()
        {
            if (_contextWindow == null)
                _contextWindow = new ContextWindow(this);

            _contextWindow.Show();
        }
        private void CloseContextWindow()
        {
            if (_contextWindow == null) return;

            _contextWindow.Close();

            _contextWindow = null;
        }


        private void OnDestroy()
        {
            CloseContextWindow();
        }





        #region Base UI

        private void SetupBaseUI()
        {
            _root = rootVisualElement;

            // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
            var visualTree = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath(
                PathFactory.BuildUiFilePath(PathFactory.GRAPH_WINDOW_LAYOUT_FILE), typeof(VisualTreeAsset));

            visualTree.CloneTree(_root);

            graph.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            graph.style.height = new StyleLength(new Length(100, LengthUnit.Percent));

            _graph = _root.Q("Graph");
            _graph.Add(graph);

            //graphvi

            //this.


            //var b = new UnityEditor.Experimental.GraphView.GraphViewBlackboardWindow();
            //b.SelectGraphViewFromWindow(this, graph);
            //b.Show();
            //b.SelectGraphViewFromWindow()
            //b.SelectGraphViewFromWindow(graph);
            //b.visible = true;
            //b.style.height = 220f;
            //b.style.width = 100f;
            //graph.ReleaseBlackboard(b);
            //var blackB = graph.GetBlackboard();
            //blackB.showInMiniMap = true;


        }

        private void SetupExtraWindows()
        {
            //_contextWindow = new ContextWindow();

        }



        private void SetupBindings()
        {

            //-----Header Area
            _testPlusButton = (Button)_root.Q("Add");
            _testMinusButton = (Button)_root.Q("Remove");
            _testSlider = (Slider)_root.Q("Value");
            _showContextButton = (Button)_root.Q("ShowContextBttn");
            _runButton = (Button)_root.Q("RunBttn");

            //-----Main Area




            //-----Footer Area

            _focusTarget = (TextField)_root.Q("FocusTarget");

            //Debug Messager
            _debugMssg = (TextElement)_root.Q("Debug");

            //Settings
            _settingsIcon = _root.Q("SettingsIcon");

            //About
            _aboutIcon = _root.Q("AboutIcon");

        }




        private void SetupPatchedDropdowns()
        {

            ////Settings
            _settingdDrop = Utils.CreateDropdownField(_root.Q("SettingsDrop"));

            ////About
            _aboutDrop = Utils.CreateDropdownField(_root.Q("AboutDrop"));
        }


        private void SetupIcons()
        {
            //Acces icons list  here -> https://github.com/halak/unity-editor-icons

            //_searchBttnIcon.style.backgroundImage = (Texture2D)EditorGUIUtility.IconContent("d_Search Icon").image;

            _settingsIcon.style.backgroundImage = (Texture2D)EditorGUIUtility.IconContent("d__Popup@2x").image;
            _aboutIcon.style.backgroundImage = (Texture2D)EditorGUIUtility.IconContent("d_console.infoicon.sml").image;

        }


        private void SetupCallbacks()
        {

            _showContextButton.clicked += OpenCloseContextWindow;
            _runButton.clicked += RunGraph;

            //Footer Menu Items
            _aboutDrop.RegisterValueChangedCallback(x => OnAboutMenuChanged(x.newValue));
            _settingdDrop.RegisterValueChangedCallback(x => OnSettingsMenuChanged(x.newValue));
        }


        private void SetupInputCallbacks()
        {
            _root.RegisterCallback<KeyDownEvent>(OnKeyboardKeyDown, TrickleDown.TrickleDown);
            _root.RegisterCallback<MouseDownEvent>(OnMouseKeyDown, TrickleDown.TrickleDown);
        }


        private void OnKeyboardKeyDown(KeyDownEvent e)
        {


            if (e.keyCode == KeyCode.Escape)
                this.Close();



            if (e.ctrlKey && e.keyCode == KeyCode.D)
            {
                DuplicateSelection();
            }

            else if (e.keyCode == KeyCode.Space)
            {
                OpenSearch();
                //_searchBar.Focus();
            }

            else if ((e.keyCode == KeyCode.Return))
            {
                RunGraph();
            }
        }

        private void OnMouseKeyDown(MouseDownEvent e)
        {

        }


        #endregion BaseUI


        private void OpenSearch()
        {

            var searchWindow = new SearchWindow(ProcedureInfoManager.ProceduresInfo, this);
            searchWindow.graphWindow = this;

            searchWindow.ShowPopup();

            addMousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);



            searchWindow.position = new Rect(addMousePos, searchWindow.position.size);
        }





        private void OnAboutMenuChanged(string option)
        {
            if (option == "") return;

            switch (option)
            {
                case ABOUT_TOOL_REPO_MSG:
                    Application.OpenURL("https://github.com/wafflesgama/LazyProcedural");
                    break;

                case ABOUT_REPORT_MSG:
                    Application.OpenURL("https://github.com/wafflesgama/LazyProcedural/issues");
                    break;

                case ABOUT_DONATE_MSG:
                    Application.OpenURL("https://www.buymeacoffee.com/guilhermeGama");
                    break;
            }

            _aboutDrop.value = "";


        }
        private void OnSettingsMenuChanged(string option)
        {
            switch (option)
            {
                //case SETTINGS_RESETPREF_MSG:
                //    if (EditorUtility.DisplayDialog(SETTING_RESETPREF_TITLE, SETTING_RESETPREF_DESC, CONFIRM_MSG, CANCEL_MSG))
                //        ResetPreferences();
                //    break;

                //case SETTINGS_CLEARTEMP_MSG:
                //    if (EditorUtility.DisplayDialog(SETTING_DELETETEMP_TITLE, SETTING_DELETETEMP_DESC, CONFIRM_MSG, CANCEL_MSG))
                //    {
                //        RemoveAllTempFiles();
                //        AssetDatabase.Refresh();
                //    }
                //    break;

                //case SETTINGS_CLEARSTORED_MSG:
                //    if (EditorUtility.DisplayDialog(SETTING_DELETESTORED_TITLE, SETTING_DELETESTORED_DESC, CONFIRM_MSG, CANCEL_MSG))
                //    {
                //        RemoveAllStoredItems();
                //        AssetDatabase.Refresh();
                //    }
                //    break;
            }

            _settingdDrop.value = "";
        }

        private void SetupFooterMenuItems()
        {

            //About
            List<string> footerAboutMenuItems = new List<string>();
            footerAboutMenuItems.Add(ABOUT_TOOL_REPO_MSG);
            footerAboutMenuItems.Add(ABOUT_REPORT_MSG);
            footerAboutMenuItems.Add(ABOUT_DONATE_MSG);
            _aboutDrop.choices = footerAboutMenuItems;


            //Settings
            List<string> footerSettingsMenuItems = new List<string>();
            footerSettingsMenuItems.Add(SETTINGS_RESETPREF_MSG);
            footerSettingsMenuItems.Add(SETTINGS_CLEARTEMP_MSG);
            footerSettingsMenuItems.Add(SETTINGS_CLEARSTORED_MSG);
            _settingdDrop.choices = footerSettingsMenuItems;


        }


        public void OnGraphValueUpdated()
        {
            RunGraph();
        }

        private void DuplicateSelection()
        {
            var mousePos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            mousePos = _root.ChangeCoordinatesTo(_root.parent, mousePos - this.position.position);

            graph.DuplicateSelection(mousePos);
        }

        private void RunGraph()
        {
            if (graphComponent == null)
                graphComponent = GraphComponentFinder.FindComponent();

            var meshes = generationManager.ExecuteGraph(graph.nodes.ToList());

            processorManager.Populate(meshes, graphComponent);
        }


        public void FocusOnGraph()
        {
            this.Focus();
            _focusTarget.Focus();
        }
        #region Utils

        private static bool CheckIfMouseOverSreen(Rect r, bool relativePos = true, Vector2? offset = null)
        {
            Vector2 mousePos = relativePos
                ? Event.current.mousePosition
                : GUIUtility.GUIToScreenPoint(Event.current.mousePosition);

            var xMin = r.xMin + (offset.HasValue ? offset.Value.x : 0);
            var xMax = r.xMax + (offset.HasValue ? offset.Value.x : 0);
            var yMin = r.yMin + (offset.HasValue ? offset.Value.y : 0);
            var yMax = r.yMax + (offset.HasValue ? offset.Value.y : 0);

            var condition = mousePos.x >= xMin && mousePos.x <= xMax &&
                            mousePos.y >= yMin && mousePos.y <= yMax;

            return condition;
        }



        /// <summary>
        /// Logs Message to the footer panel, msg fades after some seconds
        /// </summary>
        /// <param name="message">The displayed message</param>
        /// <param name="messageLevel">0-Info,1-Warning, 2-Error</param>
        private async void LogMessage(string message, int messageLevel = 0)
        {
            _debugMssg.style.opacity = 1;
            _debugMssg.text = message;
            Color color = messageLevel switch
            {
                1 => Color.yellow,
                2 => Color.red,
                _ => Color.white
            };
            _debugMssg.style.color = color;


            for (int i = 0; i < 1000; i++)
            {
                await Task.Delay(5);
                if (_debugMssg.text != message) return;

                var subval = i * 0.01f;
                if (subval > 1) break;

                _debugMssg.style.opacity = 1 - subval;
            }

            _debugMssg.text = "";
        }

        #endregion Utils
    }
}
