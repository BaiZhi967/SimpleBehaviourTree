using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.ExTools;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

namespace JLBehaviourTree.Editor.View
{
    [InitializeOnLoad]
    public static class PlayModeStateChangedExample
    {
        static PlayModeStateChangedExample()
        {
            EditorApplication.playModeStateChanged += LogPlayModeState;
        }

        private static void LogPlayModeState(PlayModeStateChange state)
        {
            if (!BehaviourTreeView.TreeWindow) return;
            if (state != PlayModeStateChange.ExitingEditMode && state != PlayModeStateChange.EnteredPlayMode) return;
            BehaviourTreeView.TreeWindow.Refresh();
        }
    }
    
    
    public class BehaviourTreeView : EditorWindow
    {
        public static BehaviourTreeView TreeWindow;
        public static MonoBehaviour BtCom;
        
        #region 元素
        public SplitView WindowRoot;
        #endregion

        [MenuItem("Tools/JLBehaviourTree/BehaviourTreeView _#&i")]
        public static void OpenView()
        {
            BehaviourTreeView wnd = GetWindow<BehaviourTreeView>();
            wnd.titleContent = new GUIContent("角落的行为树");
            TreeWindow = wnd;
        }
        
        /// <summary>
        /// 加载GUI
        /// </summary>
        public void CreateGUI()
        {
            BehaviourTreeDataInit();
        }

        private void OnInspectorUpdate()
        {
            WindowRoot?.TreeView?.OnGUI();
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed = Refresh;
        }

        private void OnDestroy()
        {
            Save();
        }

        private void OnSelectionChange()
        {
            
        }

        public void Refresh()
        {
            rootVisualElement.Clear();
            BehaviourTreeDataInit();
        }

        /// <summary>
        /// 初始化行为树数据
        /// </summary>
        public void BehaviourTreeDataInit()
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            TreeWindow = this;
            BtCom = (MonoBehaviour) (+ setting);
            VisualElement root = rootVisualElement;
            var visualTree = Resources.Load<VisualTreeAsset>("BehaviourTreeView");
            visualTree.CloneTree(root);
            WindowRoot = root.Q<SplitView>("SplitView");
            WindowRoot.TreeView = WindowRoot.Q<TreeView>();
            WindowRoot.InspectorView = WindowRoot.Q<InspectorView>();
            WindowRoot.InspectorTitle = WindowRoot.Q<Label>("InspectorTitle");
            
            //获取并设置窗口大小与位置
            IGetBtData treeData = +setting;
            if (treeData == null)return;
            var tr = (+treeData).ViewTransform;
            WindowRoot.TreeView.viewTransform.position = tr.position;
            WindowRoot.TreeView.viewTransform.scale = tr.scale;
            /*TODO 不需要用根生成了
            if (setting.TreeData == null)return;
            if (setting.TreeData.Root == null)return;
            CreateRoot(setting.TreeData.Root);*/
            if (treeData.GetBtData() == null)return;
            if (treeData.GetBtData().NodeData == null)return;

            treeData.GetBtData().NodeData
                .ForEach(n => CreateNodes(n,treeData.GetBtData().Root));
            WindowRoot.TreeView.nodes.OfType<NodeView>().ForEach(n => n.AddEdge());
        }

        public void CreateNodes(BtNodeBase nodeData,BtNodeBase rootNode = null)
        {
            TreeView view = TreeWindow.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(nodeData);
            if (nodeData == rootNode )
            {
                view.RootNode = nodeView;
            }
            nodeView.SetPosition(new Rect(nodeData.Position, Vector2.one));
            view.AddElement(nodeView);
        }

        /// <summary>
        /// 通过创建根节点创建树
        /// </summary>
        /// <param name="rootNode"></param>
        public void CreateRoot(BtNodeBase rootNode)
        {
            if (rootNode==null)return;
            TreeView view = TreeWindow.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(rootNode);
            nodeView.SetPosition(new Rect(rootNode.Position, Vector2.one));
            view.AddElement(nodeView);
            view.RootNode = nodeView;
            //view.NodeViews.Add(rootNode.Guid,nodeView);
            switch (rootNode)
            {
                case BtComposite composite:
                    composite.ChildNodes.ForEach(CreateNodeView);
                    break;
                case BtPrecondition precondition:
                    CreateNodeView(precondition.ChildNode);
                    break;
            }
        }
        
        /// <summary>
        /// 通过根节点去创建整颗树
        /// </summary>
        /// <param name="rootNode"></param>
        public void CreateNodeView(BtNodeBase rootNode)
        {
            if (rootNode==null)return;
            TreeView view = TreeWindow.WindowRoot.TreeView;
            NodeView nodeView = new NodeView(rootNode);
            nodeView.SetPosition(new Rect(rootNode.Position, Vector2.one));
            view.AddElement(nodeView);
            //view.NodeViews.Add(rootNode.Guid,nodeView);
            switch (rootNode)
            {
                case BtComposite composite:
                    composite.ChildNodes.ForEach(CreateNodeView);
                    break;
                case BtPrecondition precondition:
                    CreateNodeView(precondition.ChildNode);
                    break;
            }
        }

        public void Save()
        {
            if (Application.isPlaying)return;
            IGetBtData iGetBtData = +BehaviourTreeSetting.GetSetting();
            if (iGetBtData == null)return;
                
            (+iGetBtData).ViewTransform = new SaveTransform();
            (+iGetBtData).ViewTransform.position = WindowRoot.TreeView.viewTransform.position;
            (+iGetBtData).ViewTransform.scale = WindowRoot.TreeView.viewTransform.scale;
            var scene = SceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
    }
    
    public class RightClickMenu : ScriptableObject, ISearchWindowProvider
    {
        public delegate bool SelectEntryDelegate(SearchTreeEntry searchTreeEntry
            , SearchWindowContext context);

        public SelectEntryDelegate OnSelectEntryHandler;
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent("Create Node")));
            entries = AddNodeType<BtComposite>(entries, "组合节点");
            entries = AddNodeType<BtPrecondition>(entries, "条件节点");
            entries = AddNodeType<BtActionNode>(entries, "行为节点");
            return entries;
        }
        
        /// <summary>
        /// 通过反射获取对应的菜单数据
        /// </summary>
        public List<SearchTreeEntry> AddNodeType<T>(List<SearchTreeEntry> entries, string pathName)
        {
            entries.Add(new SearchTreeGroupEntry(new GUIContent(pathName)) { level = 1 });
            List<System.Type> rootNodeTypes = typeof(T).GetDerivedClasses();
            foreach (var rootType in rootNodeTypes)
            {
                string menuName = rootType.Name;
                if (rootType.GetCustomAttribute(typeof(NodeLabelAttribute)) is NodeLabelAttribute nodeLabel)
                {
                    menuName = nodeLabel.MenuName;
                    if (nodeLabel.MenuName == "")
                    {
                        menuName = rootType.Name;
                    }
                }
                entries.Add(new SearchTreeEntry(new GUIContent(menuName)) { level = 2,userData = rootType});
            }
            return entries;
        }


        public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
        {
            if (OnSelectEntryHandler == null)
            {
                return false;
            }
            return OnSelectEntryHandler(SearchTreeEntry, context);
        }
    }
}
