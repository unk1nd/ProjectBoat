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
    class Camera : Microsoft.Xna.Framework.GameComponent
    {
        private IInputHandler input;
        private GraphicsDeviceManager graphics;

        private Matrix projection;
        private Matrix view;

        public static float x = 0.0f;
        public static float y = 1.0f;
        public static float z = 0.0f;

        public Vector3 thirdPersonReference = new Vector3(0, 10, -10);


        public float X
        {
            get { return x; }
            set { x = value; }
        }

        public float Y
        {
            get { return y; }
            set { y = value; }
        }

        public float Z
        {
            get { return z; }
            set { z = value; }
        }

        public Vector3 camPos = new Vector3(x, y, z);
        private Vector3 camTar = Vector3.Zero;
        private Vector3 camUpVec = Vector3.Up;

        private Vector3 camRef = new Vector3(0.0f, 0.0f, -1.0f);

        private float camYaw = 0.0f;
        private float camPitch = 0.0f;

        private const float spinRate = 40.0f;

        public Vector3 CamPos
        {
            get { return camPos; }
            set
            {
                camPos = value;
                camRef = ((-1.0f) * camPos);
                camRef.Normalize();
            }
        }

        public Vector3 CamTar
        {
            get { return camTar; }
            set { camTar = value; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; }
        }

        public Camera(Game game)
            : base(game)
        {
            graphics = (GraphicsDeviceManager)Game.Services.GetService(typeof(IGraphicsDeviceManager));
            input = (IInputHandler)Game.Services.GetService(typeof(IInputHandler));
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            // TODO: Add your initialization code here

            base.Initialize();
            this.InitializeCam();
            //camRef = ((-1.0f) * camPos);

            
        }

        private void InitializeCam()
        {
            float aspecRatio = (float)graphics.GraphicsDevice.Viewport.Width / (float)graphics.GraphicsDevice.Viewport.Height;
            Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspecRatio, 1.0f, 5000.0f, out projection);
            Matrix.CreateLookAt(ref camPos, ref camTar, ref camUpVec, out view);


        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            
            /*
            float timeDelta = (float)gameTime.ElapsedGameTime.TotalSeconds;
            GamePadState padState1 = GamePad.GetState(PlayerIndex.One);

            if (padState1.IsConnected)
            {
                if (padState1.ThumbSticks.Right.Y != 0.0f)
                {
                    if (padState1.ThumbSticks.Right.Y > 0.0f)
                    {
                        if (x > 100.0f)
                        {
                            x = x - 0.1f;
                            camPos = new Vector3(x, y, z);
                        }
                    }

                    if (padState1.ThumbSticks.Right.Y < 0.0f)
                    {
                        if (x < 1000.0f)
                        {
                            x = x + 0.1f;
                            camPos = new Vector3(x, y, z);
                        }
                    }
                }

                if (padState1.ThumbSticks.Right.X != 0.0f)
                {
                    if (padState1.ThumbSticks.Right.X > 0.0f)
                    {
                        if (z > -1000.0f)
                        {
                            z = z - 0.1f;
                            camPos = new Vector3(x, y, z);
                        }
                    }

                    if (padState1.ThumbSticks.Right.X < 0.0f)
                    {
                        if (z < 1000.0f)
                        {
                            z = z + 0.1f;
                            camPos = new Vector3(x, y, z);
                        }
                    }
                }

                if (padState1.ThumbSticks.Left.Y != 0.0f)
                {
                    if (padState1.ThumbSticks.Left.Y > 0.0f)
                    {
                        if (y < 1000.0f)
                        {
                            y = y + 0.1f;
                            camPos = new Vector3(x, y, z);
                        }
                    }

                    if (padState1.ThumbSticks.Left.Y < 0.0f)
                    {
                        if (y > 0.0f)
                        {
                            y = y - 0.1f;
                            camPos = new Vector3(x, y, z);
                        }
                    }
                }

            }


            if (input.KeyboardState.IsKeyDown(Keys.Up))
            {
                
                    x = x - 0.1f;
                    camPos = new Vector3(x, y, z);
                
            }
            if (input.KeyboardState.IsKeyDown(Keys.Down))
            {
                
                    x = x + 0.1f;
                    camPos = new Vector3(x, y, z);
                

            }

            if (input.KeyboardState.IsKeyDown(Keys.S))
            {
                if (y > 0.0f)
                {
                    y = y - 0.1f;
                    camPos = new Vector3(x, y, z);
                }
            }
            if (input.KeyboardState.IsKeyDown(Keys.W))
            {
                if (y < 1000.0f)
                {
                    y = y + 0.1f;
                    camPos = new Vector3(x, y, z);
                }
            }

            if (input.KeyboardState.IsKeyDown(Keys.D))
            {
                if (z > -1000.0f)
                {
                    z = z - 0.1f;
                    camPos = new Vector3(x, y, z);
                }
            }
            if (input.KeyboardState.IsKeyDown(Keys.A))
            {
                if (z < 1000.0f)
                {
                    z = z + 0.1f;
                    camPos = new Vector3(x, y, z);
                }
            }
            */
            Matrix rotMat;
            Matrix.CreateRotationY(MathHelper.ToRadians(camYaw), out rotMat);

            rotMat = Matrix.CreateRotationX(MathHelper.ToRadians(camPitch)) * rotMat;

            Vector3 transRef;
            Vector3.Transform(ref camRef, ref rotMat, out transRef);

            Vector3.Add(ref camPos, ref transRef, out camTar);

            Matrix.CreateLookAt(ref camPos, ref camTar, ref camUpVec, out view);

            Matrix rotationMatrix = Matrix.CreateRotationY(camYaw);
            Vector3 transformedReference = Vector3.Transform(thirdPersonReference, rotationMatrix);
            Vector3 cameraPosition = transformedReference + camPos;

            view = Matrix.CreateLookAt(cameraPosition, camPos,
            new Vector3(0.0f, 1.0f, 0.0f));

            Viewport viewport = graphics.GraphicsDevice.Viewport;
            float aspectRatio = (float)viewport.Width / (float)viewport.Height;

           

            base.Update(gameTime);
        }
    }
}