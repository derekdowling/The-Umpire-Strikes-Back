using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;

namespace NLAS_game
{
    class Bat : PhysicsBox
    { 
        public static Texture2D batTexture; 

        public Bat(World physicsWorld, float box_width, float box_height,
            float positionX, float positionY, float rotation, float density)
            : base(batTexture, physicsWorld, box_width, box_height,
            positionX, positionY, rotation, density)
        {
        }
    }
}
