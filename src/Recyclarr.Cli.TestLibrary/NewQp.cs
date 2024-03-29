using Recyclarr.Cli.Pipelines.QualityProfile.PipelinePhases;
using Recyclarr.TrashLib.Config.Services;

namespace Recyclarr.Cli.TestLibrary;

public static class NewQp
{
    public static ProcessedQualityProfileData Processed(
        string profileName,
        params (int FormatId, int Score)[] scores)
    {
        return Processed(profileName, null, scores);
    }

    public static ProcessedQualityProfileData Processed(
        string profileName,
        bool? resetUnmatchedScores,
        params (int FormatId, int Score)[] scores)
    {
        return new ProcessedQualityProfileData(new QualityProfileConfig
        {
            Name = profileName, ResetUnmatchedScores = resetUnmatchedScores
        })
        {
            CfScores = scores.ToDictionary(x => x.FormatId, x => x.Score)
        };
    }
}
