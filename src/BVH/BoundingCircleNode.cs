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
    public class BoundingCircleNode : IEntity
    {
        public BoundingCircleNode Parent { get; set; }
        public BoundingCircleNode[] children = new BoundingCircleNode[2];
        public float Radius { get { return radius; } protected set { radius = value; } }
        protected float radius;
        public Vector2 Position
        {
            get { return position; }
            set
            {
                Vector2 posChange = value - Position;
                foreach (BoundingCircleNode c in children)
                    if (c != null)
                        c.Position += posChange;
                position = value;
            }
        }
        protected Vector2 position;
        public float Mass { get; set; }
        public int Count { get; set; }
        public Vector2 MassCenter { get; private set; }
        public BoundingCircle BoundingCircle { get; private set; }
        public WorldEntity WorldEntity
        {//=!null implies leaf
            get { return worldEntity; }
            set
            {
                worldEntity = value;
                BoundingCircle = worldEntity.BoundingCircle;
                position = worldEntity.Position;
                radius = WorldEntity.Radius;
                Count = 1;
                Mass = worldEntity.Mass;
                MassCenter = worldEntity.MassCenter;
            }
        }
        private WorldEntity worldEntity;
        public BoundingCircleNode()
        {
        }
        #region node-functionality
        public void Add(BoundingCircleNode node)
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

        public void RefitBoundingCircle()
        {
            if (children.Count(x => x != null) == 1)
            {
                if (children[0] != null)
                {
                    BoundingCircle = children[0].BoundingCircle; //BoundingCircle = new CollidableCircle(children[0].BoundingCircle.Position, children[0].BoundingCircle.Radius);
                    Count = children[0].Count;
                }
                else
                {
                    BoundingCircle = children[1].BoundingCircle;
                    Count = children[1].Count;
                }
            }
            else if (children.Count(x => x != null) == 2)
            {
                BoundingCircle = BoundingCircle.SurroundingBC(children[0].BoundingCircle,children[1].BoundingCircle);
                Count = children[0].Count + children[1].Count;
            }
            position = BoundingCircle.Position;
            radius = BoundingCircle.Radius;

            UpdateMassCenter();
        }
        public void UpdateMassCenter()
        {
            MassCenter = Vector2.Zero;
            Mass = 0;
            foreach (BoundingCircleNode child in children)
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
            if(worldEntity == null)
                BoundingAreaFactory.circles.Push(BoundingCircle);
            BoundingCircle = null;
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
                //float scale = 1+((int)Mass/7);
                //Vector2 origin = Vector2.Zero; //new Vector2(-Game1.font.Texture.Width/2, -Game1.font.Texture.Height/2);
                //sb.DrawString(Game1.font, ((int)Mass).ToString(), MassCenter, Color.Red, 0, origin, scale, SpriteEffects.None, 0);
                foreach (BoundingCircleNode c in children)
                    if (c != null)
                        c.Draw(sb);
            }
        }
        public void Update(GameTime gameTime)
        {
            if (WorldEntity != null)
                WorldEntity.Update(gameTime);
            else
                foreach (BoundingCircleNode child in children)
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

        public void Collide(BoundingCircleNode node, List<(WorldEntity, WorldEntity)> collissions)
        {
            if (BoundingCircle.CollidesWith(node.BoundingCircle))
            {
                if (WorldEntity == null)
                {
                    foreach (BoundingCircleNode child in children)
                        if (child != null)
                            child.Collide(node, collissions);
                }
                else if (node.WorldEntity == null)
                {
                    foreach (BoundingCircleNode childOther in node.children)
                        if (childOther != null)
                            Collide(childOther, collissions);
                }
                else
                {
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
                foreach (BoundingCircleNode node in children)
                    if (node != null)
                        node.ApplyGravityTo(massCenter, forceBeforeDistance);
        }

        public void AccelerateTo(Vector2 position, float force)
        {
            if (WorldEntity != null)
                WorldEntity.AccelerateTo(position, force);
            else
                foreach (BoundingCircleNode node in children)
                    if (node != null)
                        node.AccelerateTo(position, force);
        }
        #endregion
    }
}