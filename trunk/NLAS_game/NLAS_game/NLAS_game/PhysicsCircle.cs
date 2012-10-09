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
    public class PhysicsCircle : DynamicPhysicsGameObject
    {
         public PhysicsCircle(Texture2D sprite_texture, World physicsWorld, float radius,
            float positionX, float positionY, float rotation, float density)
            : base(sprite_texture)
        {
            this.scaleToFitTheseDimensions(radius*2.0f, radius*2.0f);
            this.position.X = positionX;
            this.position.Y = positionY;
            this.rotation = rotation;

            BodyDef dynamicBodyDef = new BodyDef();
            dynamicBodyDef.type = BodyType.Dynamic;
            dynamicBodyDef.position = new Vector2(positionX / ScreenPixelsPerMeter, positionY / ScreenPixelsPerMeter);
            dynamicBodyDef.linearDamping = 0.0f;
            Body dynamicBody = physicsWorld.CreateBody(dynamicBodyDef);

            CircleShape dynamicCircleShape = new CircleShape();
            dynamicCircleShape._radius = radius/DynamicPhysicsGameObject.ScreenPixelsPerMeter;
            //dynamicBoxShape.
            //dynamicBoxShape.SetAsBox(box_width / 2.0f, box_height / 2.0f);   //experiment with / 2.0f
            FixtureDef dynamicCircleFixtureDef = new FixtureDef();
            dynamicCircleFixtureDef.shape = dynamicCircleShape;
            dynamicCircleFixtureDef.density = density;
            dynamicCircleFixtureDef.friction = 0.3f;
            
            dynamicBody.CreateFixture(dynamicCircleFixtureDef);

            this.physicsBody = dynamicBody;
        }

    }
}
