using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace BachelorThesis.src
{
    public class Sprite
    {
        public Texture2D texture;
        public float Rotation { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Origin { get; set; }
        public int Height { get { return (int)texture.Height; } }
        public int Width { get { return (int)texture.Width; } }
        public Color Color { get; set; }
        public bool isVisible = true;

        public Sprite(Texture2D texture)
        {
            this.texture = texture;
            Origin = new Vector2(texture.Width / 2, texture.Height / 2);
            Color = Color.White;
        }

        public void Update(GameTime gameTime) { }

        public void Draw(SpriteBatch sb)
        {
            if (isVisible)
                sb.Draw(texture, Position, null, Color, Rotation, Origin, 1, SpriteEffects.None, 0f);
        }
    }
}
