using System;
using System.Collections.Generic;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.Editor.View;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace JLBehaviourTree.Editor.EditorExTools
{
    public static class EditorExTools
    {
        /// <summary>
        /// 连接时添加数据
        /// </summary>
        public static void AddNodeData(this Edge edge)
        {
            NodeView outNodeView = edge.output.node as NodeView;
            NodeView inNodeView = edge.input.node as NodeView;
            outNodeView?.AddChild(inNodeView);
        }
    
        /// <summary>
        /// 删除连接时清除数据
        /// </summary>
        public static void RemoveLink(this Edge edge)
        {
            NodeView outNodeView = edge.output.node as NodeView;
            NodeView inNodeView = edge.input.node as NodeView;
            outNodeView?.RemoveChild(inNodeView);
            
        }
        
        /// <summary>
        /// 两点相连 output <==> input 
        /// </summary>
        public static void LinkPort(this Port outputSocket, Port inputSocket)
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

    }


}
