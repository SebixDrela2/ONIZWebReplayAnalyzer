using OhNoItsZombiesAnalyzer.Models;
using ONIZAnalyzer.Core.Helpers.Replay;
using ONIZAnalyzer.Core.Serializer.CareerData;
using System.Text;

namespace ONIZAnalyzer.Core.Helpers.Record;

public class OnizRecordHandler
{
    private readonly NameHandle NameHandle;
    private readonly OnizGeneralCareerData General; 
    private readonly OnizMarineCareerData Marine; 
    private readonly OnizZombieCareerData Zombie;
    public OnizRecordHandler(OnizHandleCareerData careerData) => (NameHandle, (General, Marine, Zombie)) = careerData;

    public string GetRecordText()
    {
        var builder = new StringBuilder();

        AppendMetaData(builder);
        AppendGeneralData(builder);
        AppendMarineData(builder);
        AppendZombieData(builder);

        return builder.ToString();
    }

    private void AppendMetaData(StringBuilder builder)
    {
        builder.AppendHeaderLine("Metadata");
        builder.AppendLine($"Name: {NameHandle.Name}");
        builder.AppendLine($"Handle: {NameHandle.Handle}");
        builder.AppendLine();
    }

    private void AppendGeneralData(StringBuilder builder)
    {
        builder.AppendHeaderLine("General");
        builder.AppendLine($"Last game played: {General.LastGamePlayed}");
        builder.AppendLine($"Months of service: {General.MonthsOfService}");
        builder.AppendLine();
    }

    private void AppendMarineData(StringBuilder builder)
    {
        builder.AppendHeaderLine("Marine");
        builder.AppendLine($"Games played: {Marine.GamesPlayed}");
        builder.AppendLine($"Games won: {Marine.GamesWon}");
        builder.AppendLine($"Win percent: {Marine.GamesWinPercentage:P0}");
        builder.AppendLine($"Kills: {Marine.TotalKills}");
        builder.AppendLine($"Alpha kills: {Marine.TotalAlphaKills}");
        builder.AppendLine($"Alpha kills per game: {Marine.AverageAlphaKillsPerGame}");
        builder.AppendLine($"Favourite weapon: {Marine.FavouriteWeapon}");
        builder.AppendLine($"Favourite upgrade: {Marine.FavouriteUpgrade}");
        builder.AppendLine();
    }

    private void AppendZombieData(StringBuilder builder)
    {
        builder.AppendHeaderLine("Zombie");
        builder.AppendLine($"Games played: {Zombie.GamesPlayed}");
        builder.AppendLine($"Games won: {Zombie.GamesWon}");
        builder.AppendLine($"Win percent: {Zombie.GamesWinPercentage:P0}");
        builder.AppendLine($"Favourite alpha opener: {Zombie.FavouriteAlphaOpener}");
        builder.AppendLine($"Favourite upgrade: {Zombie.FavouriteZombieUpgrade}");
        builder.AppendLine();
    }
}
