using BachelorThesis.src.BVH;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;

namespace BachelorThesis.src
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private AABBTree controllerTree;
        //private Controller controller;
        private Camera camera;
        private PerformanceMeasurer performanceMeasurer;
        //private MeanSquareError meanSquareError;
        public static int ScreenWidth;
        public static int ScreenHeight;
        public static float GRAVITY = 10;
        public static SpriteFont font;
        public static int FRAMES_PER_SECOND = 60;
        public static float TIME_STEP = (1f / FRAMES_PER_SECOND);
        public Tests tests;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            //Add your initialization logic here
            //_graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            //_graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            ScreenWidth = _graphics.PreferredBackBufferWidth;
            ScreenHeight = _graphics.PreferredBackBufferHeight;
            _graphics.PreferMultiSampling = true;
            base.Initialize();

        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // use this and Content to load your game content here
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            _graphics.ApplyChanges();

            Texture2D textureParticle = Content.Load<Texture2D>("RotatingHull");
            font = Content.Load<SpriteFont>("font");
            controllerTree = new AABBTree();

            string[] ConfigVar = new string[4]; ;// EntityFactory.ReadConfig();
            int seed = 0;// int.Parse(ConfigVar[0]);
            int nr = 20;// int.Parse(ConfigVar[1]); f
            float gravity = 10;// float.Parse(ConfigVar[2]);
            //int test_case = int.Parse(ConfigVar[3]);

           List<WorldEntity> returnedList = EntityFactory.EntFacImplementation(seed.ToString(),nr.ToString(),textureParticle);
            //int seed = 100;
            //camera = new Camera();
            //Tests.nrOfEntities = new int[] { nr };
            //tests = new Tests(this, camera, seed);

            /*int maxEntitiesNeeded = 0;
            for(int i = 0; i< tests.nrOfEntities.Length; i++)
                if(tests.nrOfEntities[i] > maxEntitiesNeeded)
                    maxEntitiesNeeded = tests.nrOfEntities[i];*/
            GRAVITY = gravity;
            //Tests.CURRENT_CONTROLLER_TEST = test_case;
            //tests.LoadEntities(EntityFactory.EntFacImplementation(seed.ToString(), nr.ToString(), textureParticle));
            controllerTree.root = controllerTree.CreateTreeTopDown_Median(null, returnedList);
            //controller = new Controller(returnedList);
            /*foreach(WorldEntity w in returnedList)
            {
                controllerTree.Add(w);
            }*/
            camera = new Camera(controllerTree);
            //performanceMeasurer = new PerformanceMeasurer();
            //meanSquareError = new MeanSquareError(returnedList.ToArray());
            //meanSquareError.LoadPreviousPositions();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            UpdateRunning(gameTime);
            //UpdateDeterministic(gameTime);

            //this.SuppressDraw();
            //tests.Update(gameTime);
            base.Update(gameTime);
        }


        //for running
        protected void UpdateRunning(GameTime gameTime)
        {
            ScreenWidth = _graphics.PreferredBackBufferWidth;
            ScreenHeight = _graphics.PreferredBackBufferHeight;
            // Add your update logic here
            controllerTree.Update(gameTime);
            //controller.Update(gameTime);
            camera.Update();
            //performanceMeasurer.Update(gameTime);
            //meanSquareError.Update(gameTime);
            base.Update(gameTime);
        }

        //for testing
        protected void UpdateDeterministic(GameTime gameTime)
        {
            // Add your update logic here
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            //    Exit();

            //controllerTree.UpdateDeterministic(performanceMeasurer);
            //controller.UpdateDeterministic();
            //camera.Update();
            //meanSquareError.UpdateDeterministic(timeStep);
            tests.Update(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            /**/
            // Add your drawing code here
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin(transformMatrix: camera.Transform, sortMode: SpriteSortMode.Deferred, blendState: BlendState.AlphaBlend, samplerState: SamplerState.AnisotropicClamp);
            // _spriteBatch.Begin();
            //tests.Draw(_spriteBatch);
            controllerTree.Draw(_spriteBatch);
            //controller.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
