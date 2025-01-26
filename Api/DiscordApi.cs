using SDDiscordBot.Models;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;



namespace SDDiscordBot.Api
{

    public class DiscordApi
    {
        private DiscordSocketClient _client;

        private readonly string _token;

        private readonly StableDiffusionApi _stableDiffusionApi;

        public DiscordApi(IConfiguration config, StableDiffusionApi stableDiffusionApi)
        {
            DiscordSocketConfig socketConfig = new DiscordSocketConfig();
            socketConfig.GatewayIntents = GatewayIntents.None;
            
            var discordConfig = config.GetSection("DiscordApi");
            _token = discordConfig["Token"];

            _stableDiffusionApi = stableDiffusionApi;

            _client = new DiscordSocketClient(socketConfig);
            _client.Log += Log;
            _client.Ready += OnReady;
            _client.SlashCommandExecuted += SlashCommandHandler;         
        }


        public async Task Connect()
        {
            await _client.LoginAsync(TokenType.Bot, _token);
            await _client.StartAsync();
        }

        private async Task OnReady()
        {
            //create generate slash command
            var guildCommand = new SlashCommandBuilder()
                .WithName("generate")
                .WithDescription("Generate an image.")
                .AddOption("prompt", ApplicationCommandOptionType.String, "Image generation prompt", isRequired: true)
                .AddOption("scale", ApplicationCommandOptionType.Number, "Apply high res fix", isRequired: false)
                .AddOption("seed", ApplicationCommandOptionType.Integer, "Use seed", isRequired: false);


            var globalCommands = await _client.GetGlobalApplicationCommandsAsync();
            if(!globalCommands.Any(x => x.Name == "generate"))
            {       
                try
                {
                    await _client.CreateGlobalApplicationCommandAsync(guildCommand.Build());
                }
                catch (HttpException exception)
                {
                    var json = JsonConvert.SerializeObject(exception.Errors, Formatting.Indented);
                    Console.WriteLine(json);
                }
            }
        }

        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            Console.WriteLine("Received Command: " + command.Data.Name);
            switch (command.Data.Name)
            {
                case "generate":
                    await HandleGenerateCommand(command);
                    break;
            }
        }

        private async Task HandleGenerateCommand(SocketSlashCommand command)
        {
            SDSettings settings = new SDSettings(command);
            Task<SDImage?> imageGenTask = _stableDiffusionApi.GenerateImageAsync(settings);

            //builder for response message
            EmbedBuilder embedBuilder = new EmbedBuilder()
                .WithTitle("Generating Image")
                .WithColor(Color.Red)
                .WithDescription("0%");

            await command.RespondAsync(embed: embedBuilder.Build());

            //update progress in discord channel
            float progress = 0.0f;
            while (!imageGenTask.Wait(500 /*Message update delay*/))
            {
                float newProgress = (await _stableDiffusionApi.GetProgress()).GetValueOrDefault().Progress;
                if(newProgress > progress)
                {
                    progress = newProgress;
                    embedBuilder.Description = $"{float.Round(progress * 100)}%";
                    await command.ModifyOriginalResponseAsync(m => m.Embed = embedBuilder.Build());
                }
            }
        
            //generation finished/failed -> Post final update
            embedBuilder.Description = "";

            SDImage? sdImage = await imageGenTask;
            if (sdImage.HasValue && File.Exists(sdImage.Value.Path))
            {
                embedBuilder.WithTitle(sdImage.Value.Name.Split(".").FirstOrDefault())
                            .WithImageUrl($"attachment://{sdImage.Value.Name}")
                            .WithColor(Color.Green);
                   
                await command.ModifyOriginalResponseAsync(m => { m.Embed = embedBuilder.Build(); m.Attachments = new List<FileAttachment>() { new FileAttachment(sdImage.Value.Path) }; } );
            }
            else
            {
                embedBuilder.WithTitle("Generation Failed. Try again later.");
                await command.ModifyOriginalResponseAsync(m => m.Embed = embedBuilder.Build());
            }
        }


        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
