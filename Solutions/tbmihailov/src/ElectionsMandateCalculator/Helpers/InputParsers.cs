using ElectionsMandateCalculator.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionsMandateCalculator.Helpers
{
    public class InputParsers
    {
        private const char SEPARATOR = ';';

        #region single row model parsing
        /// <summary>
        /// Parse Mir from Mirs.txt
        /// ex:
        /// 1;“МИР 1“;10
        /// 2;“МИР 2“;5
        /// 3;“Чужбина“;0
        /// </summary>
        /// <param name="recordLine"></param>
        /// <returns></returns>
        public static Mir ParseMirFromString(string recordLine)
        {
            if (string.IsNullOrEmpty(recordLine))
            {
                throw new ArgumentNullException();
            }

            var propValues = recordLine.Split(SEPARATOR);

            var item = new Mir(
                                id: int.Parse(propValues[0]),
                                name: propValues[1],//.Substring(1,propValues[1].Length-2),
                                mandatesLimit: int.Parse(propValues[2])
                            );

            return item;
        }

        /// <summary>
        /// Parse Parties from Parties.txt
        /// ex:
        /// 1;“Партия 1“
        /// 2;“Коалиция 1“
        /// 1000;“Инициативен комитет в МИР 1“
        /// 1001;“Инициативен комитет в МИР 2“
        /// </summary>
        /// <param name="recordLine"></param>
        /// <returns></returns>
        public static Party ParsePartyFromString(string recordLine)
        {
            if (string.IsNullOrEmpty(recordLine))
            {
                throw new ArgumentNullException();
            }

            var propValues = recordLine.Split(SEPARATOR);

            var item = new Party(
                                id: int.Parse(propValues[0]),
                                name: propValues[1]
                           );

            return item;
        }

        /// <summary>
        /// Parsing line from Candidates.txt
        /// ex:
        /// Mir/Party/Sequence num/name
        /// 1;1;1;“Кандидат 1 в МИР 1 – Партия 1“
        /// 1;1;2;“Кандидат 2 в МИР 1 – Партия 1“
        /// </summary>
        /// <param name="recordLine"></param>
        /// <returns></returns>
        public static Candidate ParseCandidateFromString(string recordLine)
        {
            if (string.IsNullOrEmpty(recordLine))
            {
                throw new ArgumentNullException();
            }

            var propValues = recordLine.Split(SEPARATOR);

            var item = new Candidate(
                                mirId: int.Parse(propValues[0]),
                                partyId: int.Parse(propValues[1]),
                                name: propValues[2]
                            );

            return item;
        }

        /// <summary>
        /// Parse Vote from Votes.txt
        /// ex:
        /// 1;1;1000
        /// 1;2;500
        /// 1;1000;600
        /// </summary>
        /// <param name="recordLine"></param>
        /// <returns></returns>
        public static Vote ParseVoteFromString(string recordLine)
        {
            if (string.IsNullOrEmpty(recordLine))
            {
                throw new ArgumentNullException();
            }

            var propValues = recordLine.Split(SEPARATOR);

            var item = new Vote(
                                mirId: int.Parse(propValues[0]),
                                partyId: int.Parse(propValues[1]),
                                count: int.Parse(propValues[2])
                           );

            return item;
        }

        /// <summary>
        /// Parse result from sample result file
        /// 6;3;3
        /// 6;4;1
        /// </summary>
        /// <param name="recordLine"></param>
        /// <returns></returns>
        public static Result ParseResultFromString(string recordLine)
        {
            if (string.IsNullOrEmpty(recordLine))
            {
                throw new ArgumentNullException();
            }

            var propValues = recordLine.Split(SEPARATOR);

            var item = new Result(
                                mirId: int.Parse(propValues[0]),
                                partyId: int.Parse(propValues[1]),
                                mandatesCount: int.Parse(propValues[2])
                           );

            return item;
        }
        #endregion

        #region parsing files
        public static List<Mir> ParseMirsListFromFile(string fileName)
        {
            var itemsList = new List<Mir>();

            string line;
            // Read the file and display it line by line.
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var item = ParseMirFromString(line);
                    itemsList.Add(item);
                }
            }

            return itemsList;
        }

        public static List<Party> ParsePartiesListFromFile(string fileName)
        {
            var itemsList = new List<Party>();

            string line;
            // Read the file and display it line by line.
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var item = ParsePartyFromString(line);
                    itemsList.Add(item);
                }
            }

            return itemsList;
        }

        public static List<Candidate> ParseCandidatesListFromFile(string fileName)
        {
            var itemsList = new List<Candidate>();

            string line;
            // Read the file and display it line by line.
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                int currMirId = -1;
                int currPartyId = -1;
                int currCandidateIndex = 0;

                while ((line = file.ReadLine()) != null)
                {
                    var item = ParseCandidateFromString(line);
                    if ((item.MirId != currMirId)
                        || (item.PartyId != currPartyId))
                    {
                        currMirId = item.MirId;
                        currPartyId = item.PartyId;
                        currCandidateIndex = 0;
                    }

                    currCandidateIndex++;
                    item.SeqNum = currCandidateIndex;

                    itemsList.Add(item);
                }
            }

            return itemsList;
        }

        public static List<Vote> ParseVotesListFromFile(string fileName)
        {
            var itemsList = new List<Vote>();

            string line;
            // Read the file and display it line by line.
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var item = ParseVoteFromString(line);
                    itemsList.Add(item);
                }
            }

            return itemsList;
        }

        public static List<Lot> ParseLotsListFromFile(string fileName)
        {
            var itemsList = new List<Lot>();

            string line;
            // Read the file and display it line by line.
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    int lotPartyId = int.Parse(line);
                    var item = new Lot(lotPartyId);
                    itemsList.Add(item);
                }
            }

            return itemsList;
        }

        public static List<Result> ParseResultsListFromFile(string fileName)
        {
            var itemsList = new List<Result>();

            string line;
            // Read the file and display it line by line.
            using (System.IO.StreamReader file = new System.IO.StreamReader(fileName))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var item = ParseResultFromString(line);
                    itemsList.Add(item);
                }
            }

            return itemsList;
        }
        #endregion

        #region parse content form sample files (used for unit testing)
        public static List<Mir> ParseMirsListFromFileContent(string fileContent)
        {
            var itemsList = new List<Mir>();

            var contentLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in contentLines)
            {
                var item = ParseMirFromString(line);
                itemsList.Add(item);
            }

            return itemsList;
        }

        public static List<Party> ParsePartiesListFromFileContent(string fileContent)
        {
            var itemsList = new List<Party>();

            var contentLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in contentLines)
            {
                var item = ParsePartyFromString(line);
                itemsList.Add(item);
            }

            return itemsList;
        }

        public static List<Candidate> ParseCandidatesListFromFileContent(string fileContent)
        {
            var itemsList = new List<Candidate>();

            int currMirId = -1;
            int currPartyId = -1;
            int currCandidateIndex = 0;

            var contentLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in contentLines)
            {
                var item = ParseCandidateFromString(line);
                if ((item.MirId != currMirId)
                    || (item.PartyId != currPartyId))
                {
                    currMirId = item.MirId;
                    currPartyId = item.PartyId;
                    currCandidateIndex = 0;
                }

                currCandidateIndex++;
                item.SeqNum = currCandidateIndex;

                itemsList.Add(item);
            }

            return itemsList;
        }

        public static List<Vote> ParseVotesListFromFileContent(string fileContent)
        {
            var itemsList = new List<Vote>();

            // Read the file and display it line by line.
            var contentLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in contentLines)
            {
                var item = ParseVoteFromString(line);
                itemsList.Add(item);
            }

            return itemsList;
        }

        public static List<Result> ParseResultsListFromFileContent(string fileContent)
        {
            var itemsList = new List<Result>();

            // Read the file and display it line by line.
            var contentLines = fileContent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in contentLines)
            {
                var item = ParseResultFromString(line);
                itemsList.Add(item);
            }

            return itemsList;
        }
        #endregion
    }
}
