using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BachelorThesis.src.BVH;
using System;
using System.Collections.Generic;
using System.Text;

namespace BachelorThesis.src.old
{
    public interface OLDIEntity
    {
        public Vector2 Position { get; set; }
        public Vector2 MassCenter { get; }
        public float Radius { get; }
        public float Mass { get; }
        public void Update(GameTime gameTime);
        public void Accelerate(Vector2 direction, float force);
        public void AccelerateTo(Vector2 position, float force);
        public bool CollidesWith(OLDIEntity e){
            return Math.Sqrt(Math.Pow((double)(Position.X) - (double)(e.Position.X), 2) + Math.Pow((double)(Position.Y) - (double)(e.Position.Y), 2)) <= (Radius + e.Radius);
        }
        public void Draw(SpriteBatch spriteBatch);
        public void Collide(OLDIEntity e);
        public void GenerateAxes();
        public ControllerBVH ParentController {get;set;}
        public OLDIEntity BranchAndBound(OLDWorldEntity eNew, OLDIEntity bestEntity, float bestCost, float inheritedCost, PriorityQueue<OLDIEntity, float> queue);
    }
}
