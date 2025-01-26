using Discord.WebSocket;

namespace SDDiscordBot.Models
{
    public struct SDSettings
    {
        private readonly static Random _random = new Random();
        public string Prompt { get; set; } 
        public bool HighResFix { get; set; } = false;
        public double HighResScale { get; set; } = 2.0;
        public long Seed { get; set; } = -1;

        public SDSettings() {}
        public SDSettings(SocketSlashCommand command)
        {
            var options = command.Data.Options.ToList();

            foreach (var option in options)
            {
                switch(option.Name)
                {
                    case "prompt":
                        Prompt = (string)option.Value;
                        break;
                    case "scale":
                        HighResFix = true;
                        double value = (double)option.Value;
                        HighResScale = value < 1.1 ? 1.1 : value <= 2.0 ? value : 2.0;
                        break;
                    case "seed":
                        Seed = (long)option.Value;
                        break;
                }
            }
        }

        public void InsureSeed()
        {
            if(Seed < 0)
            {
                Seed = _random.Next(100000000, 999999999);
            }
        }
    }
}
