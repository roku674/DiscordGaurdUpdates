using System;

namespace DiscordGaurdUpdates
{
    internal class Settings
    {
        public string discordToken;
        public uint botErrorsId;
        public uint enemySightingsId;
        public uint voiceSlaversOnlyId;

        public Settings()
        {
        }

        public Settings(string discordToken, uint botErrorsId, uint enemySightingsId, uint voiceSlaversOnlyId)
        {
            this.discordToken = discordToken ?? throw new ArgumentNullException(nameof(discordToken));
            this.botErrorsId = botErrorsId;
            this.enemySightingsId = enemySightingsId;
            this.voiceSlaversOnlyId = voiceSlaversOnlyId;
        }
    }
}