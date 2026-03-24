#nullable enable

namespace Deenote.CoreB.Models
{
    public enum Difficulty
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Extra = 3,
        Expert = 3,
        Special = 4,
    }

    public static class DifficulityExtensions
    {
        public static string ToLowerCaseString(this Difficulty difficulty, GameVersion gameVersion)
        {
            return difficulty switch {
                Difficulty.Easy => "easy",
                Difficulty.Normal => "normal",
                Difficulty.Hard => "hard",
                Difficulty.Extra => gameVersion == GameVersion.Deemo ? "extra" : "expert",
                Difficulty.Special => "special",
                _ => "unknown",
            };
        }
    }
}
