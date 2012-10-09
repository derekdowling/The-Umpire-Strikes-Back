using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using Box2D.XNA;
using System.Diagnostics;

namespace NLAS_game
{
    public abstract class DynamicPhysicsGameObject : GameObject
    {
        public static float ScreenPixelsPerMeter = 10.0f; //(P/M)

        public Body physicsBody;

        public DynamicPhysicsGameObject(Texture2D sprite_texture)
            : base(sprite_texture)
        {
        }

        public override void update(GameTime gameTime)
        {
            if (this.physicsBody != null && this.spriteTexture != null)
            {
                this.position = this.physicsBody.GetPosition() * ScreenPixelsPerMeter;
                this.rotation = this.physicsBody.GetAngle();
            }
        }

        public void deleteFromPhysicsSimulation(World physicsWorld)
        {
            physicsWorld.DestroyBody(this.physicsBody);
            this.deleteMePlease = true;
        }

        public void setVelocity(Vector2 velocity)
        {
            this.physicsBody.SetLinearVelocity(velocity/ScreenPixelsPerMeter);
        }
    }
}
