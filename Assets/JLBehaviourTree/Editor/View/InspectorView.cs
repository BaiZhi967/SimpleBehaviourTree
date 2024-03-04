using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JLBehaviourTree.BehaviourTree;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using Sirenix.Utilities;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.Editor.View
{
    public class InspectorView : VisualElement
    {
        public IMGUIContainer inspectorBar;

        public InspectorDataView InspectorDataView;
        public new class UxmlFactory : UxmlFactory<InspectorView,UxmlTraits>{}
        
        public InspectorView()
        {
            Init();
        }

        void Init()
        {
            inspectorBar = new IMGUIContainer() { name = "inspectorBar" };
            inspectorBar.style.flexGrow = 1;
            CreateInspectorView();
            Add(inspectorBar);
        }

        /// <summary>
        /// 更新选择节点面板显示
        /// </summary>
        public void UpdateInspector()
        {
            InspectorDataView.selectDatas.Clear();
            BehaviourTreeView.TreeWindow.WindowRoot.TreeView.selection
                .Select(node => node as NodeView)
                .ForEach(node=>
                {
                    if (node != null)
                    {
                        InspectorDataView.selectDatas.Add(node.NodeViewData);
                    }
                });
        }

        private async void CreateInspectorView()
        {
            InspectorDataView = Resources.Load<InspectorDataView>("InspectorDataView");
            if (!InspectorDataView)
            {
                InspectorDataView = ScriptableObject.CreateInstance<InspectorDataView>();
                AssetDatabase.CreateAsset(InspectorDataView,"Assets/JLBehaviourTree/Editor/Resources/InspectorDataView.asset");
            }
            await Task.Delay(100);
            var odinEditor = UnityEditor.Editor.CreateEditor(InspectorDataView);
            
            inspectorBar.onGUIHandler += () =>
            {
                odinEditor.OnInspectorGUI();
            };
        }
    }
    
    
    
    
}