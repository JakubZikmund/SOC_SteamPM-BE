namespace SOC_SteamPM_BE.Utils;

public static class SteamIdValidation
{
    public static bool IsSteamIdValid(string input)
    {
        if (!ulong.TryParse(input, out ulong steamId))
        {
            return false;
        }

        if (input.Length != 17 || !input.StartsWith("7656119"))
        {
            return false;
        }

        return true;
    }
}