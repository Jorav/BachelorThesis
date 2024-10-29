using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BachelorThesis.src.old;


namespace BachelorThesis.src
{
    public class EntityFactory
    {

        public static int ScreenWidth;
        public static int ScreenHeight;

        public static string[] ReadConfig()
        {
            string filePath = Directory.GetCurrentDirectory();
            string conffile = Path.GetFullPath(Path.Combine(filePath, @"..\..\..\config.txt"));
            using (StreamReader sr = new StreamReader(conffile))
            {
                string[] lines = File.ReadAllLines(conffile);
                return lines;
            }
        }
        public static List<WorldEntity> EntFacImplementation(string SeedVar, string NbrObj, Texture2D textureParticle)
        {

            //Controller Entitycontroller = new Controller();
            //Controller Entitycontroller = controller;
            List<WorldEntity> EntityFacList = new List<WorldEntity>();

            int flSeed = int.Parse(SeedVar);
            int flNbrobj = int.Parse(NbrObj);



            Random rnd = new Random(flSeed);

            float rRadius;
            float rAngle;
            float rRotation;

            for (int j = 0; j < flNbrobj; j++)
            {
                //rRadius = (float)(rnd.NextDouble()*(500+100*flNbrobj/Game1.GRAVITY));//+Math.Pow(flNbrobj,2)/5/Game1.GRAVITY/Game1.GRAVITY));
                rRadius = (float)(rnd.NextDouble()*12*flNbrobj);
                rAngle = (float)(rnd.NextDouble()*2*Math.PI);
                rRotation = (float)(rnd.NextDouble() * 2 * Math.PI);
                //rndrang1 = rnd.Next(-1000-7*flNbrobj, 1000+7*flNbrobj);
                //rndrang2 = rnd.Next(-1000-7*flNbrobj, 1000+7*flNbrobj);
                //controllerlist.Add((Entitycontroller.AddEntity(new WorldEntity(textureParticle, new Vector2(rndrang1, rndrang2), 100f)));
                //EntityFacList.Add(new WorldEntity(textureParticle, new Vector2(rndrang1, rndrang2), 100f));
                EntityFacList.Add(new WorldEntity(textureParticle, new Vector2((float)Math.Sin(rAngle), (float)Math.Cos(rAngle)) * rRadius, 100f) { Rotation = rRotation });
            }
            return EntityFacList;
        }
    }
}
