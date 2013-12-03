using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using Microsoft.Xna.Framework.Content;
using MyLibrary;

namespace ProjectBoat
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class CubeComponent : Microsoft.Xna.Framework.DrawableGameComponent
    {
        private GraphicsDeviceManager graphics;
        private GraphicsDevice device;

        //Liste med vertekser:
        private VertexPositionColor[] vertices;
        private VertexPositionColor[] verticesTop;

        private BasicEffect effect;
       // private VertexDeclaration mVertPosColor;

        //WVP-matrisene:
        private Matrix world;

        public Matrix World
        {
            get { return world; }
            set { world = value; }
        }

        private FirstPersonCamera camera;

        private BoundingBox cubeBoundingBox;

        public BoundingBox CubeBoundingBox
        {
            get { return cubeBoundingBox; }
            set { cubeBoundingBox = value; }
        }

        public CubeComponent(Game game)
            : base(game)
        {
            camera = (FirstPersonCamera)((ICamera)game.Services.GetService(typeof(ICamera)));
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            device = graphics.GraphicsDevice;

            //Effect-objektet for komponenten:
            effect = new BasicEffect(graphics.GraphicsDevice);

            effect.VertexColorEnabled = true;
            InitVertices();

            base.Initialize();
        }

        /// <summary>
        /// Vertekser for trekanten.
        /// </summary>
        private void InitVertices()
        {
            vertices = new VertexPositionColor[10];
            vertices[0].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[0].Color = Color.Red;

            vertices[1].Position = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices[1].Color = Color.Red;

            vertices[2].Position = new Vector3(0.5f, -0.5f, 0.5f);
            vertices[2].Color = Color.Red;

            vertices[3].Position = new Vector3(0.5f, 0.5f, 0.5f);
            vertices[3].Color = Color.Red;

            vertices[4].Position = new Vector3(0.5f, -0.5f, -0.5f);
            vertices[4].Color = Color.Red;

            vertices[5].Position = new Vector3(0.5f, 0.5f, -0.5f);
            vertices[5].Color = Color.Red;

            vertices[6].Position = new Vector3(-0.5f, -0.5f, -0.5f);
            vertices[6].Color = Color.Red;

            vertices[7].Position = new Vector3(-0.5f, 0.5f, -0.5f);
            vertices[7].Color = Color.Red;

            vertices[8].Position = new Vector3(-0.5f, -0.5f, 0.5f);
            vertices[8].Color = Color.Red;

            vertices[9].Position = new Vector3(-0.5f, 0.5f, 0.5f);
            vertices[9].Color = Color.Red;

            //Toppen av kuben:
            verticesTop = new VertexPositionColor[4];
            verticesTop[0].Position = new Vector3(-0.5f, 0.5f, 0.5f);
            verticesTop[0].Color = Color.Green;

            verticesTop[1].Position = new Vector3(-0.5f, 0.5f, -0.5f);
            verticesTop[1].Color = Color.Green;

            verticesTop[2].Position = new Vector3(0.5f, 0.5f, 0.5f);
            verticesTop[2].Color = Color.Green;

            verticesTop[3].Position = new Vector3(0.5f, 0.5f, -0.5f);
            verticesTop[3].Color = Color.Green;

            //Deklarerer kubens BoundinBox:
            Vector3[] cubePoints = new Vector3[2];
            cubePoints[0] = vertices[6].Position; //Bakre venstre hjørne
            cubePoints[1] = vertices[3].Position; //Fremste høyre hjørne
            cubeBoundingBox = BoundingBox.CreateFromPoints(cubePoints);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            effect.Projection = camera.Projection;
            effect.View = camera.View;

            RasterizerState rasterizerState1 = new RasterizerState();
            rasterizerState1.CullMode = CullMode.None;
            rasterizerState1.FillMode = FillMode.Solid;
            device.RasterizerState = rasterizerState1;

            // Setter World-matrisa:
            world = Matrix.CreateScale(1.2f, 1.2f, 1.2f) * Matrix.CreateRotationY(MathHelper.Pi / 5) * Matrix.CreateTranslation(4.0f, 0.0f, -2.0f);
            effect.World = world;

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, vertices, 0, 8);
                device.DrawUserPrimitives(PrimitiveType.TriangleStrip, verticesTop, 0, 2);
            }
            base.Draw(gameTime);
        }
    }
}