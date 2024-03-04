using System.Threading.Tasks;
using PlasticPipe.PlasticProtocol.Messages;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace JLBehaviourTree.Editor.View
{
    public class MovePoint : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<MovePoint,UxmlTraits>{}

        private EdgeControl edgeC;
        public MovePoint()
        {
            Image icon = new Image(){name = "MovePoint" };
            this.Add(icon);
            icon.transform.position-=new Vector3(10,10,0);
        }

        public void ToMove(EdgeControl edgeC)
        {
            this.edgeC = edgeC;
            MoveStep();
            
        }

        async Task MoveStep()
        {
            for (int i = 0; i < 100; i++)
            {
                Debug.Log("开始");
                Debug.Log($"{edgeC.controlPoints[1]}  {edgeC.controlPoints[2]}");
                this.transform.position = Vector2.Lerp(edgeC.controlPoints[1],
                    edgeC.controlPoints[2], i / 100f);
                await Task.Delay(10); 
            }
            Debug.Log("完成");
            this.parent.Remove(this);
        }
    }
}