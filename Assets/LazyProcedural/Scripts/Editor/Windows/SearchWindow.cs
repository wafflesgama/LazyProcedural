using Sceelix.Core.Procedures;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LazyProcedural
{
    public class SearchWindow : EditorWindow
    {
        private VisualElement _root;
        private GraphWindow _parentWindow;

        public bool sucess;

        //Header
        private TextField _searchField;
        private VisualElement _searchIcon;

        //Body
        private VisualElement _bodyList;

        private List<Foldout> categories;

        public GraphWindow graphWindow;

        private IEnumerable<ProcedureInfo> allProcedures;
        private IEnumerable<ProcedureInfo> filteredProcedures;

        private Dictionary<Button, Action> buttonsList;
        private int currentHighlightedProcedure;

        private bool initialized;

        private static string lastSearch = "";

        private StyleColor? defaultProcedureColor;

        public SearchWindow(IEnumerable<ProcedureInfo> procedures, GraphWindow parentWindow)
        {
            titleContent.text = "Add Node";
            allProcedures = procedures.OrderBy(x => x.Label);
            filteredProcedures = allProcedures;
            _parentWindow = parentWindow;
        }

        //public void Init(string title, string message, string ok, string cancel, string defaultLabel)
        //{
        //    titleContent.text = title;
        //    this.message = message;
        //    this.ok = ok;
        //    this.cancel = cancel;
        //}

        private void OnFocus()
        {
            if (!initialized)
                Setup();
        }

        private void Setup()
        {
            if (rootVisualElement == null) return;

            SetupVariables();
            SetupBaseUI();
            SetupBindings();
            //SetupInfo();
            SetupCallbacks();
            SetupInputCallbacks();

            SetupIcons();

            SetupProcedures();

            _searchField.value = lastSearch;
            _searchField.SelectAll();
            _searchField.Focus();

            initialized = true;
        }


        private void OnLostFocus()
        {
            Close();
        }

        private void OnDestroy()
        {
            lastSearch = _searchField.value;

            //if (this.OnClose != null)
            //    OnClose.Invoke(sucess, GetInputValue());
        }

        private void SetupVariables()
        {
            buttonsList = new Dictionary<Button, Action>();
        }


        private void SetupBaseUI()
        {
            _root = rootVisualElement;

            // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
            var quickToolVisualTree = (VisualTreeAsset)AssetDatabase.LoadAssetAtPath(PathFactory.BuildUiFilePath(PathFactory.SEARCH_WINDOW_LAYOUT_FILE), typeof(VisualTreeAsset));

            if (quickToolVisualTree == null) return;

            quickToolVisualTree.CloneTree(_root);

            var styleSheet = (StyleSheet)AssetDatabase.LoadAssetAtPath(PathFactory.BuildUiFilePath(PathFactory.SEARCH_WINDOW_LAYOUT_FILE, false), typeof(StyleSheet));
            _root.styleSheets.Add(styleSheet);


        }

        private void SetupBindings()
        {

            _searchField = (TextField)_root.Q("Search");
            _searchIcon = _root.Q("SearchIcon");


            _bodyList = _root.Q("Body");

            //new Button().cli
            //_message = (Label)_root.Q("Message");
            //_inputField = (TextField)_root.Q("InputField");
            //_okBttn = (Button)_root.Q("OkBttn");
            //_cancelBttn = (Button)_root.Q("CancelBttn");
        }



        private void SetupCallbacks()
        {
            //_okBttn.RegisterCallback<ClickEvent>((x) => OkClicked());
            //_cancelBttn.RegisterCallback<ClickEvent>((x) => this.Close());

            _searchField.RegisterValueChangedCallback(x => Search());
        }

        private void SetupInputCallbacks()
        {
            _root.RegisterCallback<KeyDownEvent>(OnKeyboardKeyDown, TrickleDown.TrickleDown);
        }


        private void OnKeyboardKeyDown(KeyDownEvent e)
        {
            if (e.keyCode == KeyCode.Escape)
            {
                _parentWindow.FocusOnGraph();
                this.Close();
            }



            if (e.keyCode == KeyCode.UpArrow)
            {
                HighlightPreviousProcedure();
            }
            else if (e.keyCode == KeyCode.DownArrow)
            {
                HighlightNextProcedures();
            }
            else if (e.keyCode == KeyCode.Return)
            {
                SelectHighlightedProcedure();
            }
        }

        private void SetupIcons()
        {
            _searchIcon.style.backgroundImage = (Texture2D)EditorGUIUtility.IconContent("d_Search Icon").image;
        }

        private void SetupProcedures(bool fromSearch = false)
        {
            Dictionary<Button, Action> unsortedButtonsList = new Dictionary<Button, Action>();
            categories = new List<Foldout>();
            _bodyList.Clear();
            Button focusBttn = null;

            foreach (var procedure in filteredProcedures)
            {
                var b = new Button();
                b.text = procedure.Label;
                b.style.unityTextAlign = TextAnchor.MiddleLeft;

                Action clickFunction = () =>
                {
                    graphWindow.AddNode(procedure);
                    _parentWindow.FocusOnGraph();
                    this.Close();
                    //b.userData
                };

                b.clicked += clickFunction;

                unsortedButtonsList.Add(b, clickFunction);

                var categoryContainer = categories.FirstOrDefault(x => x.name == procedure.Category);
                if (categoryContainer == null)
                {
                    categoryContainer = new Foldout();
                    categoryContainer.name = procedure.Category;
                    categoryContainer.text = procedure.Category;
                    categoryContainer.value = fromSearch;
                    categoryContainer.AddToClassList("category");
                    categoryContainer.style.minHeight = 25;
                    categories.Add(categoryContainer);
                }
                categoryContainer.Add(b);
            }

            if (!fromSearch)
                categories = categories.OrderBy(x => x.name).ToList();

            //Match buttons Key indexes to Ui category sorting order

            buttonsList.Clear();
            foreach (var category in categories)
            {
                _bodyList.Add(category);

                var children = category.Children();
                foreach (var child in children)
                {
                    Button button = (Button)child;
                    buttonsList.Add(button, unsortedButtonsList[button]);
                }
            }



            HiglightProcedure(0);
        }


        private void Search()
        {
            var searchVal = _searchField.value;
            bool openCategories = true;

            if (String.IsNullOrWhiteSpace(searchVal))
            {
                filteredProcedures = allProcedures;
                openCategories = false;
            }
            else
                filteredProcedures = SearchEngine.FuzzySearch(allProcedures, searchVal);


            SetupProcedures(openCategories);

        }

        private void HiglightProcedure(int index)
        {
            if (!defaultProcedureColor.HasValue)
                defaultProcedureColor = buttonsList.ElementAt(0).Key.style.backgroundColor;
            else
                buttonsList.ElementAt(currentHighlightedProcedure).Key.style.backgroundColor = defaultProcedureColor.Value;

            currentHighlightedProcedure = index;
            buttonsList.ElementAt(currentHighlightedProcedure).Key.style.backgroundColor = new Color(0.169f, 0.365f, 0.529f);
        }

        private void SelectHighlightedProcedure()
        {
            buttonsList.ElementAt(currentHighlightedProcedure).Value.Invoke();
        }

        private void HighlightNextProcedures()
        {
            if (currentHighlightedProcedure + 1 >= buttonsList.Count) return;

            HiglightProcedure(currentHighlightedProcedure + 1);
        }

        private void HighlightPreviousProcedure()
        {
            if (currentHighlightedProcedure - 1 < 0) return;

            HiglightProcedure(currentHighlightedProcedure - 1);
        }


        private void OnProcedureClicked()
        {
            throw new System.NotImplementedException();
        }
    }
}