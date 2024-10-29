using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BachelorThesis.src.bounding_areas;
using BachelorThesis.src.BVH;
using System;
using System.Collections.Generic;
using System.Text;

namespace BachelorThesis.src.BVH
{
    public interface IEntity
    {
        public Vector2 Position { get; set; }
        public Vector2 MassCenter { get; }
        public float Radius { get; }
        public float Mass { get; }
        public void Draw(SpriteBatch spriteBatch);
        public void Update(GameTime gameTime);
        void AccelerateTo(Vector2 position, float force);
        /*
        public void Accelerate(Vector2 direction, float force);
        public void GenerateAxes();
        */
    }
}
