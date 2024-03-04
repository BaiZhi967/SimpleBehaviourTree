using System.Collections.Generic;
using JLBehaviourTree.BehaviourTree;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace JLBehaviourTree.Editor.View
{
    /// <summary>
    /// 面板中序列化显示的数据
    /// </summary>
    public class InspectorDataView : SerializedScriptableObject
    {
        [OdinSerialize,LabelText("选择的点"),HideReferenceObjectPicker][ListDrawerSettings(IsReadOnly = true)]
        public HashSet<BtNodeBase> selectDatas;
 
        public InspectorDataView()
        {
            selectDatas = new HashSet<BtNodeBase>();
        }
        
        
    }


    /*public class ExposureNode
    {
        [HideIf("@true")]
        public NodeView NodeView;
        [HideIf("@true")]
        public BtNodeBase NodeData;
        /*[ReadOnly,HideLabel,HorizontalGroup("nodeData"),HideReferenceObjectPicker]
        public System.Type FieldType;#1#
        [HideLabel,HorizontalGroup("nodeData"),HideIf("@true")]
        public string FiledName;

        public object FiledObject;

    }*/
}

