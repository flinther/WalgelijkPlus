using System.Numerics;
using Walgelijk;
using Walgelijk.AssetManager;
using Walgelijk.AssetManager.Deserialisers;
using Walgelijk.CommonAssetDeserialisers;
using Walgelijk.CommonAssetDeserialisers.Audio;
using Walgelijk.CommonAssetDeserialisers.Audio.Qoa;
using Walgelijk.OpenTK;
using Walgelijk.SimpleDrawing;
using WalgelijkPlus;
using WalgelijkPlus.Graphics;
using WalgelijkPlus.Physics;

namespace Pong
{
    public class Program
    {
        public static Game Game = null!;

        public static void Main(string[] args)
        {
            Game = new Game(
                new OpenTKWindow("Pong", new Vector2(-1), new Vector2(1280, 720)),
                new OpenALAudioRenderer()
            );

            Game.UpdateRate = 120;
            Game.FixedUpdateRate = 60;
            Game.Window.VSync = false;

            TextureLoader.Settings.FilterMode = FilterMode.Linear;

            AssetDeserialisers.Register(new OggFixedAudioDeserialiser());
            AssetDeserialisers.Register(new OggStreamAudioDeserialiser());
            AssetDeserialisers.Register(new QoaFixedAudioDeserialiser());
            AssetDeserialisers.Register(new WaveFixedAudioDeserialiser());
            AssetDeserialisers.Register(new FontDeserialiser());

            foreach (var a in Directory.EnumerateFiles("assets", "*.waa"))
                Assets.RegisterPackage(a);

            var scene = Game.Scene = new Scene(Game);
            scene.AddSystem(new CameraSystem());
            scene.AddSystem(new TransformSystem());
            scene.AddSystem(new ExampleSystem());
            scene.AddSystem(new Physics2DSystem());
            scene.AddSystem(new SpriteRenderSystem());

            var camera = scene.CreateEntity();
            scene.AttachComponent(camera, new TransformComponent());
            scene.AttachComponent(camera, new CameraComponent()
            {
                PixelsPerUnit = 1,
                OrthographicSize = 1,
                ClearColour = new Color(0, 0, 0)
            }
            );

            // Create our example thing
            var example = scene.CreateEntity();
            scene.AttachComponent(example, new ExampleComponent());

            Game.DevelopmentMode = false;
            Game.Console.DrawConsoleNotification = false;

            Game.Window.SetIcon(Assets.LoadNoCache<Texture>("textures/icon.png"));
            Game.Profiling.DrawQuickProfiler = false;
            Game.Compositor.Enabled = false;

            Game.Start();
        }
    }

    public class ExampleComponent : Walgelijk.Component
    {
    }

    public class ExampleSystem : Walgelijk.System
    {
        public RigidBody2DComponent leftRb;
        public RigidBody2DComponent rightRb;

        public RigidBody2DComponent ballRb;

        public float leftVelocity;
        public float rightVelocity;

        public override void Initialise()
        {
            var left = Scene.CreateEntity();
            var leftTransform = Scene.AttachComponent(left, new TransformComponent 
            {
                Position = new Vector2(-500, 0)
            });
            var leftCollider = Scene.AttachComponent(left, new BoxCollider2DComponent 
            {
                Size = new Vector2(10, 100),
                LayerMask = new(5)
            });
            leftRb = Scene.AttachComponent(left, new RigidBody2DComponent 
            {
                GravityScale = 0,
                Material = new PhysicsMaterial(0, 1)
            });

            var right = Scene.CreateEntity();
            var rightTransform = Scene.AttachComponent(right, new TransformComponent 
            {
                Position = new Vector2(500, 0)
            });
            var rightCollider = Scene.AttachComponent(right, new BoxCollider2DComponent
            {
                Size = new Vector2(10, 100),
                LayerMask = new(5)
            });
            rightRb = Scene.AttachComponent(right, new RigidBody2DComponent
            {
                GravityScale = 0,
                Material = new PhysicsMaterial(0, 1)
            });

            CreateWall(new Vector2(0, 350), new Vector2(3000, 100));
            CreateWall(new Vector2(0, -350), new Vector2(3000, 100));
        }

