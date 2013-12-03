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

        #region Variabler
        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;
        //private Camera camera;
        private ThirdPersonCamera camera;
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
        //private float heroRot;
        private Vector3 modelVelocity = Vector3.Zero;

        
        private Quaternion heroRot = Quaternion.Identity;

        // kolisjons stuff
        private CubeComponent cube;
        
        //Modeller:
        private Model modelHERO;
        private Model modelEnemy;

        //Materiser som holder "akkumulerte" bone-transformasjonene:
        private Matrix[] matrixHERO;
        private Matrix[] matrixEnemy;

        //Tar vare på opprinnelig Bone-transformasjoner:
        private Matrix[] originalTransforms1;
        private Matrix[] originalTransforms2;
        
        // kamera
        private const float CAMERA_FOVX = 80.0f;
        private const float CAMERA_ZFAR = 100f * 3.0f;
        private const float CAMERA_ZNEAR = 1.0f;
        private const float CAMERA_MAX_SPRING_CONSTANT = 100.0f;
        private const float CAMERA_MIN_SPRING_CONSTANT = 1.0f;

        private VertexBuffer vertexBuffer;
        private Texture2D vann;

        private const float FLOOR_WIDTH = 9999.0f;
        private const float FLOOR_HEIGHT = 9999.0f;
        private const float FLOOR_TILE_U = 4.0f;
        private const float FLOOR_TILE_V = 4.0f;

        /*
        private Vector3 camTar = Vector3.Zero;
        private Vector3 camUpVec = Vector3.Up;
        private Vector3 camPos = new Vector3(1f, 2f, 0f);
        */

        private bool isCollition = false;
        #endregion


        public bool isFullScreen = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            input = new InputHandler(this);
            this.Components.Add(input);

            /*
            camera = new Camera(this);
            this.Components.Add(camera);
            */

            this.IsFixedTimeStep = true;
        }

        private void initDevice()
        {
            device = graphics.GraphicsDevice;
            graphics.PreferredBackBufferWidth = 1400;
            graphics.PreferredBackBufferHeight = 800;
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
            // Setup the camera.         
            camera = new ThirdPersonCamera();
            camera.Perspective(CAMERA_FOVX, (float)1400 / (float)800, CAMERA_ZNEAR, CAMERA_ZFAR);
            camera.LookAt(new Vector3(0.0f, 2 * 3.0f, 2 * -7.0f), Vector3.Zero, Vector3.Up);

        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            loadBoatObjects();

            // loading of font used for camera info
            font = Content.Load<SpriteFont>("font");
            effect2 = Content.Load<Effect>(@"Effects\blinn_phong");
            vann = Content.Load<Texture2D>(@"Textures\tex_water");
        }


        #region kolisjon

        /// <summary>
        /// Laster angitt modell og beregner "global" boundingsphere.
        /// </summary>
        /// <param name="modelName"></param>
        /// <param name="matrix"></param>
        /// <param name="originalTransforms"></param>
        /// <returns></returns>
        private Model LoadModelWithBoundingSphere(String modelName, ref Matrix[] matrix, ref Matrix[] originalTransforms)
        {
            Model model = Content.Load<Model>(modelName);
            matrix = new Matrix[model.Bones.Count];

            //Denne sørger for at absolutte transformasjonsmatriser for hver
            //ModelMesh legges inn i matrisetabellen:
            model.CopyAbsoluteBoneTransformsTo(matrix);

            //Komplett omsluttende BoundingSphere:
            BoundingSphere completeBoundingSphere = new BoundingSphere();

            //Gjennomløper alle del-sfærer:
            foreach (ModelMesh mesh in model.Meshes)
            {
                BoundingSphere origMeshSphere = mesh.BoundingSphere;
                //denne må transformeres i forhold til sitt Bone:
                origMeshSphere = XNAUtils.TransformBoundingSphere(origMeshSphere, matrix[mesh.ParentBone.Index]);

                completeBoundingSphere = BoundingSphere.CreateMerged(completeBoundingSphere, origMeshSphere);

            }
            model.Tag = completeBoundingSphere;

            //completeBoundingSphere inneholder nå en komplett omsluttende sfære for hele modellen. 
            //Tar vare på relative transformasjonsmatriser (for animasjon):
            originalTransforms = new Matrix[model.Bones.Count];
            model.CopyBoneTransformsTo(originalTransforms);
            return model;
        }

        /// <summary>
        /// Returnerer true dersom tankModel2 overlapper kuben.
        /// </summary>
        /// <param name="model2"></param>
        /// <param name="world2"></param>
        /// <returns></returns>
        bool CubeCollide(Model _model, Matrix _world)
        {
            BoundingSphere origSphere2 = (BoundingSphere)_model.Tag;
            BoundingSphere sphere2 = XNAUtils.TransformBoundingSphere(origSphere2, _world);

            BoundingBox origCubeBox = cube.CubeBoundingBox;
            BoundingBox cubeBox = XNAUtils.TransformBoundingBox(origCubeBox, cube.World);

            bool collision = sphere2.Intersects(cubeBox);
            return collision;
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

            // modelTank1 = this.LoadModelWithBoundingSphere(@"Content/Models/tank", ref matrixTank1, ref originalTransforms1);


            // hero
            modelHERO = this.LoadModelWithBoundingSphere("Models/HeroShip", ref matrixHERO, ref originalTransforms1);
            HERO = new boat(10, 10, 10, new Vector3(10, 10, 10));
            HERO.BoatModel = modelHERO;//Content.Load<Model>("Models/HeroShip");  // HeroBoat Model
            (HERO.BoatModel.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();


            // enemy boats
            boatArray[0] = new boat(-7, 0, 20, new Vector3(-7, 0, 15));
            boatArray[1] = new boat(-8, 0, 50, new Vector3(-8, 0, 10));
            boatArray[2] = new boat(-9, 0, 180, new Vector3(-9, 0, 5));

            // array for setting enemy boats models
            for (int i = 0; i < boatArray.Length; i++) 
            {
                modelEnemy = this.LoadModelWithBoundingSphere("Models/EnemyShip", ref matrixEnemy, ref originalTransforms2);
                boatArray[i].BoatModel = modelEnemy; //Content.Load<Model>("Models/EnemyShip");
                (boatArray[i].BoatModel.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
            }

            // load world model and load texture
            worldTemp = Content.Load<Model>("Models/World_Total");
            (worldTemp.Meshes[0].Effects[0] as BasicEffect).EnableDefaultLighting();
            (worldTemp.Meshes[0].Effects[0] as BasicEffect).Texture = Textures[1];
            (worldTemp.Meshes[0].Effects[0] as BasicEffect).TextureEnabled = true;

        }
        
        protected override void UnloadContent()
        {
        
        }

        #region update
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            // Get some input.
            UpdateInput();

            if (!isCollition)
            {
                // Add velocity to the current position.
                heroPosition += modelVelocity;
            }   
            // Bleed off velocity over time.
            modelVelocity *= 0.95f;

            //camera.Rotate((forward > 0.0f) ? heading : -heading, 0.0f);
            camera.LookAt(heroPosition);
            camera.Update(gameTime);
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
               // heroRot = heroRot + turn;
                leftRightRot -= turn;           
            }

            if (input.KeyboardState.IsKeyDown(Keys.D))
            {
                //heroRot = heroRot - 0.1f; 
                float turn = 0.1f;
                // heroRot = heroRot + turn;
                leftRightRot += turn;       
            }
            float upDownRot = 0;
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), leftRightRot) * Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), upDownRot);
            heroRot *= additionalRot;            

        }
        #endregion

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            if (!isCollition)
            {
                Vector3 addVector = Vector3.Transform(new Vector3(0, 0, +2), rotationQuat);
                position += addVector * speed;
            }
            else isCollition = false;
        }

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
            Vector3 campos = new Vector3(0, 5, -10);

            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(heroRot));
            campos += heroPosition;
            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(heroRot));
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
            //rasterizerState1.FillMode = FillMode.WireFrame;
            device.RasterizerState = rasterizerState1;

            effect.World = Matrix.Identity;
            effect.Projection = proj;
            effect.View = view;
            DrawFloor();

            Matrix worldMatrix = Matrix.CreateScale(0.0005f, 0.0005f, 0.0005f) * Matrix.CreateRotationY(MathHelper.Pi) * Matrix.CreateFromQuaternion(heroRot) * Matrix.CreateTranslation(heroPosition);

            /*
            effect.LightingEnabled = true;
            effect.DirectionalLight0.Enabled = true;
            effect.DirectionalLight0.DiffuseColor = Color.Yellow.ToVector3();
            effect.DirectionalLight0.Direction = Vector3.Normalize(new Vector3(-1.0f, -1.5f, 0.0f));
            effect.EmissiveColor = Color.Red.ToVector3();
            effect.DirectionalLight0.Enabled = false;
            */

            Matrix worldHERO, worldEnemy;
            this.DrawWorld(gameTime);
            this.DrawHERO1(modelHERO, out worldHERO, gameTime);
            this.DrawBoats(modelEnemy, out worldEnemy, gameTime, boatArray[1]);

            if (this.ModelsCollide(modelHERO, worldHERO, modelEnemy, worldEnemy))
            {
                spriteBatch.Begin();
                spriteBatch.DrawString(font, "COLLISJON!!!! ", new Vector2(0.0f, 50), Color.Red);
                spriteBatch.End();

                Vector3 modelVelocityAdd = Vector3.Zero;
                modelVelocityAdd *= (float)-0.111f;
                modelVelocity += modelVelocityAdd;
                // Add velocity to the current position.
                heroPosition -= modelVelocity;

                isCollition = true;
            }
            else 
            {
                isCollition = false;
            }
            

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Boat Info: " + heroPosition + "\n heroRot: " + heroRot, new Vector2(0.0f, 1), Color.WhiteSmoke);
            spriteBatch.End();


            UpdateCamera();

            base.Draw(gameTime);
        }

        #region Draw Verden

        private void DrawHERO1(Model model, out Matrix world, GameTime gameTime)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            //rasterizerState1.FillMode = FillMode.WireFrame;
            device.RasterizerState = rasterizerState1;

            // 1: Deklarerere hjelpematriser (for world-transformasjonen):
            Matrix matIdent, matScale, matRotY, matTransl;

            // 2: Initialiser matrisene:
            matIdent = Matrix.Identity;
            matScale = Matrix.CreateScale(0.005f);
            //matTransl = Matrix.CreateTranslation(0.0f, 0.0f, 0.0f);
            matTransl = Matrix.CreateTranslation(heroPosition);


            matRotY = Matrix.CreateFromQuaternion(heroRot);
            

            //3. Bygg world-matrisa:
            world = matIdent * matScale * matRotY * matTransl;

            //NB! denne må med siden vi har endret på noen av transformasjonsmatrisene (knyttet til kanonen, tårnet m.m.):
            model.CopyAbsoluteBoneTransformsTo(matrixHERO);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = matrixHERO[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = proj;
                    // 4b. Lys & action:
                    //effect.EnableDefaultLighting();
                    //effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                //5. Tegn ModelMesh-objektet i korrekt posisjon i forhold til
                //   transformasjonene satt i forrige steg.
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
            worldTemp.Draw(world, view, proj);
        }
        #endregion

        #region DrawBoat

        private void DrawBoats(Model model, out Matrix world, GameTime gameTime, boat b)
        {
            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            //rasterizerState1.FillMode = FillMode.WireFrame;
            device.RasterizerState = rasterizerState1;

            // 1: Deklarerere hjelpematriser (for world-transformasjonen):
            Matrix matIdent, matScale, matRotY, matRotZ, matTransl;

            // 2: Initialiser matrisene:
            matIdent = Matrix.Identity;
            matScale = Matrix.CreateScale(0.005f);
            matTransl = Matrix.CreateTranslation(b.BoatPosition);
            matRotY = Matrix.CreateRotationY(b.BoatRotY);
            matRotZ = Matrix.Identity; // CreateRotationZ(0.0f);

            //Endrer på enkelte av Bone-matrisene for å gjøre individuelle ModeMesh-transformasjoner:

            //3. Bygg world-matrisa:
            world = matIdent * matScale * matRotZ * matRotY * matTransl;

            //NB! denne må med siden vi har endret på noen av transformasjonsmatrisene (knyttet til kanonen, tårnet m.m.):
            model.CopyAbsoluteBoneTransformsTo(matrixEnemy);

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = matrixEnemy[mesh.ParentBone.Index] * world;
                    effect.View = view;
                    effect.Projection = proj;
                    // 4b. Lys & action:
                    //effect.EnableDefaultLighting();
                    //effect.LightingEnabled = true;
                    effect.VertexColorEnabled = false;
                }
                //5. Tegn ModelMesh-objektet i korrekt posisjon i forhold til
                //   transformasjonene satt i forrige steg.
                mesh.Draw();
            }
        }
        /*
        private void DrawBoats(GameTime gameTime, boat b)
        {
            Matrix matScale, matRotateY, matTrans;

            matScale = Matrix.CreateScale(0.005f);
            matTrans = Matrix.CreateTranslation(b.boatPosition);

            matRotateY = Matrix.CreateRotationY(b.BoatRotY);

            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;
           
            //b.BoatPosition = Matrix.Invert(world).Translation;
            b.BoatModel.Draw(world, camera.ViewMatrix, camera.ProjectionMatrix);

        }
        /*
        private void DrawHero(GameTime gameTime)
        {
            Matrix matScale, matRotateY, matTrans;

            matScale = Matrix.CreateScale(0.005f);
            matTrans = Matrix.CreateTranslation(heroPosition);

            matRotateY = Matrix.CreateRotationY(heroRot);

            world = matScale * matRotateY * matTrans;
            matrixStrack.Push(world);

            effect.World = world;

            //b.BoatPosition = Matrix.Invert(world).Translation;
            HERO.BoatModel.Draw(world, camera.ViewMatrix, camera.ProjectionMatrix);

        }
        */
        #endregion
    }
        #endregion
}
