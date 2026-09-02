public static class GameUtil
{
    public static int GetLevelCost(int level)
    {
        if (level < 1)
        {
            level = 1;
        }

        return level * 100;
    }

    public static int GetLevelStat(int baseStat, int level)
    {
        if (baseStat < 0)
        {
            baseStat = 0;
        }

        if (level < 1)
        {
            level = 1;
        }

        return baseStat + baseStat * (level - 1) / 10;
    }

    public static string GetStars(string rarity)
    {
        if (rarity == "SSR") return "★★★";
        if (rarity == "SR") return "★★";
        return "★";
    }
}
