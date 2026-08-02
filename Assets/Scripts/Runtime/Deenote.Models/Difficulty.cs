namespace Deenote.Models
{
    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Extra,
        Expert = Extra,
        Special,
    }

    public static class DifficultyExtensions
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

        public static string ToCapitalizedString(this Difficulty difficulty, GameVersion gameVersion)
        {
            return difficulty switch {
                Difficulty.Easy => "Easy",
                Difficulty.Normal => "Normal",
                Difficulty.Hard => "Hard",
                Difficulty.Extra => gameVersion == GameVersion.Deemo ? "Extra" : "Expert",
                Difficulty.Special => "Special",
                _ => "Unknown",
            };
        } 
    }
}
