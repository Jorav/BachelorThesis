using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BachelorThesis.src.BVH;
using BachelorThesis.src.old;

namespace BachelorThesis.src
{
  public class Tests
  {
    Game1 game;
    Camera camera;
    PerformanceMeasurer measurer;
    IController controller;
    public static int[] nrOfEntities = { 10, 100, 1000};
    public static int CURRENT_NR_TEST = 0;
    private String[] testCases = { "AABB_TD_median", "AABB_TD_SAH", "AABB_insertion_SAH", "controller_unoptimised", "controller_SAT", "controller_boundingSpheres" };
    public static int CURRENT_CONTROLLER_TEST = 0;
    public static int ITERATIONS_PER_TEST = Game1.FRAMES_PER_SECOND * 10;
    public int currentIterations = 0;
    public List<WorldEntity> entities;
    private List<Vector2>[] positions;

    public Tests(Game1 game, Camera camera, int seed)
    {
      this.game = game;
      this.camera = camera;
      measurer = new PerformanceMeasurer();
      GeneratePositions(seed);
    }

    private void GeneratePositions(int seed)
    {
      Random rnd = new Random(seed);

      float rRadius;
      float rAngle;
      positions = new List<Vector2>[nrOfEntities.Length];
      for (int i = 0; i < nrOfEntities.Length; i++)
      {
        positions[i] = new List<Vector2>(nrOfEntities[i]);
        for (int j = 0; j < nrOfEntities[i]; j++)
        {
          rRadius = (float)(rnd.NextDouble() * 12 * nrOfEntities[i]);
          rAngle = (float)(rnd.NextDouble() * 2 * Math.PI);
          positions[i].Add(new Vector2((float)Math.Sin(rAngle), (float)Math.Cos(rAngle)) * rRadius);
        }
      }
    }

    public void LoadEntities(List<WorldEntity> entities)
    {
      this.entities = entities;
      LoadController();
      LoadEntities();
    }

    public void LoadController()
    {
      foreach (WorldEntity we in entities)
        we.Reset();
      switch (CURRENT_CONTROLLER_TEST)
      {
        case 0: controller = new AABBTree(); controller.VERSION_USED = 0; WorldEntity.UseBoundingCircle = false; break;
        case 1: controller = new AABBTree(); controller.VERSION_USED = 1; WorldEntity.UseBoundingCircle = false; break;
        case 2: controller = new AABBTree(); controller.VERSION_USED = 2; WorldEntity.UseBoundingCircle = false; break;
        case 3: controller = new Controller(entities); controller.VERSION_USED = 0; WorldEntity.UseBoundingCircle = false; break;
        case 4: controller = new Controller(entities); controller.VERSION_USED = 1; WorldEntity.UseBoundingCircle = false; break;
        case 5: controller = new Controller(entities); controller.VERSION_USED = 2; WorldEntity.UseBoundingCircle = true; break;
        default: game.Exit(); break;
      }
    }
    public void LoadEntities()
    {
      List<WorldEntity> entitySet = entities.GetRange(0, nrOfEntities[CURRENT_NR_TEST]);
      for(int i = 0; i<entitySet.Count; i++){
        entitySet[i].Position = positions[CURRENT_NR_TEST][i];
      }
      controller.SetEntities(entitySet);
      controller.BuildTree();
      camera.SetController(controller);
    }

    public void Update(GameTime gameTime)
    {
      controller.UpdateDeterministic();
      measurer.Tick();
      controller.BuildTree();
      measurer.Tick();
      controller.GetInternalCollissions();
      measurer.Tick();
      controller.ResolveInternalCollissions();
      measurer.Tick();
      if (++currentIterations == ITERATIONS_PER_TEST)
      {
        SaveData();
        CURRENT_NR_TEST++;
        currentIterations = 0;
        if (CURRENT_NR_TEST == nrOfEntities.Length)
        {
          CURRENT_NR_TEST = 0;
          if (++CURRENT_CONTROLLER_TEST == CURRENT_CONTROLLER_TEST)//6
            game.Exit();
          else
            LoadController();
        }
        LoadEntities();
      }
      camera.Update();
    }
    private void SaveData()
    {
      String fileName = testCases[CURRENT_CONTROLLER_TEST] + "_" + nrOfEntities[CURRENT_NR_TEST] + ".txt";
      measurer.SubmitTimes(fileName);
    }
    public void Draw(SpriteBatch sb)
    {
      controller.Draw(sb);
    }
  }
}