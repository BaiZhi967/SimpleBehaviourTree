using System;
using JLBehaviourTree.BehaviourTree;
using JLBehaviourTree.ExTools;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;

namespace JLBehaviourTree.Test
{
    public class BehaviourTreeTest : SerializedMonoBehaviour,IGetBtData
    {
        [OdinSerialize,HideReferenceObjectPicker]
        public BehaviourTreeData TreeData;
        
        /*public void FixedUpdate()
        {
            TreeData?.Root?.Tick();
        }*/
        public BehaviourTreeData GetBtData() => TreeData;
        [Button("打开视图")]
        public void OpenView()
        {
            TreeData.OpenView(GetInstanceID());
        }
    }
    
    [NodeLabel("同时执行")]
    public class Parallel : BtComposite
    {
        [LabelText("当前执行的")]
        private int _playIndex;
        public override BehaviourState Tick()
        {
            for (int i = _playIndex; i < ChildNodes.Count; i++)
            {
                var state = ChildNodes[i].Tick();
                switch (state)
                {
                    case BehaviourState.执行中:
                        return NodeState = state;
                    case BehaviourState.失败:
                        _playIndex = 0;
                        return NodeState = state;
                }
                if (i != ChildNodes.Count - 1) continue;
                _playIndex = 0;
                return NodeState = BehaviourState.成功;
            }
            ChangeFailState();
                return NodeState;
        }
    }
    /// <summary>
    /// 延时执行
    /// </summary>
    [NodeLabel("延时节点",Label = "延时执行")]
    public class Delay : BtPrecondition
    {
        [LabelText("延时"),SerializeField]
        private float timer;

        private float _currentTimer;
        public override BehaviourState Tick()
        {
            _currentTimer += Time.deltaTime;
            if (_currentTimer>=timer)
            {
                _currentTimer = 0f;
                ChildNode.Tick();
                return NodeState = BehaviourState.成功;
            }
            return NodeState = BehaviourState.执行中;
        }
    }
    
    [NodeLabel("播放动画")]
    public class AnimatorPlay : BtActionNode
    {
        [LabelText("名称"), SerializeField,FoldoutGroup("@NodeName")] 
        private string animationName;
        [LabelText("动画"), SerializeField,FoldoutGroup("@NodeName")]
        private Animator animator;

        public override BehaviourState Tick()
        {
            if (!animator)
            {
                ChangeFailState();
                return NodeState;
            }
            animator.Play(animationName);
            return NodeState = BehaviourState.成功;
        }
    
    }
    
    [NodeLabel("启用对象")]
    public class SetObjectActive : BtActionNode
    {
        [LabelText("是否启用"), SerializeField,FoldoutGroup("@NodeName")] 
        private bool _isActive;
        [LabelText("启用对象"), SerializeField,FoldoutGroup("@NodeName")] 
        private GameObject _particle;
        
        public override BehaviourState Tick()
        {
            _particle.SetActive(_isActive);
            return NodeState = BehaviourState.成功;
        }
    }
    
    [NodeLabel("直线移动")]
    public class Goto : BtActionNode
    {
        [LabelText("移动的秒速"),SerializeField,FoldoutGroup("@NodeName"),Range(0,20)]
        private float speed;
        [LabelText("被移动的"),SerializeField,FoldoutGroup("@NodeName")]
        private Transform move;
        [LabelText("目的地"),SerializeField,FoldoutGroup("@NodeName")]
        private Transform destination;
        [LabelText("停止范围"),SerializeField,FoldoutGroup("@NodeName")]
        private float failover;
        public override BehaviourState Tick()
        {
            if (!move||!destination)
            {
                ChangeFailState();
                return NodeState;
            }
            if (Vector3.Distance(move.position,destination.position) < failover)
            {
                return NodeState = BehaviourState.成功;
            }
            var v = (destination.position - move.position).normalized;
            move.position += v * speed * Time.deltaTime;
            return NodeState = BehaviourState.执行中;
        }
    }

    [NodeLabel("发现目标")]
    public class DiscoveryTarget : BtPrecondition
    {
        [LabelText("自己"),SerializeField,FoldoutGroup("@NodeName")]
        private Transform _self;
        [LabelText("目标"),SerializeField,FoldoutGroup("@NodeName")]
        private Transform _destination;
        [LabelText("发现距离"),SerializeField,FoldoutGroup("@NodeName")]
        private float _warnRange;
        
        public override BehaviourState Tick()
        {
            if (!_destination.gameObject.activeSelf)
            {
                ChangeFailState();
                return NodeState;
            }
            var distance = Vector3.Distance(_self.position, _destination.position);
            if (distance <= _warnRange)
            {
                ChildNode.Tick();
                return NodeState = BehaviourState.执行中;
            }
            else
            {
                ChangeFailState();
                return NodeState;
            }
        }
    }
    [NodeLabel("行为树事件系统")]
    public class BtEventSystem : BtPrecondition
    {
        public Func<bool> a1;
        
        public override BehaviourState Tick()
        {
            return ChildNode.Tick();
        }
        
        
    }

}