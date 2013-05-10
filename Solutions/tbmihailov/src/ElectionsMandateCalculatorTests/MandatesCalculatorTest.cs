using ElectionsMandateCalculator.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ElectionsMandateCalculator;
using System.Collections.Generic;
using ElectionsMandateCalculator.Helpers;

namespace ElectionsMandateCalculatorTests
{
    
    
    /// <summary>
    ///This is a test class for MandatesCalculatorTest and is intended
    ///to contain all MandatesCalculatorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MandatesCalculatorTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for CalculateMandates with sample from github electionscontest/pe2013 repo
        ///</summary>
        [TestMethod()]
        public void CalculateMandates_Test_Sample_1_Standard()
        {
            //sample input
            string mirsFileContent = @"1;""МИР1"";5
2;""МИР2"";4
3;""МИР3"";5
4;""МИР4"";10
5;""МИР5"";7
6;""МИР6"";12
7;""МИР7"";7
8;""Извън страната"";0";
            string partiesFileContent = @"1;""П1""
2;""П2""
3;""П3""
4;""П4""
5;""П5""
";
            string votesFileContent = @"1;1;35121
1;2;20010
1;3;8456
1;4;18211
1;5;8200
2;1;23332
2;2;19888
2;3;10200
2;4;15501
2;5;5500
3;1;25678
3;2;27200
3;3;15002
3;4;11521
3;5;1520
4;1;70213
4;2;34556
4;3;41234
4;4;10750
4;5;750
5;1;41111
5;2;32090
5;3;37083
5;4;10882
5;5;805
6;1;75203
6;2;71234
6;3;55222
6;4;11764
6;5;1750
7;1;28800
7;2;23230
7;3;21121
7;4;21411
7;5;11200
8;1;10009
8;2;7512
8;3;5234
8;4;22056
8;5;2350";

            string resultsFileContent = @"1;1;3
1;2;1
1;4;1
2;1;1
2;2;1
2;3;1
2;4;1
3;1;1
3;2;2
3;3;1
3;4;1
4;1;4
4;2;2
4;3;3
4;4;1
5;1;2
5;2;2
5;3;2
5;4;1
6;1;4
6;2;4
6;3;3
6;4;1
7;1;3
7;2;2
7;3;1
7;4;1
";
            //parse collections
            IList<Mir> mirs = InputParsers.ParseMirsListFromFileContent(mirsFileContent); // TODO: Initialize to an appropriate value
            IList<Party> parties = InputParsers.ParsePartiesListFromFileContent(partiesFileContent); // TODO: Initialize to an appropriate value
            IList<Vote> votes = InputParsers.ParseVotesListFromFileContent(votesFileContent); // TODO: Initialize to an appropriate value
            IList<Result> expectedResults = InputParsers.ParseResultsListFromFileContent(resultsFileContent);
            IList<Lot> lots = new List<Lot>();
            MandatesCalculator target = new MandatesCalculator(mirs, parties, votes, lots); // TODO: Initialize to an appropriate value
            target.CalculateMandates();
            var actualResults = target.Results;
            
            Assert.IsTrue(CompareHelpers.AreEqualCollections<Result>(expectedResults, actualResults));
        }

    }
}
