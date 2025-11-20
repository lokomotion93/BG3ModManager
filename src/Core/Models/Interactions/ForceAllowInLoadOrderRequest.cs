using ModManager.Models.Mod;

namespace ModManager;

public record ForceAllowInLoadOrderRequest(ModData mod, bool AddToOrder);