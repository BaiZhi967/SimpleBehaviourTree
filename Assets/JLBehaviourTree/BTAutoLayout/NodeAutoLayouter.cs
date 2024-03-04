/* by.烟雨迷离半世殇 
 * https://www.lfzxb.top/non-layered-tidy-trees-practise/
 */

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace JLBehaviourTree.BTAutoLayout
{
    
    public interface INodeForLayoutConvertor
    {
        /// <summary>
        /// 节点间的距离
        /// </summary>
        float SiblingDistance { get; }

        object PrimRootNode { get; }
        NodeAutoLayouter.TreeNode LayoutRootNode { get; }

        INodeForLayoutConvertor Init(object primRootNode);
        NodeAutoLayouter.TreeNode PrimNode2LayoutNode();
        void LayoutNode2PrimNode();
    }
    
    public class NodeAutoLayouter
    {
        [System.Flags]
        public enum CalculateMode
        {
            // 水平计算模式
            Horizontal = 1,

            // 垂直计算模式
            Vertical = Horizontal << 1,

            // 从上到下，从左到右
            Positive = Horizontal << 2,

            // 从下到上，从右到左
            Negative = Horizontal << 3
        }

        public class TreeNode
        {
            public float w, h; // ^{\normalfont Width and height.}^
            public float x, y, prelim, mod, shift, change;
            public TreeNode tl, tr; // ^{\normalfont Left and right thread.}^                        
            public TreeNode el, er; // ^{\normalfont Extreme left and right nodes.}^ 
            public float msel, mser; // ^{\normalfont Sum of modifiers at the extreme nodes.}^ 
            public List<TreeNode> children = new List<TreeNode>();
            public int childrenCount => children.Count; // ^{\normalfont Array of children and number of children.}^ 
            public CalculateMode CalculateMode;

            public TreeNode(float w, float h, float y,
                CalculateMode calculateMode = CalculateMode.Vertical | CalculateMode.Positive)
            {
                this.w = w;
                this.h = h;
                this.y = y;
                this.CalculateMode = calculateMode;
            }

            public void AddChild(TreeNode child)
            {
                this.children.Add(child);
            }

            public Vector2 GetPos()
            {
                var calculateResult = new Vector2(x, y);

                // 根节点位于原点，绕原点进行旋转(绕原点的旋转矩阵)，并对节点位置进行修正（因为宽高翻转）
                if (CalculateMode == (NodeAutoLayouter.CalculateMode.Horizontal | NodeAutoLayouter.CalculateMode.Negative))
                {
                    Vector2 temp = calculateResult;
                    temp.x = -calculateResult.y - h;
                    temp.y = calculateResult.x;
                    calculateResult = temp;
                }

                if (CalculateMode == (NodeAutoLayouter.CalculateMode.Horizontal | NodeAutoLayouter.CalculateMode.Positive))
                {
                    Vector2 temp = calculateResult;
                    temp.x = calculateResult.y;
                    temp.y = -calculateResult.x - w;
                    calculateResult = temp;
                }

                if (CalculateMode == (NodeAutoLayouter.CalculateMode.Vertical | NodeAutoLayouter.CalculateMode.Negative))
                {
                    calculateResult.y = -calculateResult.y - h;
                }

                return calculateResult;
            }
        }

        public static void Layout(INodeForLayoutConvertor nodeForLayoutConvertor)
        {
            if (nodeForLayoutConvertor.PrimNode2LayoutNode() == null)
            {
                return;
            }

            firstWalk(nodeForLayoutConvertor.LayoutRootNode);
            secondWalk(nodeForLayoutConvertor.LayoutRootNode, 0);
            nodeForLayoutConvertor.LayoutNode2PrimNode();
        }

        static void firstWalk(TreeNode t)
        {
            if (t.childrenCount == 0)
            {
                setExtremes(t);
                return;
            }

            firstWalk(t.children[0]);
            // ^{\normalfont Create siblings in contour minimal vertical coordinate and index list.}^
            IYL ih = updateIYL(bottom(t.children[0].el), 0, null);
            for (int i = 1; i < t.childrenCount; i++)
            {
                firstWalk(t.children[i]);
                //^{\normalfont Store lowest vertical coordinate while extreme nodes still point in current subtree.}^
                float minY = bottom(t.children[i].er);
                seperate(t, i, ih);
                ih = updateIYL(minY, i, ih);
            }

            positionRoot(t);
            setExtremes(t);
        }

        static void setExtremes(TreeNode t)
        {
            if (t.childrenCount == 0)
            {
                t.el = t;
                t.er = t;
                t.msel = t.mser = 0;
            }
            else
            {
                t.el = t.children[0].el;
                t.msel = t.children[0].msel;
                t.er = t.children[t.childrenCount - 1].er;
                t.mser = t.children[t.childrenCount - 1].mser;
            }
        }

        static void seperate(TreeNode t, int i, IYL ih)
        {
            // ^{\normalfont Right contour node of left siblings and its sum of modfiers.}^  
            TreeNode sr = t.children[i - 1];
            float mssr = sr.mod;
            // ^{\normalfont Left contour node of current subtree and its sum of modfiers.}^  
            TreeNode cl = t.children[i];
            float mscl = cl.mod;
            while (sr != null && cl != null)
            {
                if (bottom(sr) > ih.lowY) ih = ih.nxt;

                // ^{\normalfont How far to the left of the right side of sr is the left side of cl?}^  
                float dist = (mssr + sr.prelim + sr.w) - (mscl + cl.prelim);
                if (dist > 0)
                {
                    mscl += dist;
                    moveSubtree(t, i, ih.index, dist);
                }

                float sy = bottom(sr), cy = bottom(cl);
                // ^{\normalfont Advance highest node(s) and sum(s) of modifiers}^  
                if (sy <= cy)
                {
                    sr = nextRightContour(sr);
                    if (sr != null) mssr += sr.mod;
                }

                if (sy >= cy)
                {
                    cl = nextLeftContour(cl);
                    if (cl != null) mscl += cl.mod;
                }
            }

            // ^{\normalfont Set threads and update extreme nodes.}^  
            // ^{\normalfont In the first case, the current subtree must be taller than the left siblings.}^  
            if (sr == null && cl != null) setLeftThread(t, i, cl, mscl);
            // ^{\normalfont In this case, the left siblings must be taller than the current subtree.}^  
            else if (sr != null && cl == null) setRightThread(t, i, sr, mssr);
        }

        static void moveSubtree(TreeNode t, int i, int si, float dist)
        {
            // ^{\normalfont Move subtree by changing mod.}^  
            t.children[i].mod += dist;
            t.children[i].msel += dist;
            t.children[i].mser += dist;
            distributeExtra(t, i, si, dist);
        }

        static TreeNode nextLeftContour(TreeNode t) { return t.childrenCount == 0 ? t.tl : t.children[0]; }
        static TreeNode nextRightContour(TreeNode t) { return t.childrenCount == 0 ? t.tr : t.children[t.childrenCount - 1]; }
        static float bottom(TreeNode t) { return t.y + t.h; }

        static void setLeftThread(TreeNode t, int i, TreeNode cl, float modsumcl)
        {
            TreeNode li = t.children[0].el;
            li.tl = cl;
            // ^{\normalfont Change mod so that the sum of modifier after following thread is correct.}^  
            float diff = (modsumcl - cl.mod) - t.children[0].msel;
            li.mod += diff;
            // ^{\normalfont Change preliminary x coordinate so that the node does not move.}^  
            li.prelim -= diff;
            // ^{\normalfont Update extreme node and its sum of modifiers.}^  
            t.children[0].el = t.children[i].el;
            t.children[0].msel = t.children[i].msel;
        }

        // ^{\normalfont Symmetrical to setLeftThread.}^  
        static void setRightThread(TreeNode t, int i, TreeNode sr, float modsumsr)
        {
            TreeNode ri = t.children[i].er;
            ri.tr = sr;
            float diff = (modsumsr - sr.mod) - t.children[i].mser;
            ri.mod += diff;
            ri.prelim -= diff;
            t.children[i].er = t.children[i - 1].er;
            t.children[i].mser = t.children[i - 1].mser;
        }

        static void positionRoot(TreeNode t)
        {
            // ^{\normalfont Position root between children, taking into account their mod.}^  
            t.prelim = (t.children[0].prelim + t.children[0].mod + t.children[t.childrenCount - 1].mod +
                        t.children[t.childrenCount - 1].prelim + t.children[t.childrenCount - 1].w) / 2 - t.w / 2;
        }

        static void secondWalk(TreeNode t, float modsum)
        {
            modsum += t.mod;
            // ^{\normalfont Set absolute (non-relative) horizontal coordinate.}^  
            t.x = t.prelim + modsum;
            addChildSpacing(t);
            for (int i = 0; i < t.childrenCount; i++) secondWalk(t.children[i], modsum);
        }

        static void distributeExtra(TreeNode t, int i, int si, float dist)
        {
            // ^{\normalfont Are there intermediate children?}^
            if (si != i - 1)
            {
                float nr = i - si;
                t.children[si + 1].shift += dist / nr;
                t.children[i].shift -= dist / nr;
                t.children[i].change -= dist - dist / nr;
            }
        }

        // ^{\normalfont Process change and shift to add intermediate spacing to mod.}^  
        static void addChildSpacing(TreeNode t)
        {
            float d = 0, modsumdelta = 0;
            for (int i = 0; i < t.childrenCount; i++)
            {
                d += t.children[i].shift;
                modsumdelta += d + t.children[i].change;
                t.children[i].mod += modsumdelta;
            }
        }

        // ^{\normalfont A linked list of the indexes of left siblings and their lowest vertical coordinate.}^  
        class IYL
        {
            public float lowY;
            public int index;
            public IYL nxt;

            public IYL(float lowY, int index, IYL nxt)
            {
                this.lowY = lowY;
                this.index = index;
                this.nxt = nxt;
            }
        }

        static IYL updateIYL(float minY, int i, IYL ih)
        {
            // ^{\normalfont Remove siblings that are hidden by the new subtree.}^  
            while (ih != null && minY >= ih.lowY) ih = ih.nxt;
            // ^{\normalfont Prepend the new subtree.}^  
            return new IYL(minY, i, ih);
        }
    }
}
