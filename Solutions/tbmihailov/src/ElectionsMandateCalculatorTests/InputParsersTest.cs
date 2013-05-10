using ElectionsMandateCalculator.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using ElectionsMandateCalculator;
using System.Collections.Generic;
using ElectionsMandateCalculator.Models;

namespace ElectionsMandateCalculatorTests
{


    /// <summary>
    ///This is a test class for InputParsersTest and is intended
    ///to contain all InputParsersTest Unit Tests
    ///</summary>
    [TestClass()]
    public class InputParsersTest
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
        ///A test for ParseMirFromString
        ///</summary>
        [TestMethod()]
        public void Parse_Mir_From_String_Test_1()
        {
            string recordLine = @"1;“МИР 1“;10";
            // 2;“МИР 2“;5
            // 3;“Чужбина“;0";
            Mir expected = new Mir(1, @"“МИР 1“", 10); // TODO: Initialize to an appropriate value
            Mir actual;
            actual = InputParsers.ParseMirFromString(recordLine);
            Assert.AreEqual(expected, actual);
        }



        /// <summary>
        ///A test for ParseCandidateFromString
        ///</summary>
        [TestMethod()]
        public void Parse_Candidate_From_String_Test()
        {
            string recordLine = "7;2;\"К7 на П2 в МИР7\""; // TODO: Initialize to an appropriate value
            //7;2;"К7 на П2 в МИР7"
            //7;3;"К1 на П3 в МИР7"
            Candidate expected = new Candidate(7, 2, "\"К7 на П2 в МИР7\""); // TODO: Initialize to an appropriate value
            Candidate actual;
            actual = InputParsers.ParseCandidateFromString(recordLine);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParsePartyFromString
        ///</summary>
        [TestMethod()]
        public void Parse_Party_From_StringTest_For_Type_Party()
        {
            string recordLine = @"1;“Партия 1“"; // TODO: Initialize to an appropriate value
            /// 1;“Партия 1“
            /// 2;“Коалиция 1“
            /// 1000;“Инициативен комитет в МИР 1“
            /// 1001;“Инициативен комитет в МИР 2“
            Party expected = new Party(1, "“Партия 1“"); // TODO: Initialize to an appropriate value
            Party actual;
            actual = InputParsers.ParsePartyFromString(recordLine);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void Parse_Party_FromString_Test_For_Type_InitiativeComittee()
        {
            string recordLine = @"1000;“Инициативен комитет в МИР 1“"; // TODO: Initialize to an appropriate value
            /// 1;“Партия 1“
            /// 2;“Коалиция 1“
            /// 1000;“Инициативен комитет в МИР 1“
            /// 1001;“Инициативен комитет в МИР 2“
            Party expected = new Party(1000, "“Инициативен комитет в МИР 1“"); // TODO: Initialize to an appropriate value
            Party actual;
            actual = InputParsers.ParsePartyFromString(recordLine);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseVoteFromString
        ///</summary>
        [TestMethod()]
        public void Parse_Vote_From_String_Test()
        {
            string recordLine = "2;5;5500"; // TODO: Initialize to an appropriate value
            //2;5;5500
            //3;1;25678
            Vote expected = new Vote(2, 5, 5500); // TODO: Initialize to an appropriate value
            Vote actual;
            actual = InputParsers.ParseVoteFromString(recordLine);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ParseMirsListFromFileContent
        ///</summary>
        [TestMethod()]
        public void Parse_MirsList_From_FileContent_Test()
        {
            string fileContent = @"1;""МИР1"";5
2;""МИР2"";4
3;""МИР3"";5
4;""МИР4"";10
5;""МИР5"";7
6;""МИР6"";12
7;""МИР7"";7
8;""Извън страната"";0";

            List<Mir> expected = new List<Mir>(){
                new Mir(1,"\"МИР1\"",5),
                new Mir(2,"\"МИР2\"",4),
                new Mir(3,"\"МИР3\"",5),
                new Mir(4,"\"МИР4\"",10),
                new Mir(5,"\"МИР5\"",7),
                new Mir(6,"\"МИР6\"",12),
                new Mir(7,"\"МИР7\"",7),
                new Mir(8,"\"Извън страната\"",0),
            }; 
            List<Mir> actual;
            actual = InputParsers.ParseMirsListFromFileContent(fileContent);
            Assert.IsTrue(CompareHelpers.AreEqualCollections<Mir>(expected, actual));
        }

        [TestMethod()]
        public void Parse_PartiesList_FromFileContent_Test()
        {
            string fileContent = @"1;""П1""
2;""П2""
3;""П3""
4;""П4""
5;""П5""
";

            List<Party> expected = new List<Party>(){
                new Party(1,"\"П1\""),
                new Party(2,"\"П2\""),
                new Party(3,"\"П3\""),
                new Party(4,"\"П4\""),
                new Party(5,"\"П5\""),
            };
            List<Party> actual;
            actual = InputParsers.ParsePartiesListFromFileContent(fileContent);
            Assert.IsTrue(CompareHelpers.AreEqualCollections<Party>(expected, actual));
        }

        [TestMethod()]
        public void Parse_VotesList_From_FileContent_Test()
        {
            string fileContent = @"1;1;35121
1;2;20010
1;3;8456
1;4;18211
1;5;8200
";

            List<Vote> expected = new List<Vote>(){
                new Vote(1,1,35121),
                new Vote(1,2,20010),
                new Vote(1,3,8456),
                new Vote(1,4,18211),
                new Vote(1,5,8200),
            };
            List<Vote> actual;
            actual = InputParsers.ParseVotesListFromFileContent(fileContent);
            Assert.IsTrue(CompareHelpers.AreEqualCollections<Vote>(expected, actual));
        }

        [TestMethod()]
        public void Parse_ResultsList_From_FileContent_Test()
        {
            string fileContent = @"1;1;3
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

            List<Result> expected = new List<Result>(){
                new Result(1,1,3),
                new Result(1,2,1),
                new Result(1,4,1),
                new Result(2,1,1),
                new Result(2,2,1),
                new Result(2,3,1),
                new Result(2,4,1),
                new Result(3,1,1),
                new Result(3,2,2),
                new Result(3,3,1),
                new Result(3,4,1),
                new Result(4,1,4),
                new Result(4,2,2),
                new Result(4,3,3),
                new Result(4,4,1),
                new Result(5,1,2),
                new Result(5,2,2),
                new Result(5,3,2),
                new Result(5,4,1),
                new Result(6,1,4),
                new Result(6,2,4),
                new Result(6,3,3),
                new Result(6,4,1),
                new Result(7,1,3),
                new Result(7,2,2),
                new Result(7,3,1),
                new Result(7,4,1),
            };
            List<Result> actual;
            actual = InputParsers.ParseResultsListFromFileContent(fileContent);
            Assert.IsTrue(CompareHelpers.AreEqualCollections<Result>(expected, actual));
        }
        
    }
}
