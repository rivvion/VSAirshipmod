using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSAirshipmod
{
    //public class EntityAirship : Entity, IRenderer, ISeatInstSupplier, IMountableListener
    public class EntityAirship : EntityBoat
    {
        public override double FrustumSphereRadius => base.FrustumSphereRadius * 2;
        public override bool IsCreature => true; // For RepulseAgents behavior to work

        // current forward speed
        public double ForwardSpeed = 0.0;

        // current turning speed (rad/tick)
        public double AngularVelocity = 0.0;



        //If you read this, hello traveler. The code below is responsible for the crasing of the game.... i'm joking. its just a variable that stores the Horizontal Velocity :)
        public double HorizontalVelocity = 0.0;
        private bool IsFlying => !OnGround;


        public double AngularVelocityDivider = 10;

        ModSystemBoatingSound modsysSounds;

        public override bool ApplyGravity => applyGravity;
        private bool applyGravity = true;

        public override bool IsInteractable
        {
            get { return true; }
        }


        public override float MaterialDensity
        {
            get { return 100f; }
        }

        double swimmingOffsetY;
        public override double SwimmingOffsetY
        {
            get { return swimmingOffsetY; }
        }

        /// <summary>
        /// The speed this boat can reach at full power
        /// </summary>
        public virtual float SpeedMultiplier { get; set; } = 1f;

        public double RenderOrder => 0;
        public int RenderRange => 999;


        public string CreatedByPlayername => WatchedAttributes.GetString("createdByPlayername");
        public string CreatedByPlayerUID => WatchedAttributes.GetString("createdByPlayerUID");



        public Dictionary<string, string> MountAnimations = new Dictionary<string, string>();
        bool requiresPaddlingTool;
        bool unfurlSails;

        ICoreClientAPI capi;

        public EntityAirship() { }

        public override void Initialize(EntityProperties properties, ICoreAPI api, long InChunkIndex3d)
        {
            swimmingOffsetY = properties.Attributes["swimmingOffsetY"].AsDouble();
            SpeedMultiplier = properties.Attributes["speedMultiplier"].AsFloat(1f);

            MountAnimations = properties.Attributes["mountAnimations"].AsObject<Dictionary<string, string>>();


            base.Initialize(properties, api, InChunkIndex3d);


            requiresPaddlingTool = properties.Attributes["requiresPaddlingTool"].AsBool(false);
            unfurlSails = properties.Attributes["unfurlSails"].AsBool(false);

            capi = api as ICoreClientAPI;
            if (capi != null)
            {
                capi.Event.RegisterRenderer(this, EnumRenderStage.Before, "boatsim");
                modsysSounds = api.ModLoader.GetModSystem<ModSystemBoatingSound>();
            }
        }

        public override void OnTesselation(ref Shape entityShape, string shapePathForLogging)
        {
            var shape = entityShape;

            if (unfurlSails)
            {
                var mountable = GetInterface<IMountable>();
                if (shape == entityShape) entityShape = entityShape.Clone();

                if (mountable != null && mountable.AnyMounted())
                {
                    entityShape.RemoveElementByName("SailFurled");
                }
                else
                {
                    entityShape.RemoveElementByName("SailUnfurled");
                }
            }

            base.OnTesselation(ref entityShape, shapePathForLogging);
        }


        float curRotMountAngleZ = 0f;
        public Vec3f mountAngle = new Vec3f();

        public override void OnRenderFrame(float dt, EnumRenderStage stage)
        {
            // Client side we update every frame for smoother turning
            if (capi.IsGamePaused) return;

            updateBoatAngleAndMotion(dt);

            long ellapseMs = capi.InWorldEllapsedMilliseconds;
            float forwardpitch = 0;
            if (IsFlying)//(Swimming)//
            {
                double gamespeed = capi.World.Calendar.SpeedOfTime / 60f;
                float intensity = 0.15f + GlobalConstants.CurrentWindSpeedClient.X * 0.9f;
                float diff = GameMath.DEG2RAD / 2f * intensity;
                mountAngle.X = GameMath.Sin((float)(ellapseMs / 1000.0 * 2 * gamespeed)) * 8 * diff;
                mountAngle.Y = GameMath.Cos((float)(ellapseMs / 2000.0 * 2 * gamespeed)) * 3 * diff;
                mountAngle.Z = -GameMath.Sin((float)(ellapseMs / 3000.0 * 2 * gamespeed)) * 8 * diff;

                curRotMountAngleZ += ((float)AngularVelocity * 5 * Math.Sign(ForwardSpeed) - curRotMountAngleZ) * dt * 5;
                forwardpitch = -(float)ForwardSpeed * 1.3f;
            }

            var esr = Properties.Client.Renderer as EntityShapeRenderer;
            if (esr == null) return;

            esr.xangle = mountAngle.X + curRotMountAngleZ;
            esr.yangle = mountAngle.Y;
            esr.zangle = mountAngle.Z + forwardpitch; // Weird. Pitch ought to be xangle.
        }


        public override void OnGameTick(float dt)
        {
            if (World.Side == EnumAppSide.Server)
            {
                var ela = World.ElapsedMilliseconds;
                if (IsOnFire && (World.ElapsedMilliseconds - OnFireBeginTotalMs > 10000))
                {
                    Die();
                }

                //ApplyGravityIfNotMounted();
                updateBoatAngleAndMotion(dt);

            }

            base.OnGameTick(dt);
        }


        public override void OnAsyncParticleTick(float dt, IAsyncParticleManager manager)
        {
            base.OnAsyncParticleTick(dt, manager);

            /*double disturbance = Math.Abs(ForwardSpeed) + Math.Abs(AngularVelocity);
            if (disturbance > 0.01)
            {
                float minx = -3f;
                float addx = 6f;
                float minz = -0.75f;
                float addz = 1.5f;

                EntityPos herepos = Pos;
                var rnd = Api.World.Rand;
                SplashParticleProps.AddVelocity.Set((float)herepos.Motion.X * 20, (float)herepos.Motion.Y, (float)herepos.Motion.Z * 20);
                SplashParticleProps.AddPos.Set(0.1f, 0, 0.1f);
                SplashParticleProps.QuantityMul = 0.5f * (float)disturbance;

                double y = herepos.Y - 0.15;

                for (int i = 0; i < 10; i++)
                {
                    float dx = minx + (float)rnd.NextDouble() * addx;
                    float dz = minz + (float)rnd.NextDouble() * addz;

                    double yaw = Pos.Yaw + GameMath.PIHALF + Math.Atan2(dx, dz);
                    double dist = Math.Sqrt(dx * dx + dz * dz);

                    SplashParticleProps.BasePos.Set(
                        herepos.X + Math.Sin(yaw) * dist,
                        y,
                        herepos.Z + Math.Cos(yaw) * dist
                    );

                    manager.Spawn(SplashParticleProps);
                }
            }*/
        }

        protected override void updateBoatAngleAndMotion(float dt)
        {
            // Ignore lag spikes
            dt = Math.Min(0.5f, dt);

            float step = GlobalConstants.PhysicsFrameTime;
            var motion = SeatsToMotion(step);

            // Add some easing to it
            ForwardSpeed += (motion.X * SpeedMultiplier - ForwardSpeed) * dt;
            AngularVelocity += (motion.Z * (SpeedMultiplier / AngularVelocityDivider) - AngularVelocity) * dt;
            HorizontalVelocity = motion.Y * dt;//+= (motion.Y * SpeedMultiplier - HorizontalVelocity) * dt;


            if (!IsFlying && HorizontalVelocity == 0) return;


            var pos = SidedPos;

            if (ForwardSpeed != 0.0)
            {
                var targetmotion = pos.GetViewVector().Mul((float)-ForwardSpeed).ToVec3d();
                pos.Motion.X = targetmotion.X;
                pos.Motion.Z = targetmotion.Z;
            }

            if (true)
            {
                if (HorizontalVelocity > 0.0)
                {
                    pos.Motion.Y = 0.013;
                }

                applyGravity = IsEmptyOfPlayers() ? true : false;

                if (HorizontalVelocity < 0.0 || (IsEmptyOfPlayers() && (!OnGround || !Swimming)))
                {
                    pos.Motion.Y = -0.013;
                }
            }


            var bh = GetBehavior<EntityBehaviorPassivePhysicsMultiBox>();
            bool canTurn = true;

            if (AngularVelocity != 0.0)
            {
                float yawDelta = (float)AngularVelocity * dt * 30f;

                if (bh.AdjustCollisionBoxesToYaw(dt, true, SidedPos.Yaw + yawDelta))
                {
                    pos.Yaw += yawDelta;
                }
                else canTurn = false;
            }
            else
            {
                canTurn = bh.AdjustCollisionBoxesToYaw(dt, true, SidedPos.Yaw);
            }

            if (!canTurn)
            {
                if (bh.AdjustCollisionBoxesToYaw(dt, true, SidedPos.Yaw - 0.1f))
                {
                    pos.Yaw -= 0.0002f;
                }
                else if (bh.AdjustCollisionBoxesToYaw(dt, true, SidedPos.Yaw + 0.1f))
                {
                    pos.Yaw += 0.0002f;
                }
            }

            pos.Roll = 0;
        }

        protected virtual bool HasPaddle(Entity entity)
        {
            if (!requiresPaddlingTool) return true;

            EntityAgent agent = entity as EntityAgent;
            if (agent == null) return false;

            if (agent.RightHandItemSlot == null || agent.RightHandItemSlot.Empty) return false;
            return agent.RightHandItemSlot.Itemstack.Collectible.Attributes?.IsTrue("paddlingTool") == true;
        }

        public virtual Vec3d SeatsToMotion(float dt)
        {
            int seatsRowing = 0;

            double linearMotion = 0;
            double angularMotion = 0;
            double horizontalMotion = 0;

            var bh = GetBehavior<EntityBehaviorSeatable>();
            bh.Controller = null;

            foreach (var sseat in bh.Seats)
            {
                var seat = sseat as EntityBoatSeat;
                if (seat.Passenger == null) continue;

                if (!(seat.Passenger is EntityPlayer))
                {
                    seat.Passenger.SidedPos.Yaw = SidedPos.Yaw;
                }
                if (seat.Config.BodyYawLimit != null && seat.Passenger is EntityPlayer eplr)
                {
                    eplr.BodyYawLimits = new AngleConstraint(Pos.Yaw + seat.Config.MountRotation.Y * GameMath.DEG2RAD, (float)seat.Config.BodyYawLimit);
                    eplr.HeadYawLimits = new AngleConstraint(Pos.Yaw + seat.Config.MountRotation.Y * GameMath.DEG2RAD, GameMath.PIHALF);
                }

                if (!seat.Config.Controllable || bh.Controller != null) continue;
                var controls = seat.controls;

                bh.Controller = seat.Passenger;

                if (!HasPaddle(seat.Passenger))
                {
                    seat.Passenger.AnimManager?.StopAnimation(MountAnimations["ready"]);
                    seat.actionAnim = null;
                    continue;
                }

                if (controls.Left == controls.Right)
                {
                    StopAnimation("turnLeft");
                    StopAnimation("turnRight");
                }
                if (controls.Left && !controls.Right)
                {
                    StartAnimation("turnLeft");
                    StopAnimation("turnRight");
                }
                if (controls.Right && !controls.Left)
                {
                    StopAnimation("turnLeft");
                    StartAnimation("turnRight");
                }

                //controls altitude (horizontal motion). its before tries to move, becouse tries to move ignores up and down motion and it was not working 
                if (controls.Jump || controls.Sprint)
                {
                    float dir = controls.Jump ? 1 : -1;

                    horizontalMotion += dir * dt * 2f;
                }

                if (!controls.TriesToMove)
                {
                    seat.actionAnim = null;
                    if (seat.Passenger.AnimManager != null && !seat.Passenger.AnimManager.IsAnimationActive(MountAnimations["ready"]))
                    {
                        seat.Passenger.AnimManager.StartAnimation(MountAnimations["ready"]);
                    }
                    continue;
                }
                else
                {
                    if (controls.Right && !controls.Backward && !controls.Forward)
                    {
                        seat.actionAnim = MountAnimations["backwards"];
                    }
                    else
                    {
                        seat.actionAnim = MountAnimations[controls.Backward ? "backwards" : "forwards"];
                    }

                    seat.Passenger.AnimManager?.StopAnimation(MountAnimations["ready"]);
                }

                float str = ++seatsRowing == 1 ? 1 : 0.5f;


                if (controls.Left || controls.Right)
                {
                    float dir = controls.Left ? 1 : -1;
                    angularMotion += str * dir * dt;
                }

                if (controls.Forward || controls.Backward)
                {
                    float dir = controls.Forward ? 1 : -1;

                    var yawdist = Math.Abs(GameMath.AngleRadDistance(SidedPos.Yaw, seat.Passenger.SidedPos.Yaw));
                    bool isLookingBackwards = yawdist > GameMath.PIHALF;

                    if (isLookingBackwards && requiresPaddlingTool) dir *= -1;

                    linearMotion += str * dir * dt * 2f;
                }


            }
            return new Vec3d(linearMotion, horizontalMotion, angularMotion);
        }

        public override void OnInteract(EntityAgent byEntity, ItemSlot itemslot, Vec3d hitPosition, EnumInteractMode mode)
        {
            if (mode == EnumInteractMode.Interact && AllowPickup() && IsEmpty())
            {
                if (tryPickup(byEntity, mode)) return;
            }

            EnumHandling handled = EnumHandling.PassThrough;
            foreach (EntityBehavior behavior in SidedProperties.Behaviors)
            {
                behavior.OnInteract(byEntity, itemslot, hitPosition, mode, ref handled);
                if (handled == EnumHandling.PreventSubsequent) break;
            }
        }

        private bool AllowPickup()
        {
            return Properties.Attributes?["rightClickPickup"].AsBool(false) == true;
        }

        private bool IsEmpty()
        {
            var bhs = GetBehavior<EntityBehaviorSeatable>();
            var bhr = GetBehavior<EntityBehaviorRideableAccessories>();
            return !bhs.AnyMounted() && (bhr == null || bhr.Inventory.Empty);
        }

        private bool IsEmptyOfPlayers()
        {
            var bhs = GetBehavior<EntityBehaviorSeatable>();
            //var bhr = GetBehavior<EntityBehaviorRideableAccessories>();
            return !bhs.AnyMounted();
        }

        private bool tryPickup(EntityAgent byEntity, EnumInteractMode mode)
        {
            // shift + click to remove boat
            if (byEntity.Controls.ShiftKey)
            {
                ItemStack stack = new ItemStack(World.GetItem(Code));
                if (!byEntity.TryGiveItemStack(stack))
                {
                    World.SpawnItemEntity(stack, ServerPos.XYZ);
                }

                Api.World.Logger.Audit("{0} Picked up 1x{1} at {2}.",
                    byEntity.GetName(),
                    stack.Collectible.Code,
                    Pos
                );

                Die();
                return true;
            }

            return false;
        }

        public override bool CanCollect(Entity byEntity)
        {
            return false;
        }


        public override WorldInteraction[] GetInteractionHelp(IClientWorldAccessor world, EntitySelection es, IClientPlayer player)
        {
            return base.GetInteractionHelp(world, es, player);
        }


        public override void OnEntityDespawn(EntityDespawnData despawn)
        {
            base.OnEntityDespawn(despawn);

            capi?.Event.UnregisterRenderer(this, EnumRenderStage.Before);
        }


        public void Dispose()
        {

        }

        public IMountableSeat CreateSeat(IMountable mountable, string seatId, SeatConfig config)
        {
            return new EntityBoatSeat(mountable, seatId, config);
        }

        public void DidUnnmount(EntityAgent entityAgent)
        {
            MarkShapeModified();
        }

        public void DidMount(EntityAgent entityAgent)
        {
            MarkShapeModified();
        }

        public override string GetInfoText()
        {
            string text = base.GetInfoText();
            if (CreatedByPlayername != null)
            {
                text += "\n" + Lang.Get("entity-createdbyplayer", CreatedByPlayername);
            }
            return text;
        }
    }
}