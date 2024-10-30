using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace BachelorThesis.src.bounding_areas
{
    public class AxisAlignedBoundingBox
    {
        public Vector2 UL { get; set; }
        private Vector2 DL { get; set; }
        private Vector2 DR { get; set; }
        private Vector2 UR { get; set; }
        public float Area { get { return Width * Height; } }
        public float Width { get { return (int)(Math.Round((UR - UL).Length())); } }
        public float Height { get { return (int)(Math.Round((UR - DR).Length())); } }
        public (float, float) MaxXY {get; set;}
        public (float, float) MinXY {get; set;}
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
        public float Radius;

        public AxisAlignedBoundingBox(Vector2 upperLeftCorner, int width, int height)
        {
            SetBox(upperLeftCorner, width, height);
        }
        public AxisAlignedBoundingBox(OrientedBoundingBox OBB)
        {
            (float, float) minXY = OBB.MinXY;
            (float, float) maxXY = OBB.MaxXY;
            float width = maxXY.Item1 - minXY.Item1;
            float height = maxXY.Item2 - minXY.Item2;
            SetBox(new Vector2(minXY.Item1, minXY.Item2), width, height);
        }

        public void SetBox(Vector2 upperLeftCorner, float width, float height)
        {
            UL = new Vector2(upperLeftCorner.X, upperLeftCorner.Y);
            DL = new Vector2(upperLeftCorner.X, upperLeftCorner.Y + height);
            DR = new Vector2(upperLeftCorner.X + width, upperLeftCorner.Y + height);
            UR = new Vector2(upperLeftCorner.X + width, upperLeftCorner.Y);
            this.position = upperLeftCorner;
            Radius = (float)Math.Sqrt(Math.Pow(width / 2, 2) + Math.Pow(height / 2, 2));
            MaxXY = ((float)Math.Max(Math.Max(UL.X, UR.X), Math.Max(DL.X, DR.X)),(float)Math.Max(Math.Max(UL.Y, UR.Y), Math.Max(DL.Y, DR.Y)));
            MinXY = ((float)Math.Min(Math.Min(UL.X, UR.X), Math.Min(DL.X, DR.X)),(float)Math.Min(Math.Min(UL.Y, UR.Y), Math.Min(DL.Y, DR.Y)));
        }

        public static AxisAlignedBoundingBox SurroundingAABB(AxisAlignedBoundingBox AABB1, AxisAlignedBoundingBox AABB2)
        {
            float xMin = Math.Min(AABB1.UL.X, AABB2.UL.X);
            float xMax = Math.Max(AABB1.UR.X, AABB2.UR.X);
            float yMin = Math.Min(AABB1.UL.Y, AABB2.UL.Y);
            float yMax = Math.Max(AABB1.DL.Y, AABB2.DL.Y);
            return BoundingAreaFactory.CreateAABB(new Vector2(xMin, yMin), (int)Math.Round(xMax - xMin), (int)Math.Round(yMax - yMin));
        }

        public static AxisAlignedBoundingBox SurroundingAABB(List<WorldEntity> entities)
        {/*
            OrientedBoundingBox[] OBBs = new OrientedBoundingBox[entities.Length]; //TODO: SORT LIST ON AXIS
            for (int i = 0; i < entities.Length; i++)
                OBBs[i] = entities[i].OBB;
            return SurroundingAABB(OBBs);*/
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            foreach (WorldEntity we in entities)
            {
                OrientedBoundingBox OBB = we.OBB;
                (float, float) maxXY = OBB.MaxXY;
                (float, float) minXY = OBB.MinXY;
                if (maxXY.Item1 > maxX)
                    maxX = maxXY.Item1;
                if (minXY.Item1 < minX)
                    minX = minXY.Item1;
                if (maxXY.Item2 > maxY)
                    maxY = maxXY.Item2;
                if (minXY.Item2 < minY)
                    minY = minXY.Item2;
            }
            return BoundingAreaFactory.CreateAABB(new Vector2(minX, minY), (int)Math.Round(maxX - minX), (int)Math.Round(maxY - minY));
        }

        public static AxisAlignedBoundingBox SurroundingAABB(OrientedBoundingBox OBB)
        {
            (float, float) maxXY = OBB.MaxXY;
            (float, float) minXY = OBB.MinXY;
            float minX = minXY.Item1;
            float minY = minXY.Item2;
            float maxX = maxXY.Item1;
            float maxY = maxXY.Item2;
            return BoundingAreaFactory.CreateAABB(new Vector2(minX, minY), (int)Math.Round(maxX - minX), (int)Math.Round(maxY - minY));
        }

        public static int MajorAxis(AxisAlignedBoundingBox AABB)
        {
            (float, float) diffXY = (AABB.MaxXY.Item1-AABB.MinXY.Item1, AABB.MaxXY.Item2 - AABB.MinXY.Item2);
            if(diffXY.Item1>=diffXY.Item2)
                return 0; //x
            return 1; //y
        }

        public bool CollidesWith(AxisAlignedBoundingBox AABB)
        {
            if (MaxXY.Item1 < AABB.MinXY.Item1 || MinXY.Item1 > AABB.MaxXY.Item1) return false;
            if (MaxXY.Item2 < AABB.MinXY.Item2 || MinXY.Item2 > AABB.MaxXY.Item2) return false;
            return true;
/*
            return UL.X < AABB.UL.X + AABB.Width &&
                    UL.X + Width > AABB.UL.X &&
                    UL.Y < AABB.UL.Y + AABB.Height &&
                    UL.Y + Height > AABB.UL.Y;*/
        }

        /*public static AxisAlignedBoundingBox SurroundingAABB_sorted(WorldEntity[] worldEntities)
        {
            foreach(WorldEntity we in worldEntities){

            }
        }*/
    }
}