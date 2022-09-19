using System;

namespace DiscordGaurdUpdates
{
    internal class Settings
    {
        public uint botErrorsId;
        public string chatLogsDir;
        public string discordToken;
        public uint enemySightingsId;
        public uint voiceSlaversOnlyId;

        public Settings()
        {
        }

        public Settings(uint botErrorsId, string chatLogsDir, string discordToken, uint enemySightingsId, uint voiceSlaversOnlyId)
        {
            this.botErrorsId = botErrorsId;
            this.chatLogsDir = chatLogsDir ?? throw new ArgumentNullException(nameof(chatLogsDir));
            this.discordToken = discordToken ?? throw new ArgumentNullException(nameof(discordToken));
            this.enemySightingsId = enemySightingsId;
            this.voiceSlaversOnlyId = voiceSlaversOnlyId;
        }
    }
}