using Newtonsoft.Json;
using System.Numerics;

namespace TPtoWP
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string path = args[0];
            string json = File.ReadAllText(path);

            //Console.Write("Enter mode.. (0: COUNTED, 1: INDIVIDUAL)");
            string mode = "0";

            switch (mode)
            {
                case "0":
                    TPJson tPJson = JsonConvert.DeserializeObject<TPJson>(json) ?? throw new Exception("Attempted to deserialize null JSON!");
                    SpriteSheet spriteSheet = new SpriteSheet();
                    spriteSheet.Name = Path.GetFileNameWithoutExtension(path);
                    foreach (var frame in tPJson.Frames)
                    {
                        spriteSheet.Animations.Add(frame.Filename, new SpriteAnimation
                        {
                            FrameInfos = new List<FrameInfo> 
                            {
                                new FrameInfo
                                {
                                    Scale = new Vector2(frame.Frame.W, frame.Frame.H),
                                    TopLeft = new Vector2(frame.Frame.X, frame.Frame.Y)
                                }
                            }
                        });
                    }

                    spriteSheet.Texture = $"base:textures/sprites/{spriteSheet.Name}.png";
                    string spritejson = JsonConvert.SerializeObject(spriteSheet, Formatting.Indented);
                    File.WriteAllText($"{path}bak", spritejson);
                    break;

                default:
                    Environment.Exit(0);
                    break;
            }
        }
    }
}
