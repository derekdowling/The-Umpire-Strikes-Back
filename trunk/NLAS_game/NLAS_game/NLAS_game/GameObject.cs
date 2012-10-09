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
    public class GameObject
    {
        public Vector2 position;
        public float rotation;
        public Vector2 scale;
        public Vector2 origin;
        public Texture2D spriteTexture;

        public bool deleteMePlease;

        public GameObject(Texture2D sprite_texture)
        {
            this.deleteMePlease = false;
            this.position = new Vector2(0,0);
            this.rotation = 0;
            this.scale = new Vector2(1,1);
            this.spriteTexture = sprite_texture;
            this.origin = new Vector2(0, 0);
            if (this.spriteTexture != null)
            {
                this.origin = new Vector2(this.spriteTexture.Width / 2, this.spriteTexture.Height / 2);
            }
        }

        public void scaleToFitTheseDimensions(float width, float height)
        {
            if (this.spriteTexture != null)
            {
                this.scale.X = width / (float)this.spriteTexture.Width;
                this.scale.Y = height/ (float)this.spriteTexture.Height;
            }
        }
        
        /// <summary>
        /// Get a bounding box for the game object
        /// </summary>
        /// <returns></returns>
        public Rectangle getBounds()
        {
            int width = (int)(this.scale.X * (float)this.spriteTexture.Width);
            int height = (int)(this.scale.Y * (float)this.spriteTexture.Height); 
            return new Rectangle((int)(this.position.X - width/2), (int)(this.position.Y - height/2), width, height);
        }

        public virtual void update(GameTime gameTime)
        {

        }

        public virtual void draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (spriteTexture != null)
            {
                spriteBatch.Draw(this.spriteTexture, this.position, null, Color.White, this.rotation, this.origin, this.scale, SpriteEffects.None, 0.0f);
            }
            else
            {
                Debug.WriteLine("WARNING! Trying to render a gameObject that is missing a texture!");
            }
        }

        public virtual void draw(SpriteBatch spriteBatch, GameTime gameTime, Color color)
        {
            if (spriteTexture != null)
            {
                spriteBatch.Draw(this.spriteTexture, this.position, null, color, this.rotation, this.origin, this.scale, SpriteEffects.None, 0.0f);
            }
            else
            {
                Debug.WriteLine("WARNING! Trying to render a gameObject that is missing a texture!");
            }
        }
    }
}
