using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace BachelorThesis.src
{
    public abstract class Movable
    {
        public virtual float Mass { get; set; }
        public virtual Vector2 Position { get; set; }
        protected Vector2 position;
        public virtual float Rotation { get; set; }
        protected float rotation;
        protected Vector2 velocity;
        public virtual Vector2 Velocity { get { return velocity; } set { velocity = value; } }
        public virtual float Friction { get; set; } // percent, where 0.1f = 10% friction
        public Vector2 TotalExteriorForce;

        public Movable(Vector2 position, float rotation = 0, float mass = 1, float thrust = 1, float friction = 0.15f)
        {
            this.position = position;
            this.rotation = rotation;
            Mass = mass;
            Friction = friction;
            Velocity = Vector2.Zero;
        }

        public virtual void Update(GameTime gameTime) //OBS Ska vara en funktion i thruster
        {
            Vector2 FrictionForce = (Velocity * Mass + TotalExteriorForce) * Friction * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            Velocity = Velocity + (TotalExteriorForce - FrictionForce) / Mass * (float)gameTime.ElapsedGameTime.TotalSeconds*60;
            Position += Velocity * (float)gameTime.ElapsedGameTime.TotalSeconds * 60;
            TotalExteriorForce = Vector2.Zero;
        }

        public void AccelerateTo(Vector2 position, float thrust){
            Accelerate(position-Position, thrust);
        }

        /**
         * Recieved a directional vector and accelerates with a certain thrust
         */
        public void Accelerate(Vector2 directionalVector, float thrust)
        {
            Vector2 direction = new Vector2(directionalVector.X, directionalVector.Y);
            direction.Normalize();
            TotalExteriorForce += direction * thrust;
        }

        public Vector2 VelocityAlongVector(Vector2 directionalVector)
        {
            directionalVector = new Vector2(directionalVector.X, directionalVector.Y);//unnecessary?
            directionalVector.Normalize();
            return Vector2.Dot(Velocity, directionalVector) / Vector2.Dot(directionalVector, directionalVector) * directionalVector;
        }

        public virtual void RotateTo(Vector2 position)
        {
            RotateTo(position, Position);
        }

        public void RotateTo(Vector2 p, Vector2 p0)
        {
            Vector2 position = p - p0;
            if (position.X >= 0)
                Rotation = (float)Math.Atan(position.Y / position.X);
            else
                Rotation = (float)Math.Atan(position.Y / position.X) - MathHelper.ToRadians(180);
        }

        public void UpdateDeterministic()
        {
            Vector2 FrictionForce = (Velocity * Mass + TotalExteriorForce) * Friction * (float)Game1.TIME_STEP * 60;
            Velocity = Velocity + (TotalExteriorForce - FrictionForce) / Mass * (float)Game1.TIME_STEP*60;
            Position += Velocity * (float)Game1.TIME_STEP * 60;
            TotalExteriorForce = Vector2.Zero;
        }
    }
}
