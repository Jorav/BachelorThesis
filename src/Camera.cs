using Microsoft.Xna.Framework;
using BachelorThesis.src.BVH;
using BachelorThesis.src.old;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace BachelorThesis.src
{
    public class Camera
    {
        public Matrix Transform { get; private set; }
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public float Rotation { get; set; }
        public float Zoom { get; set; }
        public float Width { get { return Game1.ScreenWidth / Zoom; } }
        public float Height { get { return Game1.ScreenHeight / Zoom; } }
        public bool AutoAdjustZoom { get; set; }
        public float GameZoom { get { if (Controller != null) return 1.0f*Game1.ScreenHeight / (Game1.ScreenHeight + 1 * Controller.Radius); else return 1; } }
        public IController Controller { get; set; }
        private float zoomSpeed;

        public Camera([OptionalAttribute] AABBTree controller, float zoomSpeed = 0.001f)
        {
            if (controller != null)
                SetController(controller);
            else
                Position = Vector2.Zero;
            PreviousPosition = Position;
            Rotation = 0;
            Zoom = GameZoom;
            this.zoomSpeed = zoomSpeed;
            AutoAdjustZoom = true;
            UpdateTransformMatrix();
        }
        public void SetController(IController controller){
            this.Controller = controller;
            Position = controller.Position;
            PreviousPosition = Position;
            this.Controller = controller;
            Zoom = GameZoom;
            UpdateTransformMatrix();
        }

        public void Update()
        {
            PreviousPosition = Position;
            if (Controller != null)
                AdjustPosition();
            if (AutoAdjustZoom)
            {
                AdjustZoom(GameZoom);
            }

            Rotation = 0;
            UpdateTransformMatrix();
        }

        private void AdjustPosition()
        {
            PreviousPosition = Position;
            
            //    Position = tree.MassCenter;
            //else
            Position = Controller.Position;// PreviousPosition + 0.001f*(Controller.Position-PreviousPosition);
            if (Controller is AABBTree tree)
                Position += new Vector2(tree.root.AABB.Width / 2, tree.root.AABB.Height / 2);
        }

        private void AdjustZoom(float optimalZoom)
        {
            if (optimalZoom > Zoom)
            {
                if (optimalZoom / Zoom > 1 + zoomSpeed)
                    Zoom *= 1 + zoomSpeed;
                else
                    Zoom = optimalZoom;
            }
            else if (optimalZoom < Zoom)
            {
                if (Zoom / optimalZoom > 1 + zoomSpeed)
                    Zoom /= 1 + zoomSpeed;
                else
                    Zoom = optimalZoom;
            }
        }

        public void UpdateTransformMatrix()
        {
            Matrix position = Matrix.CreateTranslation(
                -Position.X,
                -Position.Y,
                0);
            Matrix rotation = Matrix.CreateRotationZ(Rotation);
            Matrix origin = Matrix.CreateTranslation(
                Game1.ScreenWidth / 2,
                Game1.ScreenHeight / 2,
                0);
            Matrix zoom = Matrix.CreateScale(Zoom, Zoom, 0);
            Transform = position * rotation * zoom * origin;
        }
    }
}
