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
        
        private InputHandler input;
        private BasicEffect effect;
        private Matrix world, view, projection;
        private SpriteBatch spriteBatch;
        private Stack<Matrix> matrixStrack = new Stack<Matrix>();
        private boat heroBoat;
        private Model heroBoatModel;
        private Texture2D[] Textures = new Texture2D[10];
        
        private Vector3 camTar = Vector3.Zero;
        private Vector3 camUpVec = Vector3.Up;
        private Vector3 camPos = new Vector3(10f, 10f, 10f);


        public bool isFullScreen = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            input = new InputHandler(this);
            this.Components.Add(input);


            heroBoat = new boat(10, 10, 10, new Vector3(20.0f, 0.0f, 0.0f), null);

            this.IsFixedTimeStep = true;


        }

        private void InitializeCam()
        {
            float aspecRatio = (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspecRatio, 1.0f, 5000.0f, out projection);
            Matrix.CreateLookAt(ref camPos, ref camTar, ref camUpVec, out view);
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
        
        }

        private void loadBoatObjects()
        {
            Textures[0] = Content.Load<Texture2D>("Textures/heroBoat");    // HeroBoat
            //heroBoat.BoatModel = Content.Load<Model>("Models/heroBoat"); // HeroBoat Model

            heroBoatModel = Content.Load<Model>("Models/heroBoat");  // HeroBoat Model
            (heroBoatModel.Meshes[0].Effects[0] as BasicEffect).Texture = Textures[0];
            (heroBoatModel.Meshes[0].Effects[0] as BasicEffect).TextureEnabled = true;
        }

        protected override void UnloadContent()
        {
        
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // fixes bug with planets overlapping / not going behind the sun in a 3D perspective 
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            DepthStencilState depthBufferState = new DepthStencilState();
            depthBufferState.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = depthBufferState;

            // execute fix again
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.DepthStencilState = depthBufferState;

                
            effect.World = Matrix.Identity;
            effect.Projection = projection;
            effect.View = view;

            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.5f, 0.0f));
            effect.EmissiveColor = Color.Red.ToVector3();

            this.DrawHero(gameTime);

            effect.DirectionalLight0.Enabled = false;

            base.Draw(gameTime);
        }

        public void DrawHero(GameTime gameTime)
        {
            Matrix matScale, matRotateY, matTrans;
            matScale = Matrix.CreateScale(5.0f);
            matTrans = Matrix.CreateTranslation(1.0f, 0.1f, 1.0f);
            matRotateY = Matrix.CreateRotationY(0.0f);

            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;
            heroBoatModel.Draw(world, view, projection);
        }
    }
}
