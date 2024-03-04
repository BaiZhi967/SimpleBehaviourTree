using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JLBehaviourTree.BehaviourTree;
using Sirenix.Serialization;
using UnityEngine;

namespace JLBehaviourTree.ExTools
{
    public static class ExTool
    {
        /// <summary>
        /// 获取所有继承了该类型的类型，包含子类的子类
        /// </summary>
        public static List<Type> GetDerivedClasses(this Type type)
        {
            List<Type> derivedClasses = new List<Type>();
            
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    if (t.IsClass && !t.IsAbstract && type.IsAssignableFrom(t))
                    {
                        derivedClasses.Add(t);
                    }
                }
            }
            return derivedClasses;
        }

        public static string GetPath(this string path,string FlieName)
        {
            int i = 0;
            while (File.Exists($"{path}/{FlieName}{i}.asset"))
            {
                i++;
            }
            return $"{path}/{FlieName}{i}.asset";
        }

        public static NodeType GetNodeType(this Type type)
        {
            if (type.IsSubclassOf(typeof(BtComposite))) return NodeType.组合节点;
            if (type.IsSubclassOf(typeof(BtPrecondition))) return NodeType.条件节点;
            if (type.IsSubclassOf(typeof(BtActionNode))) return NodeType.行为节点;

            return NodeType.无;
        }
        
        /// <summary>
        /// 用odin序列化去克隆选择的节点
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static List<BtNodeBase> CloneData(this List<BtNodeBase> nodes)
        {
            byte[] nodeBytes= SerializationUtility.SerializeValue(nodes, DataFormat.Binary);
            var toNode = SerializationUtility.DeserializeValue<List<BtNodeBase>>(nodeBytes ,DataFormat.Binary);
            
            //删掉未复制的子数据 并随机新的Guid 位置向右下偏移
            for (int i = 0; i < toNode.Count; i++)
            {
                toNode[i].Guid = System.Guid.NewGuid().ToString(); 
                switch (toNode[i])
                {
                    case BtComposite composite:
                        if (composite.ChildNodes .Count==0)break;
                        composite.ChildNodes = composite.ChildNodes.Intersect(toNode).ToList();
                        break;
                    case BtPrecondition precondition :
                        if (precondition.ChildNode == null)break;
                        if (!toNode.Exists(n => n == precondition.ChildNode))
                        {
                            precondition.ChildNode = null;
                        }
                        break;
                }
                toNode[i].Position += Vector2.one * 30;
            }
            
            return toNode;
        }
    }

    #region 特性
    /// <summary>
    /// 给需要暴露的属性标记
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class AddExposureAttribute : Attribute { }
    
    /// <summary>
    /// 给节点命名
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeLabelAttribute : Attribute
    {
        /// <summary>
        /// 菜单名称
        /// </summary>
        public string MenuName;
        /// <summary>
        /// 标题名称
        /// </summary>
        public string Label;
        /// <param name="menuName">在菜单中显示的名称</param>
        public NodeLabelAttribute(string menuName) =>MenuName = menuName;
    }
    
    #endregion

    #region 接口
    
    /// <summary>
    /// 获取当前对象上的树
    /// </summary>
    public interface IGetBtData
    {
        public BehaviourTreeData GetBtData();
        public void OpenView();

        public static BehaviourTreeData operator +(IGetBtData iGetBtData) => iGetBtData.GetBtData();

        public static bool operator -(IGetBtData iGetBtData)
        {
            iGetBtData.OpenView();
            return true;
        }
    }


    #endregion
}
