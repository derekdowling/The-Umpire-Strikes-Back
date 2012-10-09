using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Box2D.XNA;
using Microsoft.Xna.Framework;

namespace NLAS_game
{
    public class Baseball : PhysicsCircle
    {
        public static Texture2D baseballTexture;

        public Baseball(World physicsWorld, float radius, float density, float positionX, float positionY, Vector2 initialVelocity)
            :base(Baseball.baseballTexture, physicsWorld, radius, positionX, positionY, 0, density)
        {
            this.setVelocity(initialVelocity);
        }
    }
}
