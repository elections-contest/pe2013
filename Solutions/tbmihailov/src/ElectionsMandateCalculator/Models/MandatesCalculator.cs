using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using ElectionsMandateCalculator.Helpers;

namespace ElectionsMandateCalculator.Models
{
    public class MandatesCalculator
    {
        List<Party> _partiesAll;
        List<Mir> _mirsAll;
        List<Vote> _votesAll;
        List<Lot> _lots;
        List<Result> _results;

        private bool _isLotReachedAndNoLots;
        public bool IsLotReachedAndNoLots
        {
            get { return _isLotReachedAndNoLots; }
            private set { _isLotReachedAndNoLots = value; }
        }

        public List<Result> Results
        {
            get { return _results; }
        }

        public MandatesCalculator(IEnumerable<Mir> mirs, IEnumerable<Party> parties, IEnumerable<Vote> votes, IEnumerable<Lot> lots)
        {
            _mirsAll = mirs.OrderBy(m => m.Id).ToList();
            _partiesAll = parties.OrderBy(p => p.Id).ToList();
            _votesAll = new List<Vote>(votes);
            _lots = new List<Lot>(lots);

        }

        int[] _mirMandatesAvailable;//available mandates left

        /// <summary>
        /// Main method that calculate mandates depending on the input data of the current class
        /// </summary>
        /// <returns>Final results list</returns>
        public List<Result> CalculateMandates()
        {
            _results = new List<Result>();

            Logger.Info("Инициализациране на данни");
            int mirsCount;
            int partiesCountTable1;
            bool[] workingPartyFlagsTable1;
            int[,] votesTable1;
            int[] _mirVotesCountTable1;
            int[] _partyVotesCountTable1;
            decimal[] _mirMandateQuotesTable1;
            InitWorkingData(out mirsCount, out partiesCountTable1, out workingPartyFlagsTable1, out votesTable1, out _mirVotesCountTable1, out _partyVotesCountTable1, out _mirMandateQuotesTable1);

            //Стъпка 0 - разпределяне на мандати за независими кандидати
            GiveMandatesToInitiaitiveCandidatesInMirs(mirsCount, partiesCountTable1, workingPartyFlagsTable1, votesTable1, _mirMandateQuotesTable1);

            //Стъпка 1 - разпределяне на мандати за партии и коалиции
            //Изключване на партии, които не преминават 4%-та бариера
            ExcludePartiesThatDoNotPass4PercentBarrier(partiesCountTable1, workingPartyFlagsTable1, _mirVotesCountTable1, _partyVotesCountTable1);
            
            //Инициализиране на данни за рапределяне на мандатите на национално ниво
            int[,] votesTable2;
            Party[] partiesTable2;
            int partiesCountInTable2, partiesCountTable2, allMandatesAfterStep0, allVotesTable2;
            PrepareWorkingDataForNationalMandateGiving(mirsCount, partiesCountTable1, workingPartyFlagsTable1, votesTable1, out partiesCountInTable2, out votesTable2, out partiesTable2, out partiesCountTable2, out allMandatesAfterStep0, out allVotesTable2);

            //==Разпределяне на мандати на национално ниво
            List<PartyCalcInfo> partiesWithCalcInfo;
            List<MirCalcInfo> mirsWithCalcInfo;
            int mandatesLeft;
            //Начални мандати на национално ниво
            GiveInitialMandatesOnNationalLevel(mirsCount, partiesCountTable1, votesTable2, partiesTable2, partiesCountInTable2, partiesCountTable2, allMandatesAfterStep0, allVotesTable2, out partiesWithCalcInfo, out mirsWithCalcInfo, out mandatesLeft);
            //Допълнителни мандати на национално ниво
            GiveAdditionalMandatesOnNationalLevel(partiesCountTable2, ref partiesWithCalcInfo, ref mandatesLeft);

            //==Разпределяне на мандати по райони
            //Разпределяне на мандати по МИР-ове
            MirPartyCalcInfo[,] mirPartyTable = GiveInitialAdditionalMandatesByMirs(mirsCount, votesTable2, partiesCountTable2, partiesWithCalcInfo, mirsWithCalcInfo);
            //Удовлетворяване на всички партии, съобразено с мандатите на национално ниво
            SatisfyAllParties(partiesWithCalcInfo, mirPartyTable);

            //Генериране на резултатите
            GenerateResultsByPartyAndMir(partiesWithCalcInfo, mirsWithCalcInfo, mirPartyTable);

            return _results;
        }

