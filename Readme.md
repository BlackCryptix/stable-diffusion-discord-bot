# Stable Diffusion Discord Bot

This Discord bot allows you to generate images inside a Discord server using **AUTOMATIC1111 Stable Diffusion WebUI**.

---

## How to Use
To use the bot inside the server, type `/generate` into the chat. The bot is configured to work best with an **Illustrator base model**, but other models might require adjustments (e.g., sampling method or resolution) for optimal results.

### Command Details
The `/generate` command can be used in any server chat where the bot has access. Below are the parameters available:

- **Prompt** (required): The prompt used to generate the image.
- **Scale** (optional): If provided, applies high-resolution fix to the image with the specified scale.
- **Seed** (optional): The seed used to generate the image.

---

## Setup Instructions

### Launching AUTOMATIC1111 Stable Diffusion API
To launch the Stable Diffusion API, add the `--api` argument. If you want the API to run on `0.0.0.0` instead of localhost, also include the `--listen` argument.

#### For `webui-user.bat` (Windows):
Add the following arguments:
```bash
set COMMANDLINE_ARGS=--api --listen
```

#### For `webui-user.sh` (Linux):
Add the following arguments:
```bash
export COMMANDLINE_ARGS="--api --listen"
```
After adding the arguments, execute the `webui-user` script.

---

### Configuring `appsettings.json`
Adjust the following settings in `appsettings.json` (located in the root directory):

#### Discord API Configuration
```json
"DiscordApi": {
    "Token": "<Your Discord Bot Token>"
}
```

#### Stable Diffusion Configuration
```json
"StableDiffusion": {
    "Host": "http://127.0.0.1",
    "Port": 7860,
    "SaveFolderPath": "<Path to Save Generated Images>",
    "DefaultPositive": "<Default Positive Prompt>",
    "DefaultNegative": "<Default Negative Prompt>",
    "Upscaler": "4xNMKDSuperscale"
}
```
- **Host**: Hostname of your Stable Diffusion WebUI (default: `http://127.0.0.1`).
- **Port**: Port of your Stable Diffusion WebUI (default: `7860`).
- **SaveFolderPath**: Folder path to save generated images.
- **DefaultPositive**: Positive prompt added to every generated image.
- **DefaultNegative**: Negative prompt added to every generated image.
- **Upscaler**: Upscaler used when applying high-resolution fix. Suggested options include `R-ESRGAN 4x+ Anime6B` or `Latent`.

---

### Adjusting `StableDiffusionApi.cs`
Further adjustments can be made in the `StableDiffusionApi.cs` file by modifying the request body. Below is an example:

```csharp
var requestBody = new
{
    prompt = generationSettings.Prompt + ", " + DefaultPositiv,
    negative_prompt = DefaultNegative,
    steps = 25,
    width = 912,
    height = 1216,
    cfg_scale = 5,
    seed = generationSettings.Seed,
    sampler_name = "Euler a",
    restore_faces = false,
    enable_hr = generationSettings.HighResFix,
    hr_scale = generationSettings.HighResScale,
    hr_second_pass_steps = 15,
    hr_upscaler = Upscaler,
    denoising_strength = 0.5,
    hr_additional_modules = new List<string>() { "Use same choice" }
};
```

---



