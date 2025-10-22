using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;

namespace PantheonWars
{
    public class PantheonWarsModSystem : ModSystem
    {
        public string ModName => "Pantheon Wars";
        
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.Logger.Notification("[PantheonWars] Mod loaded!");
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            api.Logger.Notification("[PantheonWars] Server-side initialization complete");
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            base.StartClientSide(api);
            api.Logger.Notification("[PantheonWars] Client-side initialization complete");
        }
    }
}
