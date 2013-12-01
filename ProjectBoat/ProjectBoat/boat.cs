using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ProjectBoat
{
    class boat
    {
        private double x;
        private double y;
        private float boatRotY;
        private Vector3 boatPosition;
        private Model boatModel;
        private Texture2D boatTexture;

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

        public double X
        {
            get { return x; }
            set { x = value; }
        }

        public double Y
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

        public boat(double x, double y, float boatRotY, Vector3 boatPosition, Texture2D boatTexture)
        {
            
            this.x = x;
            this.y = y;
            this.boatRotY = boatRotY;
            this.boatPosition = boatPosition;
            this.boatTexture = boatTexture;
            
        }
    }
}