        public override void FixedUpdate()
        {
            if (!Scene.TryGetComponentFrom<TransformComponent>(leftRb.Entity, out var leftTransform) ||
                !Scene.TryGetComponentFrom<BoxCollider2DComponent>(leftRb.Entity, out var leftBc) ||
                !Scene.TryGetComponentFrom<TransformComponent>(rightRb.Entity, out var rightTransform) ||
                !Scene.TryGetComponentFrom<BoxCollider2DComponent>(rightRb.Entity, out var rightBc))
                return;

            leftTransform.Position = Vector2.Clamp(leftTransform.Position, new Vector2(-500, -280), new Vector2(-500, 280));
            rightTransform.Position = Vector2.Clamp(rightTransform.Position, new Vector2(500, -250), new Vector2(500, 250));

            Vector2 force = new Vector2(0, 0);

            if (Input.IsKeyHeld(Key.W))
                force.Y = force.Y + 1;
            if (Input.IsKeyHeld(Key.A))
                force.X = force.X - 1;
            if (Input.IsKeyHeld(Key.S))
                force.Y = force.Y - 1;
            if (Input.IsKeyHeld(Key.D))
                force.X = force.X + 1;

            if (force != Vector2.Zero)
                force = Vector2.Normalize(force);

            leftRb.Velocity.Y = force.Y * 4.5f;

            if (ballRb != null &&
                Scene.TryGetComponentFrom<TransformComponent>(ballRb.Entity, out var ballTransform) &&
                Scene.TryGetComponentFrom<BoxCollider2DComponent>(ballRb.Entity, out var ballBc))
            {
                float distanceFromBall = 0;
                distanceFromBall = ballTransform.Position.Y - rightTransform.Position.Y;
                rightRb.Velocity = Vector2.Normalize(new Vector2(0, distanceFromBall)) * 4.5f;

                if (ballTransform.Position.X > 625)
                {
                    leftPoints++;
                    Scene.RemoveEntity(ballTransform.Entity);
                    CreateBall();
                }
                else if (ballTransform.Position.X < -625)
                {
                    rightPoints++;
                    Scene.RemoveEntity(ballTransform.Entity);
                    CreateBall();
                }
            }

            if (Initialized)
                StartTimer = float.Clamp(StartTimer + Time.FixedInterval, 0, 3.5f);

            if (Input.IsKeyPressed(Key.Enter))
                Initialized = true;

            if (StartTimer > 3)
            {
                if (ballRb == null)
                {
                    CreateBall();
                }
            }
        }

        public float StartTimer = 0;
        public bool Initialized = false;

        public int leftPoints = 0;
        public int rightPoints = 0;

        public override void Render()
        {
            if (!Scene.TryGetComponentFrom<TransformComponent>(leftRb.Entity, out var leftTransform) ||
                !Scene.TryGetComponentFrom<BoxCollider2DComponent>(leftRb.Entity, out var leftBc) ||
                !Scene.TryGetComponentFrom<TransformComponent>(rightRb.Entity, out var rightTransform) ||
                !Scene.TryGetComponentFrom<BoxCollider2DComponent>(rightRb.Entity, out var rightBc))
                return;

            Draw.Reset();
            Draw.Colour = Colors.White.WithAlpha(0.5f);

            Draw.Line(new Vector2(0, 500), new Vector2(0, -500), 5);

            Draw.Colour = Colors.White;
            Draw.Quad(new Rect(leftTransform.Position + leftBc.Offset, leftBc.Size));
            Draw.Quad(new Rect(rightTransform.Position + rightBc.Offset, rightBc.Size));

            if (ballRb != null &&
                Scene.TryGetComponentFrom<TransformComponent>(ballRb.Entity, out var ballTransform) &&
                Scene.TryGetComponentFrom<BoxCollider2DComponent>(ballRb.Entity, out var ballBc))
            {
                Draw.Quad(new Rect(ballTransform.Position + ballBc.Offset, ballBc.Size));
            }

            Draw.Reset();
            Draw.ScreenSpace = true;
            Draw.Colour = Colors.White;
            Draw.Font = Assets.Load<Font>("fonts/inter.wf");
            Draw.FontSize = 72;

            string p = "READY?";
            if (StartTimer > 3)
                p = string.Empty;
            else if (StartTimer > 2)
                p = "GO!";
            else if (StartTimer > 1)
                p = "SET...";
            else
                p = "READY?";

            if (Initialized)
                Draw.Text(p, new Vector2(Window.Width / 2, 50), Vector2.One, HorizontalTextAlign.Center);

            if (StartTimer > 3)
            {
                Draw.Text(leftPoints.ToString(), new Vector2(Window.Width / 2 - (Window.Width / 4), 50), Vector2.One, HorizontalTextAlign.Center);
                Draw.Text(rightPoints.ToString(), new Vector2(Window.Width / 2 + (Window.Width / 4), 50), Vector2.One, HorizontalTextAlign.Center);
            }
        }

        public RigidBody2DComponent CreateBall()
        {
            var ball = Scene.CreateEntity();
            var ballTransform = Scene.AttachComponent(ball, new TransformComponent
            {
                Position = new Vector2(0, 0)
            });
            var ballCollider = Scene.AttachComponent(ball, new BoxCollider2DComponent
            {
                Size = new Vector2(10),
                LayerMask = new(4)
            });

            Vector2 random = Vector2.One;

            switch (Utilities.RandomInt(1, 4))
            {
                case 1:
                    random = new(1, 1);
                    break;

                case 2:
                    random = new(1, -1);
                    break;

                case 3:
                    random = new(-1, -1);
                    break;

                case 4:
                    random = new(-1, 1);
                    break;
            }

            return ballRb = Scene.AttachComponent(ball, new RigidBody2DComponent
            {
                GravityScale = 0,
                Material = new PhysicsMaterial(1, 1),
                Velocity = random * 5
            });
        }

        public BoxCollider2DComponent CreateWall(Vector2 position, Vector2 size, LayerMask? layer = null)
        {
            var ent = Scene.CreateEntity();
            var transform = Scene.AttachComponent(ent, new TransformComponent
            {
                Position = position,
                Scale = size
            });
            return Scene.AttachComponent(ent, new BoxCollider2DComponent
            {
                LayerMask = layer.HasValue ? layer.Value : new(3),
                Size = size
            });
        }
    }
}