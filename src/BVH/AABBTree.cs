using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BachelorThesis.src.bounding_areas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace BachelorThesis.src.BVH
{
    public class AABBTree: IController
    {
        public Vector2 Position { get { return root.Position; } }
        public Vector2 MassCenter { get { return root.MassCenter; } }
        public float Radius { get { return root.Radius; } }
        public AABBNode root;
        private Stack<AABBNode> freeNodes = new();
        private HashSet<WorldEntity> worldEntities = new();
        public List<(WorldEntity, WorldEntity)> CollissionPairs = new();
        public int VERSION_USED {get; set;} = 0; 

        public AABBTree()
        {
        }

        public void Add(WorldEntity we)
        {
            AABBNode leaf = AllocateLeafNode(we);
            if (root == null)
            {
                root = leaf;
                return;
            }
            // Stage 1: find the best sibling for the new leaf
            AABBNode bestSibling = FindBestSibling(leaf);

            // Stage 2: create a new parent

            AABBNode oldParent = bestSibling.Parent;
            AABBNode newParent = AllocateNode();
            newParent.Parent = oldParent;
            //newParent.BoundingCircle = Union(box, tree.nodes[sibling].box); done in stage 3
            if (oldParent != null)
            {
                if (oldParent.children[0] == bestSibling)
                    oldParent.children[0] = newParent;
                else
                    oldParent.children[1] = newParent;
            }
            else
            {
                root = newParent;
            }
            newParent.Add(bestSibling);
            newParent.Add(leaf);
            // Stage 3: walk back up the tree refitting AABBs
            AABBNode parent = newParent;
            do
            {
                parent.RefitBoundingBox();
                parent = parent.Parent;
            } while (parent != null);
        }

        //for root: parent = null, newEntities is worldEntities
        public AABBNode CreateTreeTopDown_Median(AABBNode parent, List<WorldEntity> newEntities)
        {
            //step 0: HANDLE EDGE-CASES
            if (parent == null)
                UnravelTree();

            if (newEntities.Count == 0)
                return null;

            if (newEntities.Count == 1)
            {
                return AllocateLeafNode(newEntities[0]);
            }
            //TODO: if node==root, remove current tree if it exists and add worldEntities to newEntities


            //step 1: DECIDE WHAT AXIS TO SPLIT
            AxisAlignedBoundingBox AABB = AxisAlignedBoundingBox.SurroundingAABB(newEntities);
            int axis = AxisAlignedBoundingBox.MajorAxis(AABB);
            BoundingAreaFactory.AABBs.Push(AABB);

            //step 2: SPLIT ON CHOSEN AXIS
            if (axis == 0)
                newEntities.Sort((we1, we2) => we1.Position.X.CompareTo(we2.Position.X));
            else
                newEntities.Sort((we1, we2) => we1.Position.Y.CompareTo(we2.Position.Y));
            AABBNode node = AllocateNode();
            node.Add(CreateTreeTopDown_Median(node, newEntities.GetRange(0, newEntities.Count / 2)));
            node.Add(CreateTreeTopDown_Median(node, newEntities.GetRange(newEntities.Count / 2, newEntities.Count / 2 + newEntities.Count % 2)));
            node.RefitBoundingBox();
            return node;
        }
        // newEntities order will be affected 
        public AABBNode CreateTreeTopDown_SAH(AABBNode parent, List<WorldEntity> newEntities)
        {
            //step 0: HANDLE EDGE-CASES
            if (parent == null)
                UnravelTree();

            if (newEntities.Count == 0)
                return null;

            if (newEntities.Count == 1)
            {
                return AllocateLeafNode(newEntities[0]);
            }
            //TODO: if node==root, remove current tree if it exists and add worldEntities to newEntities
            //step 0: SETUP
            float minCost = float.MaxValue;
            int minCostSplitIndex = 0;
            int minCostAxis = 0;
            //step 1: DECIDE WHAT SPLIT TO USE
            //ADD: sort along x-axis
            for (int axis = 0; axis < 2; axis++)//x=0, y=1
            {
                if(axis == 0)
                    newEntities.Sort((a, b) => a.Position.X.CompareTo(b.Position.X));
                else
                    newEntities.Sort((a, b) => a.Position.Y.CompareTo(b.Position.Y));
                for (int i = 0; i < newEntities.Count-1; i++)
                {
                    List<WorldEntity> entities = newEntities.GetRange(0, i+1);
                    AxisAlignedBoundingBox AABB1 = AxisAlignedBoundingBox.SurroundingAABB(entities);
                    float cost1 = AABB1.Area;
                    entities = newEntities.GetRange(i+1, newEntities.Count-(i+1));
                    AxisAlignedBoundingBox AABB2 = AxisAlignedBoundingBox.SurroundingAABB(entities);
                    float cost2 = AABB2.Area;
                    float total = cost1 + cost2;
                    if (total < minCost)
                    {
                        minCostSplitIndex = i;
                        minCost = total;
                        minCostAxis = axis;
                    }
                    BoundingAreaFactory.AABBs.Push(AABB1);
                    BoundingAreaFactory.AABBs.Push(AABB2);
                }
            }

            //step 2: SPLIT ON CHOSEN AXIS
            if (minCostAxis == 0)
                newEntities.Sort((we1, we2) => we1.Position.X.CompareTo(we2.Position.X));
            AABBNode node = AllocateNode();
            node.Add(CreateTreeTopDown_SAH(node, newEntities.GetRange(0, minCostSplitIndex+1)));
            node.Add(CreateTreeTopDown_SAH(node, newEntities.GetRange(minCostSplitIndex+1, newEntities.Count-minCostSplitIndex-1)));
            node.RefitBoundingBox();
            return node;
        }

        private AABBNode AllocateLeafNode(WorldEntity we)
        {
            AABBNode leafNode = AllocateNode();
            leafNode.WorldEntity = we;
            worldEntities.Add(we);
            return leafNode;
        }

        private AABBNode AllocateNode()
        {
            if (freeNodes.Count == 0)
                return new AABBNode();
            else
            {
                AABBNode freeNode = freeNodes.Pop();
                freeNode.Reset();
                return freeNode;
            }
        }

        public AABBNode FindBestSibling(AABBNode leafNew)
        {
            AABBNode bestSibling = root;
            AxisAlignedBoundingBox combinedBest = AxisAlignedBoundingBox.SurroundingAABB(root.AABB,leafNew.AABB);
            float bestCost = combinedBest.Area;
            BoundingAreaFactory.AABBs.Push(combinedBest);
            PriorityQueue<AABBNode, float> queue = new();
            queue.Enqueue(root, 0);

            while (queue.Count > 0)
            {
                queue.TryDequeue(
                        out AABBNode currentNode,
                        out float inheritedCost
                );

                if (inheritedCost >= bestCost)
                    return bestSibling;
                AxisAlignedBoundingBox combined = AxisAlignedBoundingBox.SurroundingAABB(currentNode.AABB, leafNew.AABB);
                float combinedArea = combined.Area;
                BoundingAreaFactory.AABBs.Push(combined);
                float currentCost = combinedArea + inheritedCost;
                if (currentCost < bestCost)
                {
                    bestSibling = currentNode;
                    bestCost = currentCost;
                }
                inheritedCost += combinedArea - currentNode.AABB.Area;
                float cLow = leafNew.AABB.Area + inheritedCost;
                if (cLow < bestCost)
                {
                    if (currentNode.children[0] != null)
                        queue.Enqueue(currentNode.children[0], inheritedCost);
                    if (currentNode.children[1] != null)
                        queue.Enqueue(currentNode.children[1], inheritedCost);
                }
            }
            return bestSibling;
        }

        public void Draw(SpriteBatch sb)
        {
            root.Draw(sb);
        }
        public virtual void Update(GameTime gameTime)
        {
            //RebuildTree();
            ApplyInternalGravityN();
            //ApplyInternalGravityN();
            //ApplyInternalGravityN2();
            root.GetInternalCollissions(CollissionPairs);
            ResolveInternalCollissions();
            root.Update(gameTime);

            root = CreateTreeTopDown_Median(null, worldEntities.ToList());
        }

        private void ApplyInternalGravityN()
        {
            Vector2 distanceFromController;
            foreach (WorldEntity entity in worldEntities)
            {
                distanceFromController = root.MassCenter - entity.Position;
                if (distanceFromController.Length() > entity.Radius)
                    entity.Accelerate(Vector2.Normalize(root.MassCenter - entity.Position), Game1.GRAVITY * (root.Mass - entity.Mass) * entity.Mass / (float)Math.Pow((distanceFromController.Length()), 1)); //2d gravity r is raised to 1
                //entity.Accelerate(Vector2.Normalize(Position - entity.Position), (float)Math.Pow(((distanceFromController.Length() - entity.Radius) / AverageDistance()) / 2 * entity.Mass, 2));
            }
        }
        private void ApplyInternalGravityN2()
        {
            foreach (WorldEntity we1 in worldEntities)
                foreach (WorldEntity we2 in worldEntities)
                    if (we1 != we2)
                        we1.AccelerateTo(we2.Position, Game1.GRAVITY * we1.Mass * we2.Mass / (float)Math.Pow(((we1.Position - we2.Position).Length()), 1));
        }
        public void BuildTree(){
            switch(VERSION_USED){
                case 0: root = CreateTreeTopDown_Median(null, worldEntities.ToList()); break;
                case 1: root = CreateTreeTopDown_SAH(null, worldEntities.ToList()); break;
                case 2: BuildTree_Insertion();break;
                default: throw new Exception("tree build overflow");
            }        
        }

        public void GetInternalCollissions(){
            root.GetInternalCollissions(CollissionPairs);
        }

        public void ResolveInternalCollissions()
        {
            foreach ((WorldEntity, WorldEntity) pair in CollissionPairs)
            {
                    pair.Item1.Collide(pair.Item2);
                    pair.Item2.Collide(pair.Item1);
            }
            CollissionPairs.Clear();
        }

        private void BuildTree_Insertion()
        {
            UnravelTree();
            foreach (WorldEntity we in worldEntities)
                Add(we);
        }
        private void UnravelTree()
        {
            if (root != null)
            {
                Stack<AABBNode> nodesToRemove = new();
                nodesToRemove.Push(root);
                while (nodesToRemove.Count > 0)
                {
                    AABBNode currentNode = nodesToRemove.Pop();
                    foreach (AABBNode child in currentNode.children)
                    {
                        if (child != null)
                        {
                            nodesToRemove.Push(child);
                        }
                    }
                    freeNodes.Push(currentNode);
                }
                root = null;
            }
        }

        public void UpdateDeterministic(/*PerformanceMeasurer measurer*/)
        {
            ApplyInternalGravityN();
            root.UpdateDeterministic();
            //RebuildTree();
            //measurer.Tick(); //turn to state 0: build
            //BuildTree();
            //measurer.Tick(); //turn to state 1: CD
            //GetInternalCollissions();
            //measurer.Tick(); //turn to state 2: CH
            //ResolveInternalCollissions();
            //measurer.Tick(); //turn to state 3: other
            //ApplyInternalGravityN();
            //ApplyInternalGravityN();
            //ApplyInternalGravityN2();
        }

        public void SetEntities(List<WorldEntity> entities)
        {
            worldEntities.Clear();
            foreach(WorldEntity we in entities)
                worldEntities.Add(we);
            BuildTree();
        }
    }

}