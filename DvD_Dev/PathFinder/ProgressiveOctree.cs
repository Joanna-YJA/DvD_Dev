using System;
using System.Collections;
using System.Numerics;

namespace DvD_Dev
{
    class ProgressiveOctree : Octree
    {
        public ProgressiveOctree(float _size, Vector3 _corner, int _maxLevel) : base(_size, _corner, _maxLevel)
        {
            root = new ProgressiveOctreeNode(0, new int[] { 0, 0, 0 }, null, this);
        }
    }
    class ProgressiveOctreeNode : OctreeNode
    {
        public ProgressiveOctreeNode(int _level, int[] _index, ProgressiveOctreeNode _parent, ProgressiveOctree _tree) : base(_level, _index, _parent, _tree) { }

        public override void CreateChildren()
        {
            if (children == null)
            {
                children = new ProgressiveOctreeNode[2, 2, 2];
                for (int xi = 0; xi < 2; xi++)
                    for (int yi = 0; yi < 2; yi++)
                        for (int zi = 0; zi < 2; zi++)
                        {
                            int[] newIndex = { index[0] * 2 + xi, index[1] * 2 + yi, index[2] * 2 + zi };
                           // System.Diagnostics.Debug.WriteLine("New Index is " + String.Join(", ", newIndex));
                            children[xi, yi, zi] = new ProgressiveOctreeNode(level + 1, newIndex, this, (ProgressiveOctree)tree);

                            //if (children[xi, yi, zi].level <= this.level)
                            //    throw new Exception("Children is not one level greater than parent");
                            //if (Math.Abs(children[xi, yi, zi].size - this.size) < 0.0000001)
                            //    throw new Exception("Children does not have a smaller size than parent, children.level " 
                            //        + children[xi, yi, zi].level + " parent.level " + this.level +
                            //        " children.size = " + children[xi, yi, zi].tree.size + " / " + (1 << children[xi, yi, zi].level) + " = " + children[xi, yi, zi].size + 
                            //        " parent.size " + this.tree.size + " / " + (1 << this.level) + " = " + this.size);
                        }
                if (level != 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        OctreeNode found = tree.Find(new int[] { index[0] + Octree.dir[i, 0], index[1] + Octree.dir[i, 1], index[2] + Octree.dir[i, 2] }, level);
                        if (found != null && found.level < level)
                        {
                            found.CreateChildren();
                        }
                    }
                }

            }
            //else System.Diagnostics.Debug.WriteLine("Children is not null " + string.Join(",", children));
        }
    }
}