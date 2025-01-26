
using SDDiscordBot.Api;
using Microsoft.Extensions.Configuration;


public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = new ConfigurationBuilder();
        builder.SetBasePath(AppDomain.CurrentDomain.BaseDirectory).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        IConfiguration config = builder.Build();

        StableDiffusionApi stableDiffusionApi = new StableDiffusionApi(config);        

        DiscordApi discordApi = new DiscordApi(config, stableDiffusionApi);
        await discordApi.Connect();
        await Task.Delay(-1);
    }
}


