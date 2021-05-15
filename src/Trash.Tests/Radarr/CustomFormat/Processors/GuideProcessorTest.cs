﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Common;
using FluentAssertions;
using Flurl.Http.Testing;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Serilog;
using TestLibrary.FluentAssertions;
using Trash.Radarr;
using Trash.Radarr.CustomFormat.Guide;
using Trash.Radarr.CustomFormat.Models;
using Trash.Radarr.CustomFormat.Processors;
using Trash.Radarr.CustomFormat.Processors.GuideSteps;

namespace Trash.Tests.Radarr.CustomFormat.Processors
{
    [TestFixture]
    [Parallelizable(ParallelScope.All)]
    public class GuideProcessorTest
    {
        private class TestGuideProcessorSteps : IGuideProcessorSteps
        {
            public ICustomFormatStep CustomFormat { get; } = new CustomFormatStep();
            public IConfigStep Config { get; } = new ConfigStep();
            public IQualityProfileStep QualityProfile { get; } = new QualityProfileStep();
        }

        private class Context
        {
            public Context()
            {
                Logger = new LoggerConfiguration()
                    .WriteTo.TestCorrelator()
                    .WriteTo.NUnitOutput()
                    .MinimumLevel.Debug()
                    .CreateLogger();

                Data = new ResourceDataReader(typeof(GuideProcessorTest), "Data");
            }

            public ILogger Logger { get; }
            public ResourceDataReader Data { get; }

            public JObject ReadJson(string jsonFile)
            {
                var jsonData = Data.ReadData(jsonFile);
                return JObject.Parse(jsonData);
            }
        }

        [Test]
        [SuppressMessage("Maintainability", "CA1506", Justification = "Designed to be a high-level integration test")]
        public void Guide_processor_behaves_as_expected_with_normal_markdown()
        {
            var ctx = new Context();
            var guideProcessor =
                new GuideProcessor(ctx.Logger, new CustomFormatGuideParser(ctx.Logger),
                    () => new TestGuideProcessorSteps());

            // simulate guide data
            using var testHttp = new HttpTest();
            testHttp.RespondWith(ctx.Data.ReadData("CF_Markdown1.md"));

            // Simulate user config in YAML
            var config = new List<CustomFormatConfig>
            {
                new()
                {
                    Names = new List<string> {"Surround SOUND", "DTS-HD/DTS:X", "no score", "not in guide 1"},
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile1"},
                        new() {Name = "profile2", Score = -1234}
                    }
                },
                new()
                {
                    Names = new List<string> {"no score", "not in guide 2"},
                    QualityProfiles = new List<QualityProfileConfig>
                    {
                        new() {Name = "profile3"},
                        new() {Name = "profile4", Score = 5678}
                    }
                }
            };

            guideProcessor.BuildGuideData(config, null);

            var expectedProcessedCustomFormatData = new List<ProcessedCustomFormatData>
            {
                new("Surround Sound", "43bb5f09c79641e7a22e48d440bd8868", ctx.ReadJson(
                    "ImportableCustomFormat1_Processed.json"))
                {
                    Score = 500
                },
                new("DTS-HD/DTS:X", "4eb3c272d48db8ab43c2c85283b69744", ctx.ReadJson(
                    "ImportableCustomFormat2_Processed.json"))
                {
                    Score = 480
                },
                new("No Score", "abc", JObject.FromObject(new {name = "No Score"}))
            };

            guideProcessor.ProcessedCustomFormats.Should().BeEquivalentTo(expectedProcessedCustomFormatData,
                op => op.Using(new JsonEquivalencyStep()));

            guideProcessor.ConfigData.Should().BeEquivalentTo(new List<ProcessedConfigData>
            {
                new()
                {
                    CustomFormats = expectedProcessedCustomFormatData,
                    QualityProfiles = config[0].QualityProfiles
                },
                new()
                {
                    CustomFormats = expectedProcessedCustomFormatData.GetRange(2, 1),
                    QualityProfiles = config[1].QualityProfiles
                }
            }, op => op
                .Using<JToken>(jctx => jctx.Subject.Should().BeEquivalentTo(jctx.Expectation))
                .WhenTypeIs<JToken>());

            guideProcessor.CustomFormatsWithoutScore.Should()
                .Equal(new List<(string name, string trashId, string profileName)>
                {
                    ("No Score", "abc", "profile1"),
                    ("No Score", "abc", "profile3")
                });

            guideProcessor.CustomFormatsNotInGuide.Should().Equal(new List<string>
            {
                "not in guide 1", "not in guide 2"
            });

            guideProcessor.ProfileScores.Should()
                .BeEquivalentTo(new Dictionary<string, List<QualityProfileCustomFormatScoreEntry>>
                {
                    {
                        "profile1", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[0], 500),
                            new(expectedProcessedCustomFormatData[1], 480)
                        }
                    },
                    {
                        "profile2", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[0], -1234),
                            new(expectedProcessedCustomFormatData[1], -1234),
                            new(expectedProcessedCustomFormatData[2], -1234)
                        }
                    },
                    {
                        "profile4", new List<QualityProfileCustomFormatScoreEntry>
                        {
                            new(expectedProcessedCustomFormatData[2], 5678)
                        }
                    }
                }, op => op.Using(new JsonEquivalencyStep()));
        }
    }
}