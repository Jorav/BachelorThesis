using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BachelorThesis.src
{
    public interface IController
    {
      public Vector2 Position{get;}
      public float Radius {get;}
      public void UpdateDeterministic();
      public void BuildTree();
      public void GetInternalCollissions();
      public void ResolveInternalCollissions();
      public void SetEntities(List<WorldEntity> entities);
      public void Draw(SpriteBatch sb);
      public int VERSION_USED {get; set;}
    }
}