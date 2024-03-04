using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.BTAutoLayout;
using JLBehaviourTree.Editor.EditorExTools;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Edge = UnityEditor.Experimental.GraphView.Edge;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;

namespace JLBehaviourTree.Editor.View
{
    public class TreeView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<TreeView,UxmlTraits>{}

        public NodeView RootNode;
        public Dictionary<string, NodeView> NodeViews;

        public TreeView()
        {
            Insert(0,new GridBackground());
            
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/JLBehaviourTree/Editor/Resources/BehaviourTreeView.uss"));
            //增加右键菜单
            GraphViewMenu();
            //选择对象
            RegisterCallback<KeyDownEvent>(KeyDownControl);
            RegisterCallback<MouseEnterEvent>(MouseEnterControl);
            this.graphViewChanged = OnGraphViewChanged;
            NodeViews = new Dictionary<string, NodeView>();
            
        }

        public void OnGUI()
        {
            //OnDrawMove();
            nodes.ForEach(node => (node as NodeView).UpdateNodeView());

        }
        
        public void OnStartMove()
        {
            NodeAutoLayouter.Layout(new RNG_LayoutNodeConvertor().Init(RootNode));
        }
        
        /// <summary>
        /// 添加节点时放入字典
        /// </summary>
        /// <param name="graphElement"></param>
        public new void AddElement(GraphElement graphElement)
        {
            base.AddElement(graphElement);
            if (graphElement is NodeView nodeView)
            {
                //Debug.Log($" 名称: {nodeView.NodeViewData.NodeName} -- Guid: {nodeView.NodeViewData.Guid} ");
                NodeViews.Add(nodeView.NodeViewData.Guid,nodeView);
            }
        }
        
        public override void RemoveFromSelection(ISelectable selectable)
        {
            base.RemoveFromSelection(selectable);
            switch (selectable)
            {
                case Edge edge:
                    edge.RemoveLink();
                    break;
                case NodeView view:
                    NodeViews.Remove(view.NodeViewData.Guid);
                    (+BehaviourTreeSetting.GetSetting()).GetBtData().NodeData.Remove(view.NodeViewData);
                    break;
            }
        }

        public new void RemoveElement(GraphElement graphElement)
        {
            base.RemoveElement(graphElement);
            if (graphElement is NodeView view)
            {
                NodeViews.Remove(view.NodeViewData.Guid);
                (+BehaviourTreeSetting.GetSetting()).GetBtData().NodeData.Remove(view.NodeViewData);
            }
            
        }
    
        
        #region 按键回调
        private void MouseEnterControl(MouseEnterEvent evt)
        {
            
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
            
        }

        private void KeyDownControl(KeyDownEvent evt)
        {
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
            if (evt.keyCode == KeyCode.Tab)
            {
                evt.StopPropagation();
            }

            if (!evt.ctrlKey) return;
            switch (evt.keyCode)
            {
                case KeyCode.S:
                    BehaviourTreeView.TreeWindow.Save();
                    evt.StopPropagation();
                    break;
                case KeyCode.E:
                    OnStartMove();
                    evt.StopPropagation();
                    break;
                case KeyCode.X:
                    Cut(null);
                    evt.StopPropagation();
                    break;
                case KeyCode.C:
                    Copy(null);
                    evt.StopPropagation();
                    break;
                case KeyCode.V:
                    Paste(null);
                    evt.StopPropagation();
                    break;
            }
        }


        public void Cut(DropdownMenuAction da)
        {
            Copy(null);
            base.CutSelectionCallback();
        }

        private void Copy(DropdownMenuAction da)
        {
            var ns =selection.OfType<NodeView>()
                .Select(n => n as NodeView)
                .Select(n=> n.NodeViewData).ToList();
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
    
            setting.CopyNode = ns.CloneData();
        }

