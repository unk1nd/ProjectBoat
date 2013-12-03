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
    class boat
    {
        private float x;
        private float y;
        private float boatRotY;
        public Vector3 boatPosition;
        public Model boatModel;
        private Texture2D boatTexture;
        //private IInputHandler input;

        public Texture2D BoatTexture
        {
            get { return boatTexture; }
            set { boatTexture = value; }
        }

        public Model BoatModel
        {
            get { return boatModel; }
            set { boatModel = value; }
        }

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

        public float BoatRotY
        {
            get { return boatRotY; }
            set { boatRotY = value; }
        }

        public Vector3 BoatPosition 
        {
            get { return boatPosition; }
            set { boatPosition = value; }
        }

        public boat(float boatRotY, Vector3 boatPosition)
        {
            
            
            this.boatRotY = boatRotY;
            this.boatPosition = boatPosition;
            //this.boatTexture = boatTexture;
            //this.boatModel = boatModel;
            
        }

    }
}
