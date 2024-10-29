using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BachelorThesis.src.bounding_areas
{
    //OBS: Old heritage, god knows how this works
    public class OrientedBoundingBox
    {
        private Vector2 UL { get; set; }
        private Vector2 DL { get; set; }
        private Vector2 DR { get; set; }
        private Vector2 UR { get; set; }
        public float Width { get { return (int)(Math.Round((UR - UL).Length())); } }
        public float Height { get { return (int)(Math.Round((UR - DR).Length())); } }
        public Vector2 AbsolutePosition { get { return (UL + DR) / 2; } }
        public (float, float) MaxXY{get;set;}
        public (float, float) MinXY{get;set;}
        private Vector2 origin;
        public Vector2 Origin
        {
            set
            {
                origin = value;
                UpdateRotation();
            }
            get
            {
                return origin;
            }
        }
        private Vector2 position;
        public Vector2 Position
        {
            set
            {
                Vector2 change = value - position;
                UL += change;
                DL += change;
                DR += change;
                UR += change;
                position = value;
                MaxXY = ((float)Math.Max(Math.Max(UL.X, UR.X), Math.Max(DL.X, DR.X)),(float)Math.Max(Math.Max(UL.Y, UR.Y), Math.Max(DL.Y, DR.Y)));
                MinXY = ((float)Math.Min(Math.Min(UL.X, UR.X), Math.Min(DL.X, DR.X)),(float)Math.Min(Math.Min(UL.Y, UR.Y), Math.Min(DL.Y, DR.Y)));
            }
            get
            {
                return position;
            }
        }
        private float rotation = 0;
        public float Rotation
        {
            set
            {
                rotation = value;
                UpdateRotation();
            }
            get { return rotation; }
        }
        public float Radius;
        Vector2[] axes = new Vector2[2];

        public OrientedBoundingBox(Vector2 position, float rotation, int width, int height)
        {
            UL = new Vector2(position.X, position.Y);
            DL = new Vector2(position.X, position.Y + height);
            DR = new Vector2(position.X + width, position.Y + height);
            UR = new Vector2(position.X + width, position.Y);
            this.position = position;
            origin = new Vector2(Width / 2, Height / 2);
            Rotation = rotation;
            Radius = (float)Math.Sqrt(Math.Pow(Width / 2, 2) + Math.Pow(Height / 2, 2));
            MaxXY = ((float)Math.Max(Math.Max(UL.X, UR.X), Math.Max(DL.X, DR.X)),(float)Math.Max(Math.Max(UL.Y, UR.Y), Math.Max(DL.Y, DR.Y)));
            MinXY = ((float)Math.Min(Math.Min(UL.X, UR.X), Math.Min(DL.X, DR.X)),(float)Math.Min(Math.Min(UL.Y, UR.Y), Math.Min(DL.Y, DR.Y)));
        }
        public bool CollidesWith(OrientedBoundingBox r)
        {
            bool collides = true;
            //GenerateAxes();
            //r.GenerateAxes();
            Vector2[] allAxes = new Vector2[] { axes[0], axes[1], r.axes[0], r.axes[1] };
            float[] scalarA = new float[4];
            float[] scalarB = new float[4];
            foreach (Vector2 axis in allAxes)
            {
                scalarA[0] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(UL, axis) / axis.LengthSquared()));
                scalarA[1] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(DL, axis) / axis.LengthSquared()));
                scalarA[2] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(DR, axis) / axis.LengthSquared()));
                scalarA[3] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(UR, axis) / axis.LengthSquared()));
                scalarB[0] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(r.UL, axis) / axis.LengthSquared()));
                scalarB[1] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(r.DL, axis) / axis.LengthSquared()));
                scalarB[2] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(r.DR, axis) / axis.LengthSquared()));
                scalarB[3] = Vector2.Dot(axis, Vector2.Multiply(axis, Vector2.Dot(r.UR, axis) / axis.LengthSquared()));
                if (scalarB.Max() < scalarA.Min() + 0.01f || scalarA.Max() < scalarB.Min() + 0.01f)
                    collides = false;
            }
            return collides;
        }

        private void UpdateRotation()
        {
            Matrix rotationMatrix = Matrix.CreateRotationZ(rotation);
            Vector2 height = new Vector2(0, Height);
            Vector2 width = new Vector2(Width, 0);
            UL = Position + Vector2.Transform(-Origin, rotationMatrix);
            DL = Position + Vector2.Transform(-Origin + height, rotationMatrix);
            DR = Position + Vector2.Transform(-Origin + height + width, rotationMatrix);
            UR = Position + Vector2.Transform(-Origin + width, rotationMatrix);
        }

        public bool Contains(Vector2 position)
        {
            Vector2 AM = position - UL;
            Vector2 AD = DL - UL;
            Vector2 AB = UR - UL;
            return 0 <= Vector2.Dot(AM, AB) && Vector2.Dot(AM, AB) <= Vector2.Dot(AB, AB) && 0 <= Vector2.Dot(AM, AD) && Vector2.Dot(AM, AD) <= Vector2.Dot(AD, AD);
        }
        public Vector2[] GenerateAxes()
        {
            axes[0] = new Vector2(UR.X - UL.X, UR.Y - UL.Y);
            axes[1] = new Vector2(UR.X - DR.X, UR.Y - DR.Y);
            return axes;
        }
    }
}