        private void Paste(DropdownMenuAction da)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            if (setting.CopyNode ==null)return;
            if (setting.CopyNode.Count == 0) return;
            ClearSelection();
            List<NodeView> pasteNode = new List<NodeView>();
            //生成节点并选择，重新序列化克隆的节点
            for (int i = 0; i < setting.CopyNode.Count; i++)
            {
                NodeView node = new NodeView(setting.CopyNode[i]);
                this.AddElement(node);
                node.SetPosition(new Rect(setting.CopyNode[i].Position,Vector2.one));
                AddToSelection(node);
                pasteNode.Add(node);
                (+setting).GetBtData().NodeData.Add(setting.CopyNode[i]);
            }
            pasteNode.ForEach(n=>n.AddEdge());
            setting.CopyNode = setting.CopyNode.CloneData();
        }
        
        


        private GraphViewChange OnGraphViewChanged(GraphViewChange viewChange)
        {
            viewChange.edgesToCreate?.ForEach(edge =>
            {
                edge.AddNodeData();
            });
            viewChange.elementsToRemove?.ForEach(element =>
            {
                switch (element)
                {
                    case Edge edge:
                        edge.RemoveLink();
                        break;
                    case NodeView view:
                        NodeViews.Remove(view.NodeViewData.Guid);
                        (+BehaviourTreeSetting.GetSetting()).GetBtData().NodeData.Remove(view.NodeViewData);
                        break;
                }
            });
            return viewChange;
        }
        #endregion

        #region 右键菜单

        private RightClickMenu menuWindowProvider;
        public void GraphViewMenu()
        {
            menuWindowProvider = ScriptableObject.CreateInstance<RightClickMenu>();
            menuWindowProvider.OnSelectEntryHandler = OnMenuSelectEntry;
        
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), menuWindowProvider);
            };
        }
        
        //点击右键菜单时触发
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction("Create Node", _ =>
            {
                var windowRoot = BehaviourTreeView.TreeWindow.rootVisualElement;
                var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, 
                    _.eventInfo.mousePosition+BehaviourTreeView.TreeWindow.position.position);
                SearchWindow.Open(new SearchWindowContext(windowMousePosition), menuWindowProvider);
            });
            evt.menu.AppendAction("Cut",Cut);
            evt.menu.AppendAction("Copy",Copy);
            evt.menu.AppendAction("Paste",Paste);
            base.BuildContextualMenu(evt);
            //evt.menu.MenuItems().Remove(evt.menu.MenuItems().Find(match => match.ToString() == ""));
            
        }

        //点击菜单时菜单创建Node
        private bool OnMenuSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var windowRoot = BehaviourTreeView.TreeWindow.rootVisualElement;
            
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, 
                context.screenMousePosition - BehaviourTreeView.TreeWindow.position.position);
            
            var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
            var nodeBase = System.Activator.CreateInstance((System.Type)searchTreeEntry.userData) as BtNodeBase;
            var nodeLabel = nodeBase.GetType().GetCustomAttribute(typeof(NodeLabelAttribute)) as NodeLabelAttribute;
            nodeBase.NodeName = nodeBase.GetType().Name;
            if (nodeLabel!=null)
            {
                if (nodeLabel.Label != "")
                {
                    nodeBase.NodeName = nodeLabel.Label;
                }
            }
            nodeBase.NodeType = nodeBase.GetType().GetNodeType();
            nodeBase.Position = graphMousePosition;
            nodeBase.Guid = System.Guid.NewGuid().ToString();
            NodeView group =  new NodeView(nodeBase);
            group.SetPosition(new Rect(graphMousePosition, Vector2.one));
            this.AddElement(group);
            (+BehaviourTreeSetting.GetSetting()).GetBtData().NodeData.Add(nodeBase);
            AddToSelection(group);
            return true;
        }

        //覆写GetCompatiblePorts 定义链接规则
        public override List<Port> GetCompatiblePorts(Port startAnchor, NodeAdapter nodeAdapter)
        {
            return ports.Where(endPorts => 
                endPorts.direction != startAnchor.direction && endPorts.node != startAnchor.node)
                .ToList();
        }
        
        #endregion
        
    }
}