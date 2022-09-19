using System;

namespace DiscordGaurdUpdates
{
    internal class Settings
    {
        public ulong botErrorsId;
        public string chatLogsDir;
        public string discordToken;
        public ulong enemySightingsId;
        public ulong voiceSlaversOnlyId;

        public Settings()
        {
        }

        public Settings(ulong botErrorsId, string chatLogsDir, string discordToken, ulong enemySightingsId, ulong voiceSlaversOnlyId)
        {
            this.botErrorsId = botErrorsId;
            this.chatLogsDir = chatLogsDir ?? throw new ArgumentNullException(nameof(chatLogsDir));
            this.discordToken = discordToken ?? throw new ArgumentNullException(nameof(discordToken));
            this.enemySightingsId = enemySightingsId;
            this.voiceSlaversOnlyId = voiceSlaversOnlyId;
        }
    }
}