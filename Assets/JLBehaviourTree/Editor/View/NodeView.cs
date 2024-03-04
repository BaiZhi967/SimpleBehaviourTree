using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.Editor.EditorExTools;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Direction = UnityEditor.Experimental.GraphView.Direction;

namespace JLBehaviourTree.Editor.View
{
    public class NodeView : Node
    {
        [LabelText("节点视图数据"), OdinSerialize, HideReferenceObjectPicker]
        public BtNodeBase NodeViewData;

        #region 连接数据
        [LabelText("输入端口"), HideIf("@true")] public Port InputPort;
        [LabelText("输出端口"), HideIf("@true")] public Port OutputPort;
        #endregion

        #region 节点样式
        //背景
        private VisualElement _nodeBorderBar;
        //标题背景
        private VisualElement _nodeTitleBar;
        //标题
        private Label _titleLabel;

        #endregion
        
        public NodeView(BtNodeBase NodeViewData) : base(
            "Assets/JLBehaviourTree/Editor/Resources/NodeView.uxml")
        {
            this.NodeViewData = NodeViewData;
            InitNodeView();
            InitNodeStyleData();
            
        }

        /// <summary>
        /// 初始化节点视图
        /// </summary>
        private void InitNodeView()
        {
            switch (NodeViewData.NodeType)
            {
                case NodeType.组合节点:
                    LoadCompositeNode();
                    break;
                case NodeType.条件节点:
                    LoadPreconditionNode();
                    break;
                case NodeType.行为节点:
                    LoadActionNode();
                    break;
            }
            
        }

        private async void InitNodeStyleData()
        {
            await UniTask.Delay(50);
            _nodeBorderBar = this.Q<VisualElement>("node-border");
            _nodeTitleBar = this.Q<VisualElement>("title");
            _titleLabel = this.Q<Label>("title-label");
            ChangeBgColor(BehaviourTreeSetting.GetSetting().GetNodeBgColor(NodeViewData));
            ChangeTitleColor(BehaviourTreeSetting.GetSetting().GetNodeTitleColor(NodeViewData));
        }
        
        /// <summary>
        /// 查询子节点并连接线条
        /// </summary>
        public async void AddEdge()
        {
            TreeView view = BehaviourTreeView.TreeWindow.WindowRoot.TreeView;
            switch (NodeViewData)
            {
                case BtComposite composite:
                    foreach (var t in composite.ChildNodes)
                    {
                        LinkNodes(OutputPort, view.NodeViews[t.Guid].InputPort);
                    }
                    break;
                case BtPrecondition precondition:
                    if (precondition.ChildNode == null)break;
                    LinkNodes(OutputPort, view.NodeViews[precondition.ChildNode.Guid].InputPort);
                    break;
            }
        }

        /// <summary>
        /// 添加子对象
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(NodeView node)
        {
            switch (NodeViewData)
            {
                case BtComposite composite:
                    composite.ChildNodes.Add(node.NodeViewData);
                    break;
                case BtPrecondition precondition:
                    precondition.ChildNode = node.NodeViewData;
                    break;
            }
        }
        
        /// <summary>
        /// 移除子对象
        /// </summary>
        /// <param name="node"></param>
        public void RemoveChild(NodeView node)
        {
            switch (NodeViewData)
            {
                case BtComposite composite:
                    composite.ChildNodes.Remove(node.NodeViewData);
                    break;
                case BtPrecondition precondition:
                    if (precondition.ChildNode == node.NodeViewData)
                    {
                        precondition.ChildNode = null;
                    }
                    break;
            }
        }

        void LinkNodes(Port outputSocket, Port inputSocket)
        {
            var tempEdge = new EdgeView()
            {
                output = outputSocket,
                input = inputSocket
            };
            tempEdge?.input.Connect(tempEdge);
            tempEdge?.output.Connect(tempEdge);
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.Add(tempEdge);
        }



        public override void OnSelected()
        {
            base.OnSelected();
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            BehaviourTreeView.TreeWindow.WindowRoot.InspectorView.UpdateInspector();
        }
        
        private void LoadCompositeNode()
        {
            InputPort = PortView.Create<Edge>();
            OutputPort = PortView.Create<Edge>(dir: Direction.Output, cap: Port.Capacity.Multi);
            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }

        private void LoadActionNode()
        {
            InputPort = PortView.Create<Edge>();
            inputContainer.Add(InputPort);
        }

        private void LoadPreconditionNode()
        {
            InputPort = PortView.Create<Edge>();
            OutputPort = PortView.Create<Edge>(dir: Direction.Output, cap: Port.Capacity.Single);
            inputContainer.Add(InputPort);
            outputContainer.Add(OutputPort);
        }
        
        /// <summary>
        /// 每帧更新
        /// </summary>
        public void UpdateNodeView()
        {
            //更新名称
            title = NodeViewData.NodeName;
            ChangeTitleColor(BehaviourTreeSetting.GetSetting().GetNodeTitleColor(NodeViewData));
            if (Application.isPlaying && NodeViewData.NodeState == BehaviourState.执行中)
            {
                ChangeBgColor(Color.cyan); 
                //更新线运行
            }
            else
            {
                //更新颜色
                ChangeBgColor(BehaviourTreeSetting.GetSetting().GetNodeBgColor(NodeViewData));
            }
            if (Application.isPlaying&&InputPort.connected)
            {
                UpdateEdgeState();
            }
            //更新子节点顺序
            if ((NodeViewData is BtComposite))
            {
                (NodeViewData as BtComposite)?.ChildNodes
                    .Sort((x, y) =>x.Position.x.CompareTo(y.Position.x));
            }
           
        }

        void ChangeBgColor(Color color)
        {
            if (_nodeBorderBar == null || _nodeTitleBar == null)return;
            _nodeBorderBar.style.unityBackgroundImageTintColor = color;
            _nodeTitleBar.style.unityBackgroundImageTintColor = color;
        }
        void ChangeTitleColor(Color color)
        {
            if (_titleLabel == null)return;
            _titleLabel.style.color = color;
        }

        /// <summary>
        /// 根据对象运行状态改变线条
        /// </summary>
        void UpdateEdgeState()
        {
            if (NodeViewData.NodeState == BehaviourState.执行中)
            {
                (InputPort.connections.First() as EdgeView).OnStartMovePoints();
            }
            else
            {
                (InputPort.connections.First() as EdgeView).OnStopMovePoints();
            }
            
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            NodeViewData.Position = new Vector2(newPos.xMin, newPos.yMin);
        }
        
        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            //base.BuildContextualMenu(evt);
            //evt.menu.MenuItems().Remove(evt.menu.MenuItems().Find(match => match.ToString() == ""));
            //evt.menu.AppendAction("Create Group",CreateGroup);
            evt.menu.AppendAction("设为根节点",SetRoot);
        }

        private void SetRoot(DropdownMenuAction obj)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.RootNode = this;;
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.OnStartMove();
            (+setting).GetBtData().Root = NodeViewData;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <returns></returns>
        public static bool operator+ (NodeView p1,NodeView p2)
        {
            p1.LinkNodes(p1.OutputPort, p2.InputPort);
            return false;
        }
    }

    public class PortView
    {
        public static Port Create<TEdge>(Orientation ori = Orientation.Vertical, Direction dir = Direction.Input,
            Port.Capacity cap = Port.Capacity.Single, Type type = null) where TEdge : Edge, new()
        {
            Port port = Port.Create<TEdge>(ori, dir, cap, type);
            port.portName = "";

            port.style.flexDirection = dir == Direction.Input ? FlexDirection.Column : FlexDirection.ColumnReverse;
            return port;
        }
    }
}