        private void GenerateResultsByPartyAndMir(List<PartyCalcInfo> partiesWithCalcInfo, List<MirCalcInfo> mirsWithCalcInfo, MirPartyCalcInfo[,] mirPartyTable)
        {
            var results = new List<Result>();
            //final mandates
            for (int i = 0; i < mirPartyTable.GetLength(0); i++)
            {
                for (int j = 0; j < mirPartyTable.GetLength(1); j++)
                {
                    if (mirPartyTable[i, j].MandatesGiven > 0)
                    {
                        var finalMandateInfo = new Result()
                        {
                            MirId = mirsWithCalcInfo[i].MirId,
                            PartyId = partiesWithCalcInfo[j].PartyId,
                            MandatesCount = mirPartyTable[i, j].MandatesGiven,
                        };
                        _results.Add(finalMandateInfo);
                    }
                }
            }

            Logger.Info("Краен резултат");
            Logger.Info("МИР, Партия, Получени мандати");
            foreach (var mif in _results)
            {
                Logger.logger.InfoFormat("{0},{1},{2}", mif.MirId, mif.PartyId, mif.MandatesCount);
            }
        }

        private static void SatisfyAllParties(List<PartyCalcInfo> partiesWithCalcInfo, MirPartyCalcInfo[,] mirPartyTable)
        {
            Logger.logger.Info("");
            Logger.logger.Info("== Преразпределяне на мандати за неудовлетворените партии== ");
            Logger.logger.Info("Текущо състояние:");
            Logger.logger.InfoFormat("{0,5} : [{2,15}] {1,10} {3,11}", "No.", "Разпр. от райони", "Нац. ниво", "Необходими");
            foreach (var party in partiesWithCalcInfo)
            {
                Logger.logger.InfoFormat("{0,5} : [{2,15}] {1,10} {3,11}", party.PartyId, party.MandatesByMirsAll, party.MandatesAll, party.MandatesByMirsUnncessary);
            }

            while (partiesWithCalcInfo.Count(p => p.MandatesByMirsUnncessary < 0) > 0)
            {
                int minUsedCoefI = 0;
                int minUsedCoefJ = 0;
                decimal minUsedCoef = 1.0M;
                bool hasMinUsedCoef = false;

                for (int i = 0; i < mirPartyTable.GetLength(0); i++)
                {
                    for (int j = 0; j < mirPartyTable.GetLength(1); j++)
                    {
                        //min used coef
                        if (partiesWithCalcInfo[j].MandatesByMirsUnncessary > 0)
                        {
                            if (mirPartyTable[i, j].IsMandateCoefHareRUsed
                                && !mirPartyTable[i, j].IsMandateCoefHareRUsed2)
                            {
                                if (mirPartyTable[i, j].MandateCoefHareR < minUsedCoef)
                                {
                                    minUsedCoef = mirPartyTable[i, j].MandateCoefHareR;
                                    minUsedCoefI = i;
                                    minUsedCoefJ = j;
                                    hasMinUsedCoef = true;
                                }
                            }
                        }


                    }
                }

                int maxUnsedCoefI = 0;
                int maxUnsedCoefJ = 0;
                decimal maxUnsedCoef = 0.0M;
                bool hasMaxUnusedCoef = false;

                //find max unused coef
                if (hasMinUsedCoef)
                {
                    int i = minUsedCoefI;
                    for (int j = 0; j < mirPartyTable.GetLength(1); j++)
                    {
                        //max unused coef
                        if (!mirPartyTable[i, j].IsMandateCoefHareRUsed
                            && !mirPartyTable[i, j].IsMandateCoefHareRUsed2)
                        {
                            if (mirPartyTable[i, j].MandateCoefHareR > maxUnsedCoef)//> only because parties are sorted by ID
                            {
                                maxUnsedCoef = mirPartyTable[i, j].MandateCoefHareR;
                                maxUnsedCoefI = i;
                                maxUnsedCoefJ = j;
                                hasMaxUnusedCoef = true;
                            }
                        }
                    }

                    //update for mandates
                    if (hasMaxUnusedCoef)
                    {
                        mirPartyTable[minUsedCoefI, minUsedCoefJ].MandatesAdditional--;
                        partiesWithCalcInfo[minUsedCoefJ].MandatesGivenByMirsAdditional--;
                        mirPartyTable[minUsedCoefI, minUsedCoefJ].IsMandateCoefHareRUsed2 = true;

                        mirPartyTable[maxUnsedCoefI, maxUnsedCoefJ].MandatesAdditional++;
                        partiesWithCalcInfo[maxUnsedCoefJ].MandatesGivenByMirsAdditional++;
                        mirPartyTable[maxUnsedCoefI, maxUnsedCoefJ].IsMandateCoefHareRUsed = true;
                    }
                }

                Logger.logger.Info("Текущо състояние:");
                Logger.logger.InfoFormat("{0,5} : [{2,15}] {1,10} {3,11}", "No.", "Разпр. от райони", "Нац. ниво", "Необходими");
                foreach (var party in partiesWithCalcInfo)
                {
                    Logger.logger.InfoFormat("{0,5} : [{2,15}] {1,10} {3,11}", party.PartyId, party.MandatesByMirsAll, party.MandatesAll, party.MandatesByMirsUnncessary);
                }
            }
        }

