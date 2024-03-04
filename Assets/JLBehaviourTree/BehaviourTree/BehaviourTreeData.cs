using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.BehaviourTree
{
    [BoxGroup]
    [HideReferenceObjectPicker]
    [LabelText("行为树数据")]
    public class BehaviourTreeData
    {
        [LabelText("是否显示数据")]
        public bool IsShow = false;
        [LabelText("根数据"),OdinSerialize,ShowIf("IsShow")]
        public BtNodeBase Root;
        
        [LabelText("根数据"), OdinSerialize, ShowIf("IsShow")]
        public List<BtNodeBase> NodeData = new List<BtNodeBase>();

        [OdinSerialize,ShowIf("IsShow"),HideReferenceObjectPicker]
        public SaveTransform ViewTransform;
       
        
        private bool _isActive;
        [Button("开始"),ButtonGroup("控制")]
        public void OnStart()
        {
            _isActive = true;
            OnUpdate();
        }
        private async void OnUpdate()
        {
            while (_isActive)
            {
                Root?.Tick();
                await UniTask.Yield();
            }
        }
        [Button("结束"),ButtonGroup("控制")]
        public void OnStop() => _isActive = false;

                
#if UNITY_EDITOR
        public void OpenView(int instanceID)
        {
            BehaviourTreeSetting setting = BehaviourTreeSetting.GetSetting();
            setting.TreeInstanceID = instanceID;
            UnityEditor.EditorUtility.SetDirty(setting);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.EditorApplication.ExecuteMenuItem("Tools/JLBehaviourTree/BehaviourTreeView");
            
        }
#endif
    }
}

public class SaveTransform
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector3 scale;
    public Matrix4x4 matrix;
}
