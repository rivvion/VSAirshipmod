using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

using Vintagestory.API.Util;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;

namespace VSAirshipmod
{
    public class EntityAirship : Entity, IRenderer
    {
        public double RenderOrder => 0;
        public int RenderRange => 999;



        public void Dispose()
        {
            
        }

        public virtual void OnRenderFrame(float dt, EnumRenderStage stage)
        {
            // Client side we update every frame for smoother turning
            //if (capi.IsGamePaused) return;

            //updateBoatAngleAndMotion(dt);

            /*long ellapseMs = capi.InWorldEllapsedMilliseconds;
            float forwardpitch = 0;
            if (Swimming)
            {
                double gamespeed = capi.World.Calendar.SpeedOfTime / 60f;
                float intensity = 0.15f + GlobalConstants.CurrentWindSpeedClient.X * 0.9f;
                float diff = GameMath.DEG2RAD / 2f * intensity;
                mountAngle.X = GameMath.Sin((float)(ellapseMs / 1000.0 * 2 * gamespeed)) * 8 * diff;
                mountAngle.Y = GameMath.Cos((float)(ellapseMs / 2000.0 * 2 * gamespeed)) * 3 * diff;
                mountAngle.Z = -GameMath.Sin((float)(ellapseMs / 3000.0 * 2 * gamespeed)) * 8 * diff;

                curRotMountAngleZ += ((float)AngularVelocity * 5 * Math.Sign(ForwardSpeed) - curRotMountAngleZ) * dt * 5;
                forwardpitch = -(float)ForwardSpeed * 1.3f;
            }*/

            //var esr = Properties.Client.Renderer as EntityShapeRenderer;
            //if (esr == null) return;

            //esr.xangle = mountAngle.X + curRotMountAngleZ;
            //esr.yangle = mountAngle.Y;
            //esr.zangle = mountAngle.Z + forwardpitch; // Weird. Pitch ought to be xangle.
        }
    }
}
