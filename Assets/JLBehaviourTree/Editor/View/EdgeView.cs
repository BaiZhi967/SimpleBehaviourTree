using Cysharp.Threading.Tasks;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.Editor.View
{
    public enum MovePointState
    {
        停止的,运行的,暂停的
    }

    public class EdgeView : Edge
    {
        private VisualElement[] _movePoints;

        private MovePointState _isMoveState;

        private int _stepIndex;

        private int _pointNumber = 4;
        
        public EdgeView() : base()
        {
            _movePoints = new VisualElement[_pointNumber];
        }

        public void OnStartMovePoints()
        {
            if (_isMoveState == MovePointState.运行的)return;
            if (_isMoveState == MovePointState.停止的)
            {
                for (int i = 0; i < _pointNumber; i++)
                {
                    _movePoints[i] = new MovePoint();
                    Add(_movePoints[i]);
                }
                _stepIndex = 0;    
            }
            _isMoveState = MovePointState.运行的;
            MovePoints();
        }

        async void MovePoints()
        {
            while (_isMoveState == MovePointState.运行的)
            {
                _stepIndex = _stepIndex % 100;
                for (int i = 0; i < _pointNumber; i++)
                {
                    float t = (_stepIndex / 100f) + (float)i / _pointNumber;
                    t = t > 1 ? t - 1 : t;
                    _movePoints[i].transform.position = Vector2.Lerp(edgeControl.controlPoints[1],
                        edgeControl.controlPoints[2], t);
                }

                _stepIndex++;
                await UniTask.Delay(10);
            }
            
        }

        public void OnSuspendMovePoints() => _isMoveState = MovePointState.暂停的;

        public void OnStopMovePoints()
        {
            if(_isMoveState == MovePointState.停止的)return;
            _isMoveState = MovePointState.停止的;
            for (int i = _movePoints.Length-1; i >= 0; i--)
            {
                if (_movePoints[i] != null)
                {
                    this.Remove(_movePoints[i]);
                }
                
            }
        }
    }
}


