using UnityEngine;
using UnityEngine.UIElements;


namespace JLBehaviourTree.Editor.View
{
    public class SplitView : TwoPaneSplitView
    {
        #region VisualElement内容
        public InspectorView InspectorView;
        public Label InspectorTitle;
        public TreeView TreeView;
        #endregion

        
        public new class UxmlFactory : UxmlFactory<SplitView,UxmlTraits>{}
        public SplitView()
        {
            Init();
        }

        private void Init(){}
    }
}