        private static MirPartyCalcInfo[,] GiveInitialAdditionalMandatesByMirs(int mirsCount, int[,] votesTable2, int partiesCountTable2, List<PartyCalcInfo> partiesWithCalcInfo, List<MirCalcInfo> mirsWithCalcInfo)
        {
            Logger.logger.Info("\r\n== Разпределяне на мандати по райони== ");
            //calculate per mir mandates
            MirPartyCalcInfo[,] mirPartyTable = new MirPartyCalcInfo[mirsCount, partiesCountTable2];
            for (int i = 0; i < mirsCount; i++)
            {
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    int votes = votesTable2[i, j];
                    decimal mandateCoefHare = mirsWithCalcInfo[i].MandateHareQuote != 0 ? decimal.Divide(votes, mirsWithCalcInfo[i].MandateHareQuote) : 0;
                    int mandatesInit = (int)mandateCoefHare;
                    decimal mandateCoefHareR = mandateCoefHare - mandatesInit;

                    var mirparty = new MirPartyCalcInfo()
                    {
                        MandateCoefHareR = mandateCoefHareR,
                        MandatesInit = mandatesInit,
                        IsMandateCoefHareRUsed = false,
                    };

                    mirsWithCalcInfo[i].MandatesGivenInit += mandatesInit;
                    partiesWithCalcInfo[j].MandatesGivenByMirsInit += mandatesInit;
                    mirPartyTable[i, j] = mirparty;//for indexed table access
                }

                //additional mandates
                while (mirsWithCalcInfo[i].MandatesGivenAll < mirsWithCalcInfo[i].MandatesLimit)
                {
                    int maxInd = 0;
                    decimal maxR = 0;
                    for (int j = 0; j < partiesCountTable2; j++)
                    {
                        if (!mirPartyTable[i, j].IsMandateCoefHareRUsed && maxR < mirPartyTable[i, j].MandateCoefHareR)
                        {
                            maxR = mirPartyTable[i, j].MandateCoefHareR;
                            maxInd = j;
                        }
                    }
                    mirPartyTable[i, maxInd].MandatesAdditional++;
                    mirPartyTable[i, maxInd].IsMandateCoefHareRUsed = true;

                    partiesWithCalcInfo[maxInd].MandatesGivenByMirsAdditional++;
                    mirsWithCalcInfo[i].MandatesGivenAdditional++;
                }

            }

