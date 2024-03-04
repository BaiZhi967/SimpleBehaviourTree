using System;
using System.Collections.Generic;
using JLBehaviourTree.BehaviourTree;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Object = UnityEngine.Object;

namespace JLBehaviourTree.ExTools
{
    [CreateAssetMenu(menuName = "行为树数据/设置数据")]
    public class BehaviourTreeSetting : SerializedScriptableObject
    { 
        public BehaviourTreeData TreeData;
        public int TreeInstanceID;
        
        public List<BtNodeBase> CopyNode = new List<BtNodeBase>();

        #region 颜色设置
        [HideIf("@true"),OdinSerialize]
        private Color _colorAll;
        [LabelText("所有节点背景颜色"),FoldoutGroup("背景颜色"),ShowInInspector]
        public Color ColorAll
        {
            get => _colorAll;
            set
            {
                _colorAll = value;
                compositeColor = value;
                preconditionColor = value;
                actionColor = value;
            }
        }
        [LabelText("组件节点颜色"),FoldoutGroup("背景颜色")]
        public Color compositeColor;
        [LabelText("条件节点颜色"),FoldoutGroup("背景颜色")]
        public Color preconditionColor;
        [LabelText("行为节点颜色"),FoldoutGroup("背景颜色")]
        public Color actionColor;
        [HideIf("@true"),OdinSerialize]
        private Color _textColorAll;
        [LabelText("所有节点字体颜色"),FoldoutGroup("字体颜色"),ShowInInspector]
        public Color TextColorAll
        {
            get => _textColorAll;
            set
            {
                _textColorAll = value;
                compositeTextColor = value;
                preconditionTextColor = value;
                actionTextColor = value;
            }
        }
        [LabelText("组件字体颜色"),FoldoutGroup("字体颜色")]
        public Color compositeTextColor;
        [LabelText("条件字体颜色"),FoldoutGroup("字体颜色")]
        public Color preconditionTextColor;
        [LabelText("行为字体颜色"), FoldoutGroup("字体颜色")]
        public Color actionTextColor;
        #endregion
        

        public static BehaviourTreeSetting GetSetting()
        {
            return Resources.Load<BehaviourTreeSetting>("BehaviourTreeSetting");
        }

        public static IGetBtData operator +(BehaviourTreeSetting treeSetting)
        {
#if UNITY_EDITOR
            return UnityEditor.EditorUtility.InstanceIDToObject(treeSetting.TreeInstanceID) as IGetBtData;
#endif              
        }


        /*public IGetBtData GetTreeData()
        {
            EditorUtility.InstanceIDToObject(setting.TreeInstanceID) as IGetBtData;
        }*/

        

        public Color GetNodeBgColor(BtNodeBase node)
        {
            switch (node)
            {
                case BtComposite composite:
                    return compositeColor;
                case BtPrecondition precondition:
                    return preconditionColor;
                case BtActionNode actionNode:
                    return actionColor;
            }
            return Color.white;
        }
        
        public Color GetNodeTitleColor(BtNodeBase node)
        {
            switch (node)
            {
                case BtComposite composite:
                    return compositeTextColor;
                case BtPrecondition precondition:
                    return preconditionTextColor;
                case BtActionNode actionNode:
                    return actionTextColor;
            }
            return Color.white;
        }
    }
    
}
