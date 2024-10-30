using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using BachelorThesis.src;
using BachelorThesis.src.bounding_areas;

namespace BachelorThesis.src.BVH
{
    public class AABBNode : IEntity
    {
        public AABBNode Parent { get; set; }
        public AABBNode[] children = new AABBNode[2];
        public float Radius { get { return radius; } protected set { radius = value; } }
        protected float radius;
        public Vector2 Position
        {
            get { return position; }
            set
            {
                Vector2 posChange = value - Position;
                foreach (AABBNode c in children)
                    if (c != null)
                        c.Position += posChange;
                position = value;
            }
        }
        protected Vector2 position;
        public float Mass { get; set; }
        public int Count { get; set; }
        public Vector2 MassCenter { get; private set; }
        public AxisAlignedBoundingBox AABB { get; private set; }
        public WorldEntity WorldEntity
        {//=!null implies leaf
            get { return worldEntity; }
            set
            {
                worldEntity = value;
                AABB = AxisAlignedBoundingBox.SurroundingAABB(worldEntity.OBB);// new AxisAlignedBoundingBox(worldEntity.OBB);
                position = worldEntity.Position;
                radius = WorldEntity.Radius;
                Count = 1;
                Mass = worldEntity.Mass;
                MassCenter = worldEntity.MassCenter;
            }
        }
        private WorldEntity worldEntity;
        public AABBNode()
        {
        }
        #region node-functionality
        public void Add(AABBNode node)
        {
            if (children.Count(x => x != null) < 2)
            {
                if (children[0] == null)
                    children[0] = node;
                else if (children[1] == null)
                    children[1] = node;
                node.Parent = this;
                Mass += node.Mass;
                Count += node.Count;
            }
        }

        public void RefitBoundingBox()
        {
            if (AABB != null)
                BoundingAreaFactory.AABBs.Push(AABB);
            if (children.Count(x => x != null) == 1)
            {
                if (children[0] != null)
                {
                    AABB = BoundingAreaFactory.CreateAABB(children[0].AABB.UL, (int)Math.Round(children[0].AABB.Width), (int)Math.Round(children[0].AABB.Height)); //BoundingCircle = new CollidableCircle(children[0].BoundingCircle.Position, children[0].BoundingCircle.Radius);
                    Count = children[0].Count;
                }
                else
                {
                    AABB = BoundingAreaFactory.CreateAABB(children[1].AABB.UL, (int)Math.Round(children[1].AABB.Width), (int)Math.Round(children[1].AABB.Height)); //BoundingCircle = new CollidableCircle(children[0].BoundingCircle.Position, children[0].BoundingCircle.Radius);
                    Count = children[1].Count;
                }
            }
            else if (children.Count(x => x != null) == 2)
            {
                AABB = AxisAlignedBoundingBox.SurroundingAABB(children[0].AABB,children[1].AABB);
                Count = children[0].Count + children[1].Count;
            }
            position = AABB.Position;
            radius = AABB.Radius;

            UpdateMassCenter();
        }
        public void UpdateMassCenter()
        {
            MassCenter = Vector2.Zero;
            Mass = 0;
            foreach (AABBNode child in children)
            {
                if (child != null)
                {
                    Mass += child.Mass;
                    MassCenter += child.MassCenter * child.Mass;
                }
            }
            MassCenter /= Mass;
        }