            Logger.logger.Info("Разпределени мандати по райони:");
            for (int i = 0; i < mirsCount; i++)
            {
                StringBuilder sbLine = new StringBuilder();
                int sum = 0;
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    sbLine.AppendFormat("{0, 6}", mirPartyTable[i, j].MandatesInit);
                    sum += mirPartyTable[i, j].MandatesInit;
                }
                sbLine.AppendFormat("[{0,2}]", sum);
                sbLine.AppendFormat(" от максимални [{0,2}]", mirsWithCalcInfo[i].MandatesLimit);

                Logger.logger.Info(sbLine.ToString());
            }

            Logger.logger.Info("Остатъци по райони - R:");
            Logger.logger.Info("* - получават допълнителен мандат");
            for (int i = 0; i < mirsCount; i++)
            {
                StringBuilder sbLine = new StringBuilder();
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    sbLine.AppendFormat("{0, 6}", (mirPartyTable[i, j].IsMandateCoefHareRUsed ? "*" : "") + mirPartyTable[i, j].MandateCoefHareR.ToString("0.00"));
                }
                Logger.logger.Info(sbLine.ToString());
            }

            Logger.logger.Info("Допълнителни мандати по райони:");
            for (int i = 0; i < mirsCount; i++)
            {
                StringBuilder sbLine = new StringBuilder();
                int sum = 0;
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    sbLine.AppendFormat("{0, 6}", mirPartyTable[i, j].MandatesAdditional);
                    sum += mirPartyTable[i, j].MandatesAdditional;
                }

                sbLine.AppendFormat("[{0,2}]", sum);
                sbLine.AppendFormat(" от максимални [{0,2}]", mirsWithCalcInfo[i].MandatesLimit); ;

                Logger.logger.Info(sbLine.ToString());
            }

            Logger.logger.Info("Мандати по райони:");
            for (int i = 0; i < mirsCount; i++)
            {
                StringBuilder sbLine = new StringBuilder();
                int sum = 0;
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    sbLine.AppendFormat("{0, 6}", mirPartyTable[i, j].MandatesGiven);
                    sum += mirPartyTable[i, j].MandatesGiven;
                }
                sbLine.AppendFormat("[{0,2}]", sum);
                sbLine.AppendFormat(" от [{0,2}]", mirsWithCalcInfo[i].MandatesLimit);
                Logger.logger.Info(sbLine.ToString());
            }
            return mirPartyTable;
        }

        private void GiveAdditionalMandatesOnNationalLevel(int partiesCountTable2, ref List<PartyCalcInfo> partiesWithCalcInfo, ref int mandatesLeft)
        {
            Logger.Info("\r\n==Разпределяне на допълнителни мандати на национално ниво==");
            var partiesOrderedByCoefR = partiesWithCalcInfo.OrderByDescending(p => p.MandateCoefHareR).ToArray();
            int mi = 0;
            while (mandatesLeft > 0)
            {
                decimal currR = partiesOrderedByCoefR[mi].MandateCoefHareR;
                int k = 0;
                int j = mi + 1;
                while (j < partiesCountTable2 && partiesOrderedByCoefR[j].MandateCoefHareR == currR)
                {
                    k++;
                }
                if (k > mandatesLeft)
                {
                    Logger.Info("Достигнат е случай на партии с равни коефициенти:");
                    for (int p = mi; p < mi + k; p++)
                    {
                        Logger.Info(partiesOrderedByCoefR[p].ToString());
                    }

                    if (_lots.Count > 0)
                    {
                        if (_lots.Count != mandatesLeft)
                        {
                            throw new ArgumentException(string.Format("Броя партии избрани със жребии({0}) е различен от останалите мандати - {1}", _lots.Count, mandatesLeft));
                        }
                        foreach (var lot in _lots)
                        {
                            var partyToGiveMandate = partiesOrderedByCoefR.First(p => p.PartyId == lot.PartyId);
                            if (partyToGiveMandate == null)
                            {
                                throw new ArgumentException("Невалиден жребии за партия с ID:" + lot.PartyId);
                            }
                            partyToGiveMandate.MandatesGivenAdditional++;
                            mandatesLeft--;
                        }
                    }
                    else
                    {
                        Logger.Info("Достигнат е жребии, за който няма резултати!");
                        IsLotReachedAndNoLots = true;
                    }
                }
                else
                {
                    partiesOrderedByCoefR[mi].MandatesGivenAdditional++;
                    mi++;
                    mandatesLeft--;
                };
            }

            partiesWithCalcInfo = partiesOrderedByCoefR.OrderBy(p => p.Index).ToList();

            Logger.logger.Info("Крайно разпределяне на мандати по партии:");
            LogNationalMandatesByParties(partiesWithCalcInfo);
        }

        private void GiveInitialMandatesOnNationalLevel(int mirsCount, int partiesCountTable1, int[,] votesTable2, Party[] partiesTable2, int partiesCountInTable2, int partiesCountTable2, int allMandatesAfterStep0, int allVotesTable2, out List<PartyCalcInfo> partiesWithCalcInfo, out List<MirCalcInfo> mirsWithCalcInfo, out int mandatesLeft)
        {
            Logger.Info("\r\n==Разпределяне на мандатите на национално ниво==");
            decimal globalHareQuote = (decimal)allVotesTable2 / allMandatesAfterStep0;
            Logger.logger.InfoFormat("Квота на Хеър = {0} = {1}/{2}", globalHareQuote, allVotesTable2, allMandatesAfterStep0);

            //parties with calc info
            partiesWithCalcInfo = new List<PartyCalcInfo>();
            for (int i = 0; i < partiesCountInTable2; i++)
            {
                var partyWithCalcInfo = new PartyCalcInfo()
                {
                    Index = i,
                    PartyId = partiesTable2[i].Id,
                };
                partiesWithCalcInfo.Add(partyWithCalcInfo);

            }

            //calculate party mir mandates
            mirsWithCalcInfo = new List<MirCalcInfo>();
            for (int i = 0; i < mirsCount; i++)
            {
                var mirCalcInfo = new MirCalcInfo()
                {
                    MirId = _mirsAll[i].Id,
                    MirIndex = i,
                };
                mirsWithCalcInfo.Add(mirCalcInfo);
            }

            //calculate mir and partyVotes votes
            int[] mirVotesCountTable2 = new int[mirsCount];
            int[] partyVotesCountTable2 = new int[partiesCountTable1];
            for (int i = 0; i < mirsCount; i++)
            {
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    //mir votes
                    mirVotesCountTable2[i] += votesTable2[i, j];
                    mirsWithCalcInfo[i].Votes = mirVotesCountTable2[i];
                    mirsWithCalcInfo[i].MandatesLimit = _mirMandatesAvailable[i];
                    mirsWithCalcInfo[i].MandateHareQuote = _mirMandatesAvailable[i] != 0 ? decimal.Divide(mirVotesCountTable2[i], _mirMandatesAvailable[i]) : 0;
                    //party votes
                    partyVotesCountTable2[j] += votesTable2[i, j];
                    partiesWithCalcInfo[j].Votes = partyVotesCountTable2[j];
                }
            }

            //mandates that every party should have
            for (int i = 0; i < partiesCountTable2; i++)
            {
                decimal mandateCoefHare = decimal.Divide(partyVotesCountTable2[i], globalHareQuote);
                int mandatesInit = (int)mandateCoefHare;
                partiesWithCalcInfo[i].MandatesGivenInit = mandatesInit;
                decimal mandateCoefHareR = mandateCoefHare - mandatesInit;
                partiesWithCalcInfo[i].MandateCoefHareR = mandateCoefHareR;
            }

            //summary init mandates given
            int mandatesInitGiven = partiesWithCalcInfo.Sum(p => p.MandatesGivenInit);
            mandatesLeft = allMandatesAfterStep0 - mandatesInitGiven;

            Logger.logger.Info("Начални мандати по партии:");
            LogNationalMandatesByParties(partiesWithCalcInfo);
        }

        private void PrepareWorkingDataForNationalMandateGiving(int mirsCount, int partiesCountTable1, bool[] workingPartyFlagsTable1, int[,] votesTable1, out int partiesCountInTable2, out int[,] votesTable2, out Party[] partiesTable2, out int partiesCountTable2, out int allMandatesAfterStep0, out int allVotesTable2)
        {
            //GENERATE TABLE2
            partiesCountInTable2 = workingPartyFlagsTable1.Count(f => f);

            //Votes table
            votesTable2 = new int[mirsCount, partiesCountInTable2];
            int[,] givenMandatesTable2 = new int[mirsCount, partiesCountInTable2];
            List<Party> partiesListTable2 = new List<Party>();
            int partyIndexTable2 = 0;
            for (int j = 0; j < partiesCountTable1; j++)//TO DO - optimize by j
            {
                if (workingPartyFlagsTable1[j])
                {
                    for (int i = 0; i < mirsCount; i++)
                    {
                        votesTable2[i, partyIndexTable2] = votesTable1[i, j];
                        givenMandatesTable2[i, partyIndexTable2] = 0;//_givenMandatesTable1[i, j];
                    }

                    partiesListTable2.Add(_partiesAll[j]);
                    partyIndexTable2++;
                }
            }

            partiesTable2 = partiesListTable2.ToArray();//indexed array with parties in Table2
            partiesCountTable2 = partiesListTable2.Count;
            //calculatae new quote - Hare
            allMandatesAfterStep0 = _mirMandatesAvailable.Sum();
            allVotesTable2 = 0;//chl.14-chl16
            for (int i = 0; i < mirsCount; i++)
            {
                for (int j = 0; j < partiesCountTable2; j++)
                {
                    allVotesTable2 += votesTable2[i, j];
                }
            }
        }

        private void ExcludePartiesThatDoNotPass4PercentBarrier(int partiesCountTable1, bool[] workingPartyFlagsTable1, int[] _mirVotesCountTable1, int[] _partyVotesCountTable1)
        {
            int allVotesCount = _mirVotesCountTable1.Sum();
            Logger.logger.Info("\r\n==Определяне на минималния брой гласове за допускане==");
            //Calculate 4% barrier
            decimal fourPercentBarrier = 0.04M * allVotesCount;
            Logger.logger.InfoFormat("4% бариера = {0} вота", fourPercentBarrier);

            Logger.logger.Info("\r\n==Определяне на партиите, които преминават бариерата от 4%==");
            Logger.logger.Info("Изключени партии:");
            for (int i = 0; i < partiesCountTable1; i++)
            {
                if (!workingPartyFlagsTable1[i])
                {
                    continue;
                }
                int partyVotes = _partyVotesCountTable1[i];
                if (partyVotes < fourPercentBarrier)
                {
                    workingPartyFlagsTable1[i] = false;
                    Logger.logger.InfoFormat("Изключена партия:\n {0} \n", _partiesAll[i].ToString());
                    Logger.logger.InfoFormat("Причина: {0} има {1} вота {2} {3}", _partiesAll[i].DisplayName, partyVotes, partyVotes < fourPercentBarrier ? "<" : ">=", fourPercentBarrier);
                }
            }
        }

        private void GiveMandatesToInitiaitiveCandidatesInMirs(int mirsCount, int partiesCountTable1, bool[] workingPartyFlagsTable1, int[,] votesTable1, decimal[] _mirMandateQuotesTable1)
        {
            Logger.logger.Info("\r\n==Определяне на мандати за независими кандидате==");
            //check which INITIATIVE candidates pass the mir quote and give them a MANDATE
            int initPartiesCount = _partiesAll.Count(p => p.Type == PartyType.InitCommittee);

            int[,] _givenMandatesTable1 = new int[mirsCount, partiesCountTable1];
            Logger.logger.InfoFormat("Брой инициативни комитети: {0}", initPartiesCount);
            if (initPartiesCount > 0)
            {
                for (int i = 0; i < partiesCountTable1; i++)
                {
                    if (_partiesAll[i].Type != PartyType.InitCommittee)
                    {
                        continue;
                    }

                    for (int j = 0; j < mirsCount; j++)
                    {
                        int partyMirVotesCnt = votesTable1[j, i];
                        decimal mirMandateQuote = _mirMandateQuotesTable1[j];

                        if (partyMirVotesCnt > mirMandateQuote)
                        {
                            Logger.logger.InfoFormat("Инициативен комитет {0} има {1} вота в МИР {2} ({3} от {4}) и получава мандат", _partiesAll[i].DisplayName, partyMirVotesCnt, _mirsAll[j].DisplayName, partyMirVotesCnt >= mirMandateQuote ? ">=" : "<", mirMandateQuote);
                            _givenMandatesTable1[j, i] += 1;
                            _mirMandatesAvailable[j] -= 1;
                            //INITITIVE COMMITTEE can have only 1 mandate in only 1 MIR
                            _results.Add(new Result { MirId = _mirsAll[i].Id, PartyId = _partiesAll[i].Id, MandatesCount = 1 });
                        }
                        workingPartyFlagsTable1[i] = false;
                        //Logger.logger.InfoFormat("{0} excluded from working parties", _parties[i].DisplayName);
                    }
                }
            }
            else
            {
                Logger.Info("Няма разпределяне на мандати за независими кандидати");
            }
        }

        private void InitWorkingData(out int mirsCount, out int partiesCountTable1, out bool[] workingPartyFlagsTable1, out int[,] votesTable1, out int[] _mirVotesCountTable1, out int[] _partyVotesCountTable1, out decimal[] _mirMandateQuotesTable1)
        {
            mirsCount = _mirsAll.Count();
            partiesCountTable1 = _partiesAll.Count();

            workingPartyFlagsTable1 = new bool[partiesCountTable1];
            for (int i = 0; i < partiesCountTable1; i++)
            {
                workingPartyFlagsTable1[i] = true;
            }

            //associate index from array to mir
            Dictionary<int, int> mirIndeces = new Dictionary<int, int>();
            for (int i = 0; i < mirsCount; i++)
            {
                mirIndeces.Add(_mirsAll[i].Id, i);
            }

            //associate index from array to mir
            Dictionary<int, int> partyIndecesTable1 = new Dictionary<int, int>();
            for (int i = 0; i < partiesCountTable1; i++)
            {
                partyIndecesTable1.Add(_partiesAll[i].Id, i);
            }

            //fill votes table
            votesTable1 = new int[mirsCount, partiesCountTable1];//all votes per party per mir
            foreach (var vote in _votesAll)
            {
                votesTable1[mirIndeces[vote.MirId], partyIndecesTable1[vote.PartyId]] = vote.Count;
            }

            //set initial mir mandates available
            _mirMandatesAvailable = _mirsAll.Select(mir => mir.MandatesLimit).ToArray();

            //calculate mir and partyVotes votes
            _mirVotesCountTable1 = new int[mirsCount];
            _partyVotesCountTable1 = new int[partiesCountTable1];
            for (int i = 0; i < mirsCount; i++)
            {
                for (int j = 0; j < partiesCountTable1; j++)
                {
                    _mirVotesCountTable1[i] += votesTable1[i, j];
                    _partyVotesCountTable1[j] += votesTable1[i, j];
                }
            }

            //calculate _mirMandateQuote
            _mirMandateQuotesTable1 = new decimal[mirsCount];
            for (int i = 0; i < mirsCount; i++)
            {
                if (_mirMandatesAvailable[i] == 0)
                {
                    _mirMandateQuotesTable1[i] = 0;
                }
                else
                {
                    _mirMandateQuotesTable1[i] = (decimal)_mirVotesCountTable1[i] / _mirMandatesAvailable[i];
                }
            }
        }

        private static void LogNationalMandatesByParties(List<PartyCalcInfo> partiesWithCalcInfo)
        {
            Logger.logger.Info(string.Format("{0,6} {1,8} {2,8} {3,8} {4,4} {5,5}", "Номер", "Гласове", "Начални", "Остатък", "Доп", "Общо"));
            foreach (var party in partiesWithCalcInfo)
            {
                StringBuilder sbMFinal = new StringBuilder();
                sbMFinal.AppendFormat("{0,6} {1,8} {2,8} {3,8} {4,4} {5,5}", party.PartyId, party.Votes, party.MandatesGivenInit, party.MandateCoefHareR.ToString("0.000"), party.MandatesGivenAdditional, party.MandatesAll);
                Logger.logger.Info(sbMFinal.ToString());
            }
        }
    }

    public class MirCalcInfo
    {
        public int MirId { get; set; }
        public int MirIndex { get; set; }

        public int Votes { get; set; }
        public int MandatesLimit { get; set; }
        public decimal MandateHareQuote { get; set; }
        public int MandatesGivenInit { get; set; }
        public int MandatesGivenAdditional { get; set; }
        public int MandatesGivenAll { get { return MandatesGivenInit + MandatesGivenAdditional; } }

        List<MirPartyCalcInfo> _mirPartyInfos;
        public List<MirPartyCalcInfo> MirPartyInfos
        {
            get
            {
                if (_mirPartyInfos == null)
                {
                    _mirPartyInfos = new List<MirPartyCalcInfo>();
                }
                return _mirPartyInfos;
            }
            set { _mirPartyInfos = value; }
        }
    }

    class PartyCalcInfo
    {
        public int PartyId { get; set; }
        public int Index { get; set; }
        public int Votes { get; set; }

        //public decimal MandateCoefHare { get; set; }
        public decimal MandateCoefHareR { get; set; }

        //national
        public int MandatesGivenInit { get; set; }
        public int MandatesGivenAdditional { get; set; }
        public int MandatesAll { get { return MandatesGivenInit + MandatesGivenAdditional; } }

        //by mirs
        public int MandatesGivenByMirsInit { get; set; }
        public int MandatesGivenByMirsAdditional { get; set; }
        public int MandatesByMirsAll { get { return MandatesGivenByMirsInit + MandatesGivenByMirsAdditional; } }

        public int MandatesByMirsUnncessary { get { return MandatesByMirsAll - MandatesAll; } }
    }

    public class MirPartyCalcInfo
    {
        //public int PartyId { get; set; }
        //public int PartyIndex { get; set; }
        //public PartyCalcInfo Party { get; set; }

        //public int MirId { get; set; }
        //public int MirIndex { get; set; }
        //public MirCalcInfo Mir { get; set; }

        public int MandatesInit { get; set; }
        public int MandatesAdditional { get; set; }
        public int MandatesGiven { get { return MandatesInit + MandatesAdditional; } }
        public int MandatesLimit { get; set; }

        public decimal MandateCoefHareR { get; set; }
        public bool IsMandateCoefHareRUsed { get; set; }

        public bool IsMandateCoefHareRUsed2 { get; set; }//used in last step for unnecessary mandates
    }


}
