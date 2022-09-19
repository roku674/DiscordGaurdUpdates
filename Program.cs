//Created by Alexander Fields https://github.com/roku674
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace DiscordGaurdUpdates
{
    internal class Program
    {
        //nentwork variables
        private static DiscordSocketClient _client;

        public static DiscordSocketClient client { get => _client; set => _client = value; }

        public static string lastLand { get; set; }
        public static string lastSystem { get; set; }

        private static Settings settings { get; set; }

        public static string[] allies { get; set; }
        public static string[] enemies { get; set; }
        public static string[] nap { get; set; }

        private static void Main(string[] args)
        {
            settings = Newtonsoft.Json.JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Directory.GetCurrentDirectory() + "/JSON/settings.json"));
            string directory = Directory.GetCurrentDirectory();

            Algorithms.FileManipulation.DeleteFile(directory + "/JSON/allies.json");

            GetFileFromInternet("https://raw.githubusercontent.com/roku674/StarportObjects/main/JSON/allies.json", directory + "/JSON/allies.json");
            System.Console.WriteLine("allies List Updated!");

            Algorithms.FileManipulation.DeleteFile(directory + "/JSON/enemies.json");

            GetFileFromInternet("https://raw.githubusercontent.com/roku674/StarportObjects/main/JSON/enemies.json", directory + "/JSON/enemies.json");
            System.Console.WriteLine("Enemies List Updated!");

            Algorithms.FileManipulation.DeleteFile(directory + "/JSON/nap.json");
            GetFileFromInternet("https://raw.githubusercontent.com/roku674/StarportObjects/main/JSON/nap.json", directory + "/JSON/nap.json");
            System.Console.WriteLine("NAP List Updated!");

            try
            {
                allies = JsonSerializer.Deserialize<string[]>(File.ReadAllText(directory + "/JSON/allies.json"));
                enemies = JsonSerializer.Deserialize<string[]>(File.ReadAllText(directory + "/JSON/enemies.json"));
                nap = JsonSerializer.Deserialize<string[]>(File.ReadAllText(directory + "/JSON/nap.json"));
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine(e);
                System.Console.WriteLine("You are missing either the Allies, Enemies or NAP json file thus the updater will not operate properly!");
            }

            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        private static void GetFileFromInternet(string hyperlink, string filePath)
        {
            WebClient webClient = new WebClient();
            Stream stream = webClient.OpenRead(hyperlink);
            StreamReader reader = new StreamReader(stream);
            File.WriteAllText(filePath, reader.ReadToEnd());
        }

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();

            string token = settings.discordToken;

            _client.Log += _client_Log;

            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(10000);
            await Task.Run(() => ChatLogListener());

            await Task.Delay(System.Threading.Timeout.Infinite);
        }

        private Task _client_Log(Discord.LogMessage arg)
        {
            System.Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private async Task ChatLogListener()
        {
            bool chatLogsFound = false;

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = settings.chatLogsDir;

            if (!watcher.Path.Equals(" ") && !string.IsNullOrEmpty(watcher.Path))
            {
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";

                watcher.Changed += new FileSystemEventHandler(OnChatChangedAsync);

                watcher.EnableRaisingEvents = true;

                System.Console.WriteLine("Sucessfully ran " + settings.chatLogsDir + " Listener!");

                await Task.Delay(System.Threading.Timeout.Infinite);
            }

            if (!chatLogsFound)
            {
                System.Console.WriteLine("Couldn't find chatlogs program will now self destruct in 10 seconds!");
                await OutprintAsync("Couldn't find chatlogs program will now self destruct in 10 seconds!", settings.botErrorsId);
                await Task.Delay(10000);
                System.Environment.Exit(1);
            }
        }

        private async void OnChatChangedAsync(object sender, FileSystemEventArgs fileSysEvent)
        {
            string filePath = fileSysEvent.FullPath;
            string[] fileStrArr = new string[0];

            string[] split = Path.GetFileName(filePath).Split(" ");
            string chatLogOwner = split[0];

            try
            {
                fileStrArr = await File.ReadAllLinesAsync(filePath);
            }
            catch (System.Exception ex)
            {
                await OutprintAsync(ex.ToString(), settings.botErrorsId);
            }

            if (fileStrArr.Length > 0)
            {
                string lastLine = fileStrArr[fileStrArr.Length - 1];
                string secondToLastLine = fileStrArr[fileStrArr.Length - 2];

                if (lastLine.Contains("Landed on ") && lastLine.Contains("world"))
                {
                    lastLand = Algorithms.StringManipulation.GetBetween(lastLine, "on", ",");
                    lastLand = lastLand.Replace("world ", "");
                }
                else if (lastLine.Contains("Warped to"))
                {
                    lastSystem = Algorithms.StringManipulation.GetBetween(lastLine, "to", ",");
                }
                else if (lastLine.Contains("Taking you directly"))
                {
                    lastLand = Algorithms.StringManipulation.GetBetween(secondToLastLine, "on", ",");
                    lastLand = lastLand.Replace("world ", "");
                }

                //warped in
                if (enemies.Any(s => lastLine.Contains(s)) && (lastLine.Contains("warped into") || lastLine.Contains("entered the system")))
                {
                    List<string> enemiesList = enemies.ToList<string>();
                    string enemy = enemiesList.Find(s => lastLine.Contains(s));

                    await OutprintAsync("@everyone " + chatLogOwner + ": " + lastLine + " in " + lastSystem, settings.enemySightingsId);
                    await SayAsync(enemy + " spotted! By " + chatLogOwner + " in " + lastSystem, settings.voiceSlaversOnlyId);
                }
                else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
                {
                    List<string> enemiesList = allies.ToList<string>();
                    string enemy = enemiesList.Find(s => lastLine.Contains(s));

                    await OutprintAsync("@everyone " + lastLine, settings.enemySightingsId);
                    await SayAsync(enemy + " ran away cause he's a bitch nigga", settings.voiceSlaversOnlyId);
                }
                else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("landed on a planet"))
                {
                    List<string> enemiesList = allies.ToList<string>();
                    string enemy = enemiesList.Find(s => lastLine.Contains(s));

                    await OutprintAsync(lastLine + " in " + lastSystem, settings.enemySightingsId);
                    await SayAsync(enemy + " landed!", settings.voiceSlaversOnlyId);
                }
                else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("docked"))
                {
                    List<string> enemiesList = allies.ToList<string>();
                    string enemy = enemiesList.Find(s => lastLine.Contains(s));

                    await OutprintAsync(lastLine + " in " + lastSystem, settings.enemySightingsId);
                    await SayAsync(enemy + " Re-Shielded!", settings.voiceSlaversOnlyId);
                }
            }
        }

        public static async Task OutprintAsync(string message, ulong channelId)
        {
            System.Console.WriteLine(message);
            if (message.Contains("shouts") || message.Contains("radios") || message.Contains("tells"))
            {
                Discord.IMessageChannel channel = Program.client.GetChannel(settings.botErrorsId) as Discord.IMessageChannel;
                await channel.SendMessageAsync(message);
            }
            else
            {
                Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
                await channel.SendMessageAsync(message);
            }
        }

        public static async Task SayAsync(string message, ulong channelId)
        {
            System.Console.WriteLine(message);

            Discord.IVoiceChannel channel = Program.client.GetChannel(channelId) as Discord.IVoiceChannel;
            await channel.SendMessageAsync(message, isTTS: true);
        }
    }
}