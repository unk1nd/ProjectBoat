/*
 *  Project Boat
 *  Av 
 *  Mikael Bendiksen, Lisa Marie Sørensen og Svein Olav Bustnes
 *  2013
 * 
*/


using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace ProjectBoat
{

    public class Game1 : Microsoft.Xna.Framework.Game
    {

        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private Camera camera;
        private InputHandler input;
        private BasicEffect effect;
        private SpriteFont font;
        private Matrix world, view, projection;
        private SpriteBatch spriteBatch;
        private Stack<Matrix> matrixStrack = new Stack<Matrix>();
        private boat heroBoat;
        private Model heroBoatModel;
        private Texture2D[] Textures = new Texture2D[10];
        private boat[] boatArray = new boat[1];
        /*
        private Vector3 camTar = Vector3.Zero;
        private Vector3 camUpVec = Vector3.Up;
        private Vector3 camPos = new Vector3(1f, 2f, 0f);
        */

        public bool isFullScreen = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            input = new InputHandler(this);
            this.Components.Add(input);

            camera = new Camera(this);
            this.Components.Add(camera);

            this.IsFixedTimeStep = true;
        }

        private void initDevice()
        {
            device = graphics.GraphicsDevice;
            graphics.PreferredBackBufferWidth = 1400;
            graphics.PreferredBackBufferHeight = 800;
            graphics.IsFullScreen = isFullScreen;
            graphics.ApplyChanges();

            Window.Title = "Project Boat";

            //Initialiserer Effect-objektet:
            effect = new BasicEffect(graphics.GraphicsDevice);
            effect.VertexColorEnabled = false;
            effect.TextureEnabled = true;

        }
        
        protected override void Initialize()
        {
            initDevice();
           
            base.Initialize();
            this.IsMouseVisible = true;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            loadBoatObjects();

            // loading of font used for camera info
            font = Content.Load<SpriteFont>("font");
        }

        private void loadBoatObjects()
        {
            //Textures[0] = Content.Load<Texture2D>("Textures/tex_ship_hero");    // HeroBoat

            boatArray[0] = new boat(1, 1, 10);
            boatArray[0].BoatModel = Content.Load<Model>("Models/HeroShip");  // HeroBoat Model
            (boatArray[0].BoatModel.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();

            /*
            //for (int i = 0; i < 8; i++)
            //{
                (boatArray[0].BoatModel.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
                (boatArray[0].BoatModel.Meshes[0].Effects[0] as BasicEffect).Texture = Textures[0];
                (boatArray[0].BoatModel.Meshes[0].Effects[0] as BasicEffect).TextureEnabled = true;
            //}
            */
           //heroBoatModel = Content.Load<Model>("Models/HeroShip");  // sun
            

        }

        protected override void UnloadContent()
        {
        
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            if (input.KeyboardState.IsKeyDown(Keys.Up))
            {

                boatArray[0].X = boatArray[0].X - 0.1f;
                boatArray[0].boatPosition = new Vector3(boatArray[0].X, boatArray[0].Y, 0);

            }
            if (input.KeyboardState.IsKeyDown(Keys.Down))
            {

                boatArray[0].X = boatArray[0].X + 0.1f;
                boatArray[0].boatPosition = new Vector3(boatArray[0].X, boatArray[0].Y, 0);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);
 
            //GraphicsDevice.BlendState = BlendState.Opaque;
            //GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            //DepthStencilState depthBufferState = new DepthStencilState();
            //depthBufferState.DepthBufferEnable = true;
            //GraphicsDevice.DepthStencilState = depthBufferState;
            
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            //rasterizerState1.FillMode = FillMode.WireFrame;
            device.RasterizerState = rasterizerState1;

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Boat Info: " + boatArray[0].BoatPosition, new Vector2(0.0f, 1), Color.WhiteSmoke);
            spriteBatch.End();

            effect.World = Matrix.Identity;
            effect.Projection = camera.Projection;
            effect.View = camera.View;

            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.5f, 0.0f));
            effect.EmissiveColor = Color.Red.ToVector3();
            effect.DirectionalLight0.Enabled = false;

            //this.DrawHero(gameTime);
            //this.DrawBoat(gameTime);

            foreach (boat b in boatArray)
            {
                this.DrawBoat(gameTime, b);
                matrixStrack.Pop();
            }
            
            base.Draw(gameTime);
        }

        public void DrawHero(GameTime gameTime)
        {
            Matrix matScale, matRotateY, matTrans;

            matScale = Matrix.CreateScale(0.005f);
            matTrans = Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);

            matRotateY = Matrix.CreateRotationY(0f);
            
            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;
            heroBoatModel.Draw(world, camera.View, camera.Projection);
        }


        private void DrawBoat(GameTime gameTime, boat b)
        {
            Matrix matScale, matRotateY, matTrans;

            matScale = Matrix.CreateScale(0.005f);
            matTrans = Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);

            matRotateY = Matrix.CreateRotationY(0f);

            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;
           
            b.BoatPosition = Matrix.Invert(world).Translation;
            b.BoatModel.Draw(world, camera.View, camera.Projection);

        }
    }
}