        public void Reset()
        {
            if(AABB != null)
                BoundingAreaFactory.AABBs.Push(AABB);
            AABB = null;
            worldEntity = null;
            children[0] = null;
            children[1] = null;
            Parent = null;
            Count = 0;
            Radius = 0;
            Mass = 0;
            position = Vector2.Zero;
            MassCenter = Vector2.Zero;
        }
        #endregion
        #region update-logic
        public void Draw(SpriteBatch sb)
        {
            if (WorldEntity != null)
                WorldEntity.Draw(sb);
            else{
                float scale = 3+((int)Mass/14);
                Vector2 origin = Vector2.Zero; //new Vector2(-Game1.font.Texture.Width/2, -Game1.font.Texture.Height/2);
                sb.DrawString(Game1.font, ((int)Mass).ToString(), Position + new Vector2(AABB.Width / 2, AABB.Height / 2), Color.Red, 0, origin, scale, SpriteEffects.None, 0);
                foreach (AABBNode c in children)
                    if (c != null)
                        c.Draw(sb);
            }
        }
        public void Update(GameTime gameTime)
        {
            if (WorldEntity != null)
                WorldEntity.Update(gameTime);
            else
                foreach (AABBNode child in children)
                    if (child != null)
                        child.Update(gameTime);
        }

        public void GetInternalCollissions(List<(WorldEntity, WorldEntity)> collissions)
        {
            if (children.Count(x => x != null) == 2)
                children[0].Collide(children[1], collissions);
            if (children[0] != null)
                children[0].GetInternalCollissions(collissions);
            if (children[1] != null)
                children[1].GetInternalCollissions(collissions);
        }

        public void Collide(AABBNode node, List<(WorldEntity, WorldEntity)> collissions)
        {
            if (AABB.CollidesWith(node.AABB))
            {
                if (WorldEntity == null)
                {
                    foreach (AABBNode child in children)
                        if (child != null)
                            child.Collide(node, collissions);
                }
                else if (node.WorldEntity == null)
                {
                    foreach (AABBNode childOther in node.children)
                        if (childOther != null)
                            Collide(childOther, collissions);
                }
                else
                {
                    WorldEntity.GenerateAxes();
                    node.WorldEntity.GenerateAxes();
                    if(WorldEntity.CollidesWith(node.WorldEntity))
                        collissions.Add((this.WorldEntity, node.WorldEntity));
                }
            }
        }
        public void ApplyInternalGravityNLOGN() //does not scale well... poor tree due to incremental?
        {
            Vector2 distanceFromController;
            if(children[0] != null){
                if(children[1] != null){
                    children[0].ApplyGravityTo(children[1].MassCenter, Game1.GRAVITY*children[1].Mass);
                    children[1].ApplyGravityTo(children[0].MassCenter, Game1.GRAVITY*children[0].Mass);
                }
            }
            if(children[0] != null)
                children[0].ApplyInternalGravityNLOGN();
            if(children[1] != null)
                children[1].ApplyInternalGravityNLOGN();
            /*foreach (BoundingCircleNode child in children)
            {
                if (child != null)
                {                     
                    float distanceFromController = (MassCenter - child.MassCenter).Length();
                    if (distanceFromController > 1)//entity.Radius)
                        child.AccelerateTo(MassCenter, Game1.GRAVITY * (Mass - child.Mass) / (float)Math.Pow((distanceFromController), 1)); //2d gravity r is raised to 1

                    child.ApplyInternalGravityNLOGN();
                }
            }*/
        }

        private void ApplyGravityTo(Vector2 massCenter, float forceBeforeDistance)
        {
            if (WorldEntity != null)
            {
                float distanceFromController = (massCenter - WorldEntity.MassCenter).Length();
                if (distanceFromController > 1)
                    WorldEntity.AccelerateTo(massCenter, forceBeforeDistance / distanceFromController);
            }
            else
                foreach (AABBNode node in children)
                    if (node != null)
                        node.ApplyGravityTo(massCenter, forceBeforeDistance);
        }

        public void AccelerateTo(Vector2 position, float force)
        {
            if (WorldEntity != null)
                WorldEntity.AccelerateTo(position, force);
            else
                foreach (AABBNode node in children)
                    if (node != null)
                        node.AccelerateTo(position, force);
        }

        public void UpdateDeterministic()
        {
            if (WorldEntity != null)
                WorldEntity.UpdateDeterministic();
            else
                foreach (AABBNode child in children)
                    if (child != null)
                        child.UpdateDeterministic();
        }
        #endregion
    }
}