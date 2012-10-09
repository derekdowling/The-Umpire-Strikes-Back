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
    public class CashRewardEffect: GameObject
    {
        public static Texture2D hitSprite;
        public static float effectHeight = 48;
        public static float effectWidth = 48;

        public double lifeSpan = 1.1;
        public double floatSpeed = 4.9f;
        double spawnedTime;

        public CashRewardEffect(Vector2 location, GameTime spawnTime)
            : base(hitSprite)
        {
            this.position = location;
            this.scaleToFitTheseDimensions(effectWidth, effectHeight);
            spawnedTime = spawnTime.TotalGameTime.TotalSeconds;
        }

        public override void update(GameTime gameTime)
        {
            this.position.Y -= (float)(gameTime.ElapsedGameTime.TotalSeconds * floatSpeed);
            if (gameTime.TotalGameTime.TotalSeconds > spawnedTime + lifeSpan)
            {
                this.deleteMePlease = true;
            }
            base.update(gameTime);
        }
    }
}
