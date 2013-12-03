/*
 *  Project Boat
 *  Av 
 *  Mikael Bendiksen, Lisa Marie Sørensen og Svein Olav Bustnes
 *  2013
 * 
 * Controlls:
 * 
 * Forward: W
 * Rotate left: A
 * Rotate Right: D
 * Debug mode on: I
 * Debug mode off: O
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

        #region Variabler

        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        private InputHandler input;
        private BasicEffect effect;
        private Effect effect2;
        private SpriteFont font;
        private Matrix world, view, proj;
        private SpriteBatch spriteBatch;
        private Stack<Matrix> matrixStrack = new Stack<Matrix>();
        private Model worldTemp;
        private Texture2D[] Textures = new Texture2D[10];
        private boat[] boatArray = new boat[3];
        private boat HERO;
        private Vector3 heroPosition = Vector3.Zero;

        private Vector3 modelVelocity = Vector3.Zero;
        private Quaternion heroRot = Quaternion.Identity;
        Quaternion cameraRotation = Quaternion.Identity;
        
        //Modeller:
        private Model modelHERO, modelEnemy;
        private Model modelWorld_North, modelWorld_East, modelWorld_West, modelWorld_South, modelIsland1, modelIsland2, modelIsland3;

        //Materiser som holder "akkumulerte" bone-transformasjonene:
        private Matrix[] matrixHERO, matrixEnemy;
        private Matrix[] matrixWorld_North, matrixWorld_East, matrixWorld_West, matrixWorld_South, matrixIsland1, matrixIsland2, matrixIsland3;

        //Tar vare på opprinnelig Bone-transformasjoner:
        private Matrix[] originalTransforms1;
        private Matrix[] originalTransforms2;
        private Matrix[] originalTransforms3;
        private Matrix[] originalTransforms4;
        private Matrix[] originalTransforms5;
        private Matrix[] originalTransforms6;
        private Matrix[] originalTransforms7;
        private Matrix[] originalTransforms8;

        private VertexBuffer vertexBuffer;
        private Texture2D vann;

        private const float FLOOR_WIDTH = 9999.0f;
        private const float FLOOR_HEIGHT = 9999.0f;
        private const float FLOOR_TILE_U = 4.0f;
        private const float FLOOR_TILE_V = 4.0f;

        private Model skyboxModel;

        private bool infotext = false;
        private bool isCollition = false;

        private int windowWidth;
        private int windowHeight;
        private int frames;
        private int framesPerSecond;
        private TimeSpan elapsedTime = TimeSpan.Zero;

        public bool isFullScreen = false;
        #endregion

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            input = new InputHandler(this);
            this.Components.Add(input);

            this.IsFixedTimeStep = true;
        }

        private void initDevice()
        {
            // Setup the window to be a quarter the size of the desktop.
            windowWidth = GraphicsDevice.DisplayMode.Width -200;
            windowHeight = GraphicsDevice.DisplayMode.Height -200;

            // Setup frame buffer.
            device = graphics.GraphicsDevice;
            graphics.PreferredBackBufferWidth = windowWidth;
            graphics.PreferredBackBufferHeight = windowHeight;
            graphics.IsFullScreen = isFullScreen;
            graphics.PreferMultiSampling = true;
            graphics.SynchronizeWithVerticalRetrace = false;
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
            effect2 = Content.Load<Effect>(@"Effects\effects");
            vann = Content.Load<Texture2D>(@"Textures\tex_water");
            
        }

        #region kolisjon

        private Model LoadModelWithBoundingSphere(String modelName, ref Matrix[] matrix, ref Matrix[] originalTransforms)
        {
            Model model = Content.Load<Model>(modelName);
            matrix = new Matrix[model.Bones.Count];

            model.CopyAbsoluteBoneTransformsTo(matrix);

            BoundingSphere completeBoundingSphere = new BoundingSphere();

            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere origMeshSphere = mesh.BoundingSphere;
                origMeshSphere = XNAUtils.TransformBoundingSphere(origMeshSphere, matrix[mesh.ParentBone.Index]);
                completeBoundingSphere = BoundingSphere.CreateMerged(completeBoundingSphere, origMeshSphere);
            }

            model.Tag = completeBoundingSphere;
            originalTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(originalTransforms);
            return model;
        }

        bool ModelsCollide(Model _model1, Matrix _world1, Model _model2, Matrix _world2)
        {
            BoundingSphere origSphere1 = (BoundingSphere)_model1.Tag;
            BoundingSphere sphere1 = XNAUtils.TransformBoundingSphere(origSphere1, _world1);

            BoundingSphere origSphere2 = (BoundingSphere)_model2.Tag;
            BoundingSphere sphere2 = XNAUtils.TransformBoundingSphere(origSphere2, _world2);

            bool collision = sphere1.Intersects(sphere2);
            return collision;
        }

        #endregion

        private void loadBoatObjects()
        {
            // different textures
            Textures[0] = Content.Load<Texture2D>("Textures/tex_ship_hero");    // HeroBoat
            Textures[1] = Content.Load<Texture2D>("Textures/soil_texture");    // jordtextur
            Textures[2] = Content.Load<Texture2D>("Textures/skybox_top");
            Textures[3] = Content.Load<Texture2D>("Sprites/map");

            // hero
            modelHERO = this.LoadModelWithBoundingSphere("Models/HeroShip", ref matrixHERO, ref originalTransforms1);
            HERO = new boat(10, new Vector3(5, 5, 10));
            HERO.BoatModel = modelHERO;
            (HERO.BoatModel.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();

            // enemy boats
            boatArray[0] = new boat(6, new Vector3(6, 0, -4));
            boatArray[1] = new boat(50, new Vector3(-8, 0, 10));
            boatArray[2] = new boat(180, new Vector3(-9, 0, 5));

            // array for setting enemy boats models
            for (int i = 0; i < boatArray.Length; i++) 
            {
                modelEnemy = this.LoadModelWithBoundingSphere("Models/EnemyShip", ref matrixEnemy, ref originalTransforms2);
                boatArray[i].BoatModel = modelEnemy; 
                (boatArray[i].BoatModel.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
            }

            // load world model and load texture
            modelWorld_North = this.LoadModelWithBoundingSphere("Models/World_Northnew", ref matrixWorld_North, ref originalTransforms2);
            modelWorld_East = this.LoadModelWithBoundingSphere("Models/World_Eastnew", ref matrixWorld_East, ref originalTransforms3);
            modelWorld_West = this.LoadModelWithBoundingSphere("Models/World_Westnew", ref matrixWorld_West, ref originalTransforms4);
            modelWorld_South = this.LoadModelWithBoundingSphere("Models/World_Southnew", ref matrixWorld_South, ref originalTransforms5);
            modelIsland1 = this.LoadModelWithBoundingSphere("Models/Island1new", ref matrixIsland1, ref originalTransforms6);
            modelIsland2 = this.LoadModelWithBoundingSphere("Models/Island2new", ref matrixIsland2, ref originalTransforms7);
            modelIsland3 = this.LoadModelWithBoundingSphere("Models/Island3new", ref matrixIsland3, ref originalTransforms8);
            
            // skybox model 
            skyboxModel = Content.Load<Model>("Models/skybox2");
        }
        
        protected override void UnloadContent()
        {}

        #region update
        protected override void Update(GameTime gameTime)
        {
            UpdateFrameRate(gameTime);
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Get some input.
            UpdateInput();

            base.Update(gameTime);
        }
        #endregion

        #region keyinputs

        
        protected void UpdateInput()
        {
            
            Vector3 modelVelocityAdd = Vector3.Zero;

            float leftRightRot = 0;

            if (input.KeyboardState.IsKeyDown(Keys.W))
            {
                modelVelocityAdd *= (float)-0.001f;  
                modelVelocity += modelVelocityAdd;
                float moveSpeed = 0.01f;
                MoveForward(ref heroPosition, heroRot, moveSpeed);
            }

            if (input.KeyboardState.IsKeyDown(Keys.A))
            {
                float turn = 0.1f;
                leftRightRot -= turn;           
            }

            if (input.KeyboardState.IsKeyDown(Keys.D))
            {
                float turn = 0.1f;
                leftRightRot += turn;       
            }

            if (input.KeyboardState.IsKeyDown(Keys.I))
            {
                infotext = true;
            }

            if (input.KeyboardState.IsKeyDown(Keys.O))
            {
                infotext = false;
            }

            float upDownRot = 0;
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
            heroRot *= additionalRot;            

        }

        private void UpdateFrameRate(GameTime gameTime)
        {
            elapsedTime += gameTime.ElapsedGameTime;

            if (elapsedTime > TimeSpan.FromSeconds(1))
            {
                elapsedTime -= TimeSpan.FromSeconds(1);
                framesPerSecond = frames;
                frames = 0;
            }
        }

        private void IncrementFrameCounter()
        {
            ++frames;
        }


        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            if (!isCollition)
            {
                Vector3 addVector = Vector3.Transform(new Vector3(0, 0, +2), rotationQuat);
                position += addVector * speed;
            }
            else isCollition = false;
        }
        #endregion

        #region Vannet

        private void CreateFloor()
        {
            float w = 40;
            float h = 40;

            Vector3[] positions =
            {
                new Vector3(-w, 0.05f, -h),
                new Vector3( w, 0.05f, -h),
                new Vector3(-w, 0.05f,  h),
                new Vector3( w, 0.05f,  h)
            };

            Vector2[] texCoords =
            {
                new Vector2(0.0f, 0.0f),
                new Vector2(1, 0.0f),
                new Vector2(0.0f, 1),
                new Vector2(1, 1)
            };

            VertexPositionNormalTexture[] vertices =
            {
                new VertexPositionNormalTexture(
                    positions[0], Vector3.Up, texCoords[0]),
                new VertexPositionNormalTexture(
                    positions[1], Vector3.Up, texCoords[1]),
                new VertexPositionNormalTexture(
                    positions[2], Vector3.Up, texCoords[2]),
                new VertexPositionNormalTexture(
                    positions[3], Vector3.Up, texCoords[3]),
            };

            vertexBuffer = new VertexBuffer(GraphicsDevice,
                typeof(VertexPositionNormalTexture), vertices.Length,
                BufferUsage.WriteOnly);

            vertexBuffer.SetData(vertices);
        }

        private void DrawFloor()
        {
            CreateFloor();
            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            effect.PreferPerPixelLighting = true;
            effect.VertexColorEnabled = false;

            effect.TextureEnabled = true;
            effect.Texture = vann;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
            }

        }
        #endregion

        private void UpdateCamera()
        {

            cameraRotation = Quaternion.Lerp(cameraRotation, heroRot, 0.1f);

            Vector3 campos = new Vector3(0, 2, -10);
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(cameraRotation));
            campos += heroPosition;
            
            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(cameraRotation));
            
            view = Matrix.CreateLookAt(campos, heroPosition, camup);
            proj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 0.2f, 500.0f);
        }

        #region draw

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Blue);
 
            GraphicsDevice.BlendState = BlendState.Opaque;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            DepthStencilState depthBufferState = new DepthStencilState();
            depthBufferState.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = depthBufferState;
            
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState1;

            effect.World = Matrix.Identity;
            effect.Projection = proj;
            effect.View = view;
            
            DrawFloor();
            

            Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(heroRot) * Matrix.CreateTranslation(heroPosition);

            
            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.5f, 0.0f));
            effect.EmissiveColor = Color.Blue.ToVector3();
            effect.DirectionalLight0.Enabled = true;


            Matrix worldHERO, worldEnemy, worldNorth, worldWest, worldEast, worldSouth, worldIsland1, worldIsland2, worldIsland3;
            this.DrawSky(gameTime);
            
            
            this.DrawHERO(modelHERO, out worldHERO, gameTime);
            this.DrawWorldItems(modelWorld_North, out worldNorth, gameTime, 31, 0, matrixWorld_North);
            this.DrawWorldItems(modelWorld_East, out worldEast, gameTime, 0, 31, matrixWorld_East);
            this.DrawWorldItems(modelWorld_West, out worldWest, gameTime, 0,-31, matrixWorld_West);
            this.DrawWorldItems(modelWorld_South, out worldSouth, gameTime, -31, 0, matrixWorld_South);
            this.DrawWorldItems(modelIsland1, out worldIsland1, gameTime, -12f, 12 , matrixIsland1);
            this.DrawWorldItems(modelIsland2, out worldIsland2, gameTime, 12, -12, matrixIsland2);
            this.DrawWorldItems(modelIsland3, out worldIsland3, gameTime, 12, 12, matrixIsland3);
            this.DrawBoats(modelEnemy, out worldEnemy, gameTime, boatArray[0]);


            
            if (
                this.ModelsCollide(modelHERO, worldHERO, modelEnemy, worldEnemy) ||
                /* this.ModelsCollide(modelHERO, worldHERO, modelWorld_North, worldNorth) || // did not work as intended!!! ;*(
                 this.ModelsCollide(modelHERO, worldHERO, modelWorld_East, worldEast) ||
                 this.ModelsCollide(modelHERO, worldHERO, modelWorld_West, worldWest) ||
                 this.ModelsCollide(modelHERO, worldHERO, modelWorld_South, worldSouth)||*/
                 this.ModelsCollide(modelHERO, worldHERO, modelIsland1, worldIsland1) ||
                 this.ModelsCollide(modelHERO, worldHERO, modelIsland2, worldIsland2) ||
                 this.ModelsCollide(modelHERO, worldHERO, modelIsland3, worldIsland3) 
                
                )
            {
                spriteBatch.Begin();
                if (infotext)
                {
                    spriteBatch.DrawString(font, "COLLISJON!!!! ", new Vector2(0.0f, 80), Color.Red);
                }
                spriteBatch.End();

                isCollition = true;
            }
            else 
            {
                isCollition = false;
            }
            

            spriteBatch.Begin();
            if (infotext)
            {
                spriteBatch.DrawString(font, "Boat Info: " + heroPosition + "\nBoatRot: " + heroRot + "\nFPS: " + framesPerSecond, new Vector2(0.0f, 1), Color.WhiteSmoke);
            }
            spriteBatch.Draw(Textures[3], new Rectangle(Window.ClientBounds.Width-170, 0, 170, 170), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
            spriteBatch.End();


            UpdateCamera();

            base.Draw(gameTime);
            IncrementFrameCounter();
        }

        #region Draw Verden

        private void DrawWorldItems(Model model, out Matrix world, GameTime gameTime, float x, float z, Matrix[] m)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState1;

            Matrix matIdent, matScale, matRotY, matTransl;

            matIdent = Matrix.Identity;
            matScale = Matrix.CreateScale(0.005f);
            matTransl = Matrix.CreateTranslation(x, 0, z);

            matRotY = Matrix.CreateRotationY(0f);
            world = matIdent * matScale * matRotY * matTransl;
            
            model.CopyAbsoluteBoneTransformsTo(m);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = m[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = proj;

                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                mesh.Draw();
            }
        }

        private void DrawHERO(Model model, out Matrix world, GameTime gameTime)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState1;

            Matrix matIdent, matScale, matRotY, matTransl;

            matIdent = Matrix.Identity;
            matScale = Matrix.CreateScale(0.005f);
            matTransl = Matrix.CreateTranslation(heroPosition);
            matRotY = Matrix.CreateFromQuaternion(heroRot);
            world = matIdent * matScale * matRotY * matTransl;
            model.CopyAbsoluteBoneTransformsTo(matrixHERO);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = matrixHERO[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = proj;
                    
                    effect.EnableDefaultLighting();
                    effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                mesh.Draw();
            }
        }

        public void DrawWorld(GameTime gameTime)
        {
            Matrix matScale, matRotateY, matTrans;

            matScale = Matrix.CreateScale(0.005f);
            matTrans = Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);

            matRotateY = Matrix.CreateRotationY(0f);
            
            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;

            modelWorld_North.Draw(world, view, proj);
            modelWorld_East.Draw(world, view, proj);
            modelWorld_West.Draw(world, view, proj);
            modelWorld_South.Draw(world, view, proj);
        }

        public void DrawSky(GameTime gameTime)
        {
            Matrix matScale, matRotateY, matTrans;

            matScale = Matrix.CreateScale(1.205f, 0.485f, 1.205f);
            matTrans = Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);

            matRotateY = Matrix.CreateRotationY(0f);

            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;
            skyboxModel.Draw(world, view, proj);
        }

        #endregion

        #region DrawBoat

        private void DrawBoats(Model model, out Matrix world, GameTime gameTime, boat b)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            device.RasterizerState = rasterizerState1;
            Matrix matIdent, matScale, matRotY, matRotZ, matTransl;

            matIdent = Matrix.Identity;
            matScale = Matrix.CreateScale(0.005f);
            matTransl = Matrix.CreateTranslation(b.BoatPosition);
            matRotY = Matrix.CreateRotationY(b.BoatRotY);
            matRotZ = Matrix.Identity; 

            world = matIdent * matScale * matRotZ * matRotY * matTransl;
            model.CopyAbsoluteBoneTransformsTo(matrixEnemy);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = matrixEnemy[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = proj;
                    effect.VertexColorEnabled = false;
                }
                mesh.Draw();
            }
        }
      
        #endregion
    }
        #endregion
}
