using BaseHarvest = Landis.Harvest;
using Edu.Wisc.Forest.Flel.Util;
using Landis.Extensions.BiomassHarvest;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace Landis.Tests.BiomassHarvest
{
    [TestFixture]
    public class ParametersParser_Test
    {
        private Species.IDataset speciesDataset;
        private const int startTime = 1950;
        private const int endTime =   2400;
        private ParametersParser parser;
        private LineReader reader;

        //---------------------------------------------------------------------

        [TestFixtureSetUp]
        public void Init()
        {
            Species.DatasetParser speciesParser = new Species.DatasetParser();
            reader = OpenFile("Species.txt");
            try {
                speciesDataset = speciesParser.Parse(reader);
            }
            finally {
                reader.Close();
            }

            parser = new ParametersParser(speciesDataset,
                                          startTime,
                                          endTime);
        }

        //---------------------------------------------------------------------

        [TestFixtureTearDown]
        public void TearDown()
        {
            // Unsubscribe from the event (static ctor of parser class
            // subscribes to the event)
            PartialThinning.ReadAgeOrRangeEvent -= ParametersParser.AgeOrRangeWasRead;
        }

        //---------------------------------------------------------------------

        private FileLineReader OpenFile(string filename)
        {
            string path = Data.MakeInputPath(filename);
            return Landis.Data.OpenTextFile(path);
        }

        //---------------------------------------------------------------------

        private BaseHarvest.IParameters ParseFile(string filename)
        {
            try {
                reader = OpenFile(filename);
                return parser.Parse(reader);
            }
            finally {
                reader.Close();
            }
        }

        //---------------------------------------------------------------------

        private string GetFileAssociatedWithTest(StackFrame currentTest)
        {
            //  Get the name of the input file associated with the test that's
            //  currently running.  The file's name is "{test}.txt".
            MethodBase testMethod = currentTest.GetMethod();
            string testName = testMethod.Name;
            string filename = testName + ".txt";
            return filename;
        }

        //---------------------------------------------------------------------

        private BaseHarvest.IParameters ParseGoodFile(string filename)
        {
            BaseHarvest.IParameters parameters = ParseFile(filename);
            Assert.IsNotNull(parameters);

            Assert.IsNotNull(parser.RoundedRepeatIntervals);
            Assert.AreEqual(1, parser.RoundedRepeatIntervals.Count);
            AssertAreEqual(new BaseHarvest.RoundedInterval(15, 20, 37),
                           parser.RoundedRepeatIntervals[0]);

            return parameters;
        }

        //---------------------------------------------------------------------

        [Test]
        public void GoodFile()
        {
            string filename = GetFileAssociatedWithTest(new StackFrame(0));
            ParseGoodFile(filename);
        }

        //---------------------------------------------------------------------

        [Test]
        public void GoodFile_BiomassMaps()
        {
            string filename = GetFileAssociatedWithTest(new StackFrame(0));
            BaseHarvest.IParameters baseHarvestParams = ParseGoodFile(filename);

            IParameters parameters = baseHarvestParams as IParameters;
            Assert.IsNotNull(parameters);
            Assert.IsNotNull(parameters.BiomassMapNames);
        }

        //---------------------------------------------------------------------

        private void AssertAreEqual(BaseHarvest.RoundedInterval expected,
                                    BaseHarvest.RoundedInterval actual)
        {
            Assert.AreEqual(expected.Original, actual.Original);
            Assert.AreEqual(expected.Adjusted, actual.Adjusted);
            Assert.AreEqual(expected.LineNumber, actual.LineNumber);
        }

        //---------------------------------------------------------------------

        private void TryParse(string filename)
        {
            int? errorLineNum = Testing.FindErrorMarker(Data.MakeInputPath(filename));
            try {
                reader = OpenFile(filename);
                BaseHarvest.IParameters parameters = parser.Parse(reader);
            }
            catch (System.Exception e) {
                Data.Output.WriteLine();
                Data.Output.WriteLine(e.Message.Replace(Data.Directory, Data.DirPlaceholder));
                LineReaderException lrExc = e as LineReaderException;
                if (lrExc != null && errorLineNum.HasValue)
                    Assert.AreEqual(errorLineNum.Value, lrExc.LineNumber);
                throw;
            }
            finally {
                reader.Close();
            }
        }

        //---------------------------------------------------------------------

        private void TryParseFileAssociatedWithTest()
        {
            //  This line:
            //
            //  string filename = GetFileAssociatedWithTest(new StackFrame(1));
            //
            //  doesn't work with the release configuration.
            MethodBase testMethod = new StackFrame(1).GetMethod();
            string testName = testMethod.Name;
            string filename = testName + ".txt";
            TryParse(filename);
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void LandisData_WrongValue()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Timestep_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Timestep_Negative()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void ManagementAreas_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void ManagementAreas_Empty()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void ManagementAreas_Whitespace()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Stands_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Stands_Empty()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Stands_Whitespace()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Prescription_NameMissing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Prescription_NameRepeated()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void StandRanking_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void StandRanking_Unknown()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_Empty()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_SpeciesUnknown()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_SpeciesRepeated()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_RankMissing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_RankBad()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_RankMoreThanMax()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_AgeMissing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_AgeBad()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_AgeTooBig()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_AgeNegative()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EconomicRankTable_Extra()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void RankingRequirement_MinAge_TooBig()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void RankingRequirement_MaxAge_Negative()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void RankingRequirement_MaxLessThanMin()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_MissingValue()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Unknown()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Complete_Extra()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_CompleteSpread_NoSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_CompleteSpread_BadSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_CompleteSpread_Negative()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_CompleteSpread_Extra()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_TargetSize_NoSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_TargetSize_BadSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_TargetSize_Negative()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_TargetSize_NotImplemented()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_TargetSize_Extra()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_NoPercentage()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_BadPercentage()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_PercentageTooBig()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_NoSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_BadSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_NegativeSize()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_Extra()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SiteSelection_Patch_NotImplemented()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_Unknown()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_None()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_UnknownSpecies()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_RepeatedSpecies()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_JustSpecies()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_ExtraAfterAll()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_Nis0()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_NisMissing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_NisBad()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_ExtraAfter1OverN()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_Age0()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_AgeBad()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_NoStart()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_NoEnd()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_StartBad()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_Start0()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_StartAfterEnd()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_EndBad()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_EndTooBig()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_AgeRepeated()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_AgeInRange()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_RangesOverlap()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void CohortSelection_SpeciesList_RangeContainsAge()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Plant_NoSpecies()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Plant_UnknownSpecies()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void Plant_RepeatedSpecies()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void SingleRepeat_NoCohortsRemoved()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void MultipleRepeat_CohortsRemoved()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_MgmtArea_CodeTooBig()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_MgmtArea_NegativeCode()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Prescription_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Prescription_AppliedTwice()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Prescription_Unknown()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Area_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Area_Negative()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Area_TooBig()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_BeginTime_BeforeScenarioStart()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_BeginTime_AfterScenarioEnd()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_EndTime_BeforeBeginTime()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_EndTime_AfterScenarioEnd()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void HarvestImpl_Extra()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void PrescriptionMaps_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void PrescriptionMaps_NoTimestep()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void BiomassMaps_NoTimestep()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EventLog_Missing()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EventLog_Empty()
        {
            TryParseFileAssociatedWithTest();
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(LineReaderException))]
        public void EventLog_Whitespace()
        {
            TryParseFileAssociatedWithTest();
        }
    }
}
