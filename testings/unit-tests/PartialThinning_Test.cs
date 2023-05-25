using Edu.Wisc.Forest.Flel.Util;
using Landis.Extensions.BiomassHarvest;
using Landis.Harvest;
using NUnit.Framework;

namespace Landis.Tests.BiomassHarvest
{
    [TestFixture]
    public class PartialThinning_Test
    {
        private bool eventHandlerCalled;
        private AgeRange expectedRange;
        private Percentage expectedPercentage;

        //---------------------------------------------------------------------

        [TestFixtureSetUp]
        public void Init()
        {
            PartialThinning.ReadAgeOrRangeEvent += AgeOrRangeWasRead;
        }

        //---------------------------------------------------------------------

        [TestFixtureTearDown]
        public void TearDown()
        {
            PartialThinning.ReadAgeOrRangeEvent -= AgeOrRangeWasRead;
        }

        //---------------------------------------------------------------------

        public void AgeOrRangeWasRead(AgeRange   ageRange,
                                      Percentage percentage)
        {
            Assert.AreEqual(expectedRange, ageRange);
            if (expectedPercentage == null)
                Assert.IsNull(percentage);
            else
                Assert.AreEqual((double) expectedPercentage, (double) percentage);
            eventHandlerCalled = true;
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadAgeOrRange_RangePercentage()
        {
            StringReader reader = new StringReader("30-75(10%)");
            int index;
            eventHandlerCalled = false;
            expectedRange = new AgeRange(30, 75);
            expectedPercentage = Percentage.Parse("10%");
            InputValue<AgeRange> ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(0, index);
            Assert.AreEqual(-1, reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadAgeOrRange_RangeWhitespacePercentage()
        {
            StringReader reader = new StringReader(" 1-100 (22.2%)Hi");
            int index;
            eventHandlerCalled = false;
            expectedRange = new AgeRange(1, 100);
            expectedPercentage = Percentage.Parse("22.2%");
            InputValue<AgeRange> ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(1, index);
            Assert.AreEqual('H', reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadAgeOrRange_AgeWhitespacePercentage()
        {
            StringReader reader = new StringReader("66 ( 50% )\t");
            int index;
            eventHandlerCalled = false;
            expectedRange = new AgeRange(66, 66);
            expectedPercentage = Percentage.Parse("50%");
            InputValue<AgeRange> ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(0, index);
            Assert.AreEqual('\t', reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadAgeOrRange_Multiple()
        {
            StringReader reader = new StringReader(" 1-40 (50%)  50(65%)\t 65-70  71-107 ( 15% )  109");
            int index;                            //0123456789_123456789_^123456789_123456789_12345678

            eventHandlerCalled = false;
            expectedRange = new AgeRange(1, 40);
            expectedPercentage = Percentage.Parse("50%");
            InputValue<AgeRange> ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(1, index);
            Assert.AreEqual(' ', reader.Peek());

            eventHandlerCalled = false;
            expectedRange = new AgeRange(50, 50);
            expectedPercentage = Percentage.Parse("65%");
            ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(13, index);
            Assert.AreEqual('\t', reader.Peek());

            eventHandlerCalled = false;
            expectedRange = new AgeRange(65, 70);
            expectedPercentage = null;
            ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(22, index);
            Assert.AreEqual('7', reader.Peek());

            eventHandlerCalled = false;
            expectedRange = new AgeRange(71, 107);
            expectedPercentage = Percentage.Parse("15%");
            ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(29, index);
            Assert.AreEqual(' ', reader.Peek());

            eventHandlerCalled = false;
            expectedRange = new AgeRange(109, 109);
            expectedPercentage = null;
            ageRange = PartialThinning.ReadAgeOrRange(reader, out index);
            Assert.IsTrue(eventHandlerCalled);
            Assert.AreEqual(45, index);
            Assert.AreEqual(-1, reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadPercentage_NoWhitespace()
        {
            StringReader reader = new StringReader("(10%)");
            int index;
            InputValue<Percentage> percentage = PartialThinning.ReadPercentage(reader, out index);
            Assert.IsNotNull(percentage);
            Assert.AreEqual("(10%)", percentage.String);
            Assert.AreEqual(0.10, (double) (percentage.Actual));
            Assert.AreEqual(0, index);
            Assert.AreEqual(-1, reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadPercentage_LeadingWhitespace()
        {
            StringReader reader = new StringReader(" \t (10%)");
            int index;
            InputValue<Percentage> percentage = PartialThinning.ReadPercentage(reader, out index);
            Assert.IsNotNull(percentage);
            Assert.AreEqual("(10%)", percentage.String);
            Assert.AreEqual(0.10, (double) (percentage.Actual));
            Assert.AreEqual(3, index);
            Assert.AreEqual(-1, reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadPercentage_WhitespaceAfterLParen()
        {
            StringReader reader = new StringReader("( 8%)a");
            int index;
            InputValue<Percentage> percentage = PartialThinning.ReadPercentage(reader, out index);
            Assert.IsNotNull(percentage);
            Assert.AreEqual("( 8%)", percentage.String);
            Assert.AreEqual(0.08, (double) (percentage.Actual));
            Assert.AreEqual(0, index);
            Assert.AreEqual('a', reader.Peek());
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadPercentage_Whitespace()
        {
            StringReader reader = new StringReader("( 55.5%\t)");
            int index;
            InputValue<Percentage> percentage = PartialThinning.ReadPercentage(reader, out index);
            Assert.IsNotNull(percentage);
            Assert.AreEqual("( 55.5%\t)", percentage.String);
            Assert.AreEqual(0.555, (double) (percentage.Actual));
            Assert.AreEqual(0, index);
            Assert.AreEqual(-1, reader.Peek());
        }

        //---------------------------------------------------------------------

        private void TryReadPercentage(string input,
                                       string expectedValue,
                                       string expectedError)
        {
            StringReader reader = new StringReader(input);
            int index;
            try {
                InputValue<Percentage> percentage = PartialThinning.ReadPercentage(reader, out index);
            }
            catch (InputValueException exc) {
                Data.Output.WriteLine();
                Data.Output.WriteLine(exc.Message);
                Assert.AreEqual(expectedValue, exc.Value);
                MultiLineText multiLineMesg = exc.MultiLineMessage;
                string lastLine = multiLineMesg[multiLineMesg.Count - 1];
                Assert.AreEqual(expectedError, lastLine.Trim());
                throw exc;
            }
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_Empty()
        {
            TryReadPercentage("",
                              null,
                              "Missing value");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_Missing()
        {
            TryReadPercentage("  \t",
                              null,
                              "Missing value");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_1stCharNotLeftParen()
        {
            TryReadPercentage("  50% )",
                              "50%",
                              "Value does not start with \"(\"");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_NoPercentage()
        {
            TryReadPercentage("  ( \t",
                              "( \t",
                              "No percentage after \"(\"");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_BadNumber()
        {
            TryReadPercentage("  ( 50a%)",
                              "( 50a%",
                              "\"50a\" is not a valid number");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_BelowMin()
        {
            TryReadPercentage("( -1.2%)",
                              "( -1.2%",
                              "-1.2% is not between 0% and 100%");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_AboveMax()
        {
            TryReadPercentage("(123%)",
                              "(123%",
                              "123% is not between 0% and 100%");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_MissingPercent()
        {
            TryReadPercentage("  ( 25 %)",
                              "( 25",
                              "Missing \"%\" at end of percentage value");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_NoRightParen()
        {
            TryReadPercentage("( 22% ",
                              "( 22% ",
                              "Missing \")\"");
        }

        //---------------------------------------------------------------------

        [Test]
        [ExpectedException(typeof(InputValueException))]
        public void ReadPercentage_LastCharNotRightParen()
        {
            TryReadPercentage(" ( 8% XYZ",
                              "( 8% X",
                              "Value ends with \"X\" instead of \")\"");
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_Empty()
        {
            StringReader reader = new StringReader("");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("", word);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(0, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_Whitespace()
        {
            StringReader reader = new StringReader(" \t ");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("", word);
            Assert.AreEqual(' ', reader.Peek());
            Assert.AreEqual(0, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_Age()
        {
            StringReader reader = new StringReader("150");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("150", word);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(3, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_AgeWhiteSpace()
        {
            StringReader reader = new StringReader("70 ");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("70", word);
            Assert.AreEqual(' ', reader.Peek());
            Assert.AreEqual(2, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_AgeLeftParen()
        {
            StringReader reader = new StringReader("200(75%)");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("200", word);
            Assert.AreEqual('(', reader.Peek());
            Assert.AreEqual(3, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_Range()
        {
            StringReader reader = new StringReader("40-110");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("40-110", word);
            Assert.AreEqual(-1, reader.Peek());
            Assert.AreEqual(6, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_RangeWhitespace()
        {
            StringReader reader = new StringReader("1-35\t");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("1-35", word);
            Assert.AreEqual('\t', reader.Peek());
            Assert.AreEqual(4, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_RangeLeftParen()
        {
            StringReader reader = new StringReader("55-90( 10%)\t");
            string word = PartialThinning.ReadWord(reader, '(');
            Assert.AreEqual("55-90", word);
            Assert.AreEqual('(', reader.Peek());
            Assert.AreEqual(5, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_PercentageRightParen()
        {
            StringReader reader = new StringReader("10.5%)\t");
            string word = PartialThinning.ReadWord(reader, ')');
            Assert.AreEqual("10.5%", word);
            Assert.AreEqual(')', reader.Peek());
            Assert.AreEqual(5, reader.Index);
        }

        //---------------------------------------------------------------------

        [Test]
        public void ReadWord_RightParen()
        {
            StringReader reader = new StringReader(")");
            string word = PartialThinning.ReadWord(reader, ')');
            Assert.AreEqual("", word);
            Assert.AreEqual(')', reader.Peek());
            Assert.AreEqual(0, reader.Index);
        }
    }
}
