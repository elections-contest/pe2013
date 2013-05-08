<?php

/**
 * Elections 2013 mandate calculation script
 *
 * @category   Government
 * @package    pe2013
 * @author     Evgeniy Vasilev <aquilax@gmail.com>
 * @license    http://www.opensource.org/licenses/mit-license.html  MIT License
 * @version    1.0
 * @link       https://github.com/aquilax/pe2013
 */
class Pe {
	/**
	 * Filenames
	 */

	const MIR_FILENAME = "MIRs.txt";
	const PARTIES_FILENAME = "Parties.txt";
	const CANDIDATE_FILENAME = "Candidates.txt";
	const VOTE_FILENAME = "Votes.txt";
	const LOT_FILENAME = "Lot.txt";

	/**
	 * Vote barreer
	 */
	const VOTE_BAREER = 0.04;

	/**
	 * Party value offsets
	 */
	const VOTES = 0;
	const PRE_MANDATES_BASE = 1;
	const PRE_MANDATES_EXTRA = 2;
	const HARE_REMAINDER = 3;

	/**
	 * Abroad mir id
	 * @var int
	 */
	private $abroad_mir_id = 0;

	/**
	 * Total mandates counter
	 */
	private $total_mandates = 0;

	/**
	 * Total votes counter
	 */
	private $total_votes = 0;

	/**
	 * Basic data structures
	 */
	private $mirs = array();
	private $party_list = array();
	private $parties = array();
	private $ind_candidates = array(); // independent candidates
	private $lot = array();

	/**
	 * Supporting data structures
	 */
	private $proportional_mandates = array();
	private $party_votes = array();
	private $mir_total_votes = array();
	private $parties_with_extra_mandates = array();

	/**
	 * Starts the whole calculation process
	 * @param string $path Input data path
	 * @param string $result_filename Output file name
	 */
	function start($path, $result_filename) {
		$this->loadData($path);
		$this->processData();
		$this->saveData($result_filename);
	}

	/**
	 * Load input files
	 * @param string $path Input data path
	 */
	function loadData($path) {
		$this->loader($path . self::MIR_FILENAME, 'mirProcessor');
		$this->loader($path . self::PARTIES_FILENAME, 'partyProcessor');
		$this->loader($path . self::CANDIDATE_FILENAME, 'candidateProcessor');
		$this->loader($path . self::VOTE_FILENAME, 'voteProcessor');
		$this->loader($path . self::VOTE_FILENAME, 'lotProcessor', FALSE);
	}

	/**
	 * Common CSV File loadre
	 * @param strint $filename Input filename
	 * @param string $processor Filename processor
	 * @param boolean $mandatory Is the file mandatory
	 * @return boolean True if file exists
	 */
	function loader($filename, $processor, $mandatory = TRUE) {
		if (!file_exists($filename)) {
			if (!$mandatory) {
				// We can live without it
				return FALSE;
			}
			printf("Cannot open file: %s" . PHP_EOL, $filename);
			exit(2);
		}
		if (($handle = fopen($filename, "r")) !== FALSE) {
			while (($data = fgetcsv($handle, 1000, ";")) !== FALSE) {
				call_user_func('self::' . $processor, $data);
			}
			fclose($handle);
		}
		return TRUE;
	}

	/**
	 * Processes the Mir file
	 * @param array $data Input row of data
	 */
	function mirProcessor($data) {
		$mir_id = (int) $data[0];
		$mandates = (int) $data[2];
		$this->mirs[$mir_id] = $mandates;
		if ($mandates === 0) {
			$this->abroad_mir_id = $mir_id;
		}
		$this->total_mandates += $mandates;
	}

	/**
	 * Processes the Parties file
	 * @param array $data Input row of data
	 */
	function partyProcessor($data) {
		$id = (int) $data[0];
		$this->party_list[] = $id;
	}

	/**
	 * Returns the proper data bucket for candidate (party or inie)
	 * @param integer $candidate_id Candidate ID
	 * @return arrray data bucket
	 */
	function &getBucket($candidate_id) {
		if (in_array($candidate_id, $this->party_list)) {
			return $this->parties;
		}
		return $this->ind_candidates;
	}

	/**
	 * Processes the Candidates file
	 * @param array $data Input row of data
	 */
	function candidateProcessor($data) {
		$mir_id = (int) $data[0];
		$candidate_id = (int) $data[1];
		if ($mir_id !== $this->abroad_mir_id) {
			$bucket = &$this->getBucket($candidate_id);
			if (!isset($bucket[$mir_id])) {
				$bucket[$mir_id] = array();
			}
			$bucket[$mir_id][$candidate_id] = 0;
		}
	}

	/**
	 * Processes the Votes file
	 * @param array $data Input row of data
	 */
	function voteProcessor($data) {
		$mir_id = (int) $data[0];
		$candidate_id = (int) $data[1];
		$votes = (int) $data[2];

		if ($mir_id !== $this->abroad_mir_id) {
			$bucket = &$this->getBucket($candidate_id);
			$bucket[$mir_id][$candidate_id] = array();
			$bucket[$mir_id][$candidate_id][self::VOTES] = $votes;
			$this->addNum($this->mir_total_votes, $mir_id, $votes);
		}

		$this->total_votes += $votes;
		$is_party = in_array($candidate_id, $this->party_list);
		if ($is_party) {
			$this->addNum($this->party_votes, $candidate_id, $votes);
		}
	}

	/**
	 * Processes the Lot file
	 * @param array $data Input row of data
	 */
	function lotProcessor($data) {
		$party_id = (int) $data[0];
		$this->lot[] = $party_id;
	}

	/**
	 * Add number to array key helper function
	 * @param array $array Array
	 * @param mixed $id Key
	 * @param integer $num Number to add
	 */
	function addNum(&$array, $id, $num) {
		if (!isset($array[$id])) {
			$array[$id] = 0;
		}
		$array[$id] += $num;
	}

	/**
	 * Main data crunching function
	 */
	function processData() {
		$this->processIndependentCandidates();
		$min_votes = (int) ($this->total_votes * self::VOTE_BAREER);

		$this->removePartiesBelowMinVotesLimit($min_votes);

		$hare_quota = $this->total_votes / $this->total_mandates;

		$this->processPartyProportionalMandates($hare_quota);

		$this->generateHareTable();

		while (!$this->checkSolution()) {
			$this->rearrangeMandates();
		}
		$this->populateResults();
	}

	/**
	 * Process Independent candidates
	 * @return boolean True if there are independent cancidates
	 */
	function processIndependentCandidates() {
		if (count($this->ind_candidates) == 0) {
			return FALSE;
		}
		foreach ($this->ind_candidates as $mir_id => $candidates) {
			$mirIQuota = (int) ($this->mir_total_votes[$mir_id] / $this->mirs[$mir_id]);
			foreach ($candidates as $candidate_id => $values) {
				if ($values[self::VOTES] >= $mirIQuota) {
					$mandates = 1;
					$this->results[] = array($mir_id, $candidate_id, $mandates);
					$this->total_mandates -= $mandates;
					$this->mirs[$mir_id] -= $mandates;
				}
			}
		}
		return TRUE;
	}

	/**
	 * Remove parties with wotes below the VOTE_BAREER
	 * @param integer $min_votes Minimum votes required
	 */
	function removePartiesBelowMinVotesLimit($min_votes) {
		foreach ($this->party_votes as $party_id => $votes) {
			if ($votes < $min_votes) {
				$key = array_search($party_id, $this->party_list);
				if ($key !== FALSE) {
					unset($this->party_list[$key]);
				}
				unset($this->party_votes[$party_id]);
				foreach ($this->parties as $mir_id => $parties) {
					// remove party from all mirs
					if (isset($parties[$party_id])) {
						unset($this->parties[$mir_id][$party_id]);
					}
				}
			}
		}
	}

	/**
	 * Returns the index-th element from an value sorted array, according to
	 * CIK's rules
	 * @param array $arr Associative array of parties and values
	 * @param array $lot The Lot
	 * @param integer $index Index of the element
	 * @param intger $total Remaining numbers to check
	 * @return mixed party_id or FALSE on failiure
	 */
	function getElement(&$arr, &$lot, $index, $total) {
		// get party_id-s
		$keys = array_keys($arr);
		// remainders
		$vals = array_values($arr);
		// the natural result
		$result = $keys[$index];
		if (!isset($vals[$index + 1])) {
			// last element = no duplicates
			return $result;
		}
		// check if totals is exceeding the elements count
		if ($total >= count($vals)) {
			$total = count($vals)-1;
		}
		// check the remaining remainders for duplicates
		if ($vals[$index] != $vals[$total]) {
			// if the first and the last elements differ we're good to go
			return $result;
		}
		// Check the lot
		if ($lot) {
			// fighting parties
			$lot_keys = array_slice($keys, $index, $total - $index);
			foreach ($lot as $party_id) {
				// check the lot and return the first matching party
				if (in_array($party_id, $lot_keys)) {
					// rearrange the array (this is nasty)
					$new_index = array_search($party_id, $keys);
					$t = $keys[$new_index];
					$keys[$index] = $keys[$new_index];
					$keys[$new_index] = $t;
					$arr = array_combine($keys, $vals);
					return $party_id;
				}
			}
		}
		// All hope is lost. Call CIK
		return FALSE;
	}

	/**
	 * Calculates the proprtional mandates for parties
	 * @param float $quota Hare's quota
	 */
	function processPartyProportionalMandates($quota) {
		$this->proportional_mandates = array();
		$remainders = array();
		$pre_total_mandates = 0;
		foreach ($this->party_votes as $party_id => $votes) {
			$party_mandates = $votes / $quota;
			$this->proportional_mandates[$party_id] = (int) $party_mandates;
			$pre_total_mandates += (int) $party_mandates;
			$remainder = $party_mandates - (int) $party_mandates;
			$remainders[$party_id] = $remainder;
		}
		$remaining_mandates = $this->total_mandates - $pre_total_mandates;
		if ($remaining_mandates > 0) {
			// Distribute remaining mandates
			arsort($remainders, SORT_NUMERIC);
			for ($i = 0; $i < $remaining_mandates; $i++) {
				$party_id = $this->getElement($remainders, $this->lot, $i, $remaining_mandates);
				if ($party_id === FALSE) {
					echo '0' . PHP_EOL;
					echo 'Достигнат жребий по Чл. 16.(6)' . PHP_EOL;
					exit(3);
				}
				$this->proportional_mandates[$party_id]++;
			}
		}
	}

	/**
	 * Returns the total votes for MIR
	 * @param integer $mir_id Mir ID
	 * @return integer
	 */
	function getMirTotalVotes($mir_id) {
		$result = 0;
		foreach ($this->parties[$mir_id] as $values) {
			$result += $values[self::VOTES];
		}
		return $result;
	}

	/**
	 * Generates Hare's table
	 */
	function generateHareTable() {
		$pre_totals = array();
		foreach ($this->parties as $mir_id => $parties) {
			$total_mir_votes = $this->getMirTotalVotes($mir_id);
			$mandates = $this->mirs[$mir_id];
			$hare_quota = $total_mir_votes / $mandates;

			$pre_totals[$mir_id] = 0;
			foreach ($parties as $party_id => $values) {
				$mandates = $values[self::VOTES] / $hare_quota;
				$this->parties[$mir_id][$party_id][self::PRE_MANDATES_BASE] = (int) $mandates; // BASE mandates
				$this->parties[$mir_id][$party_id][self::PRE_MANDATES_EXTRA] = 0; // Extra mandates
				$pre_totals[$mir_id] += (int) $mandates;

				$this->parties[$mir_id][$party_id][self::HARE_REMAINDER] = $mandates - (int) $mandates;
			}
		}
		foreach ($this->parties as $mir_id => $parties) {
			$remaining = $this->mirs[$mir_id] - $pre_totals[$mir_id];
			arsort($parties, SORT_NUMERIC);
			for ($i = 0; $i < $remaining; $i++) {
				$party_id = $this->getElement($parties, $this->lot, $i, $remaining);
				if ($party_id === FALSE) {
					echo '0' . PHP_EOL;
					echo 'Достигнат жребий по Чл. 21.(6)' . PHP_EOL;
					exit(4);
				}
				$this->parties[$mir_id][$party_id][self::PRE_MANDATES_EXTRA] = 1;
			}
		}
	}

	/**
	 * Check if the current solution is correct
	 * @return boolean True if solution is correct
	 */
	function checkSolution() {
		$agg = array();
		$result = TRUE;
		$this->parties_with_extra_mandates = array();
		foreach ($this->parties as $parties) {
			foreach ($parties as $party_id => $values) {
				if (!isset($agg[$party_id])) {
					$agg[$party_id] = 0;
				}
				$agg[$party_id] += $values[self::PRE_MANDATES_BASE] + $values[self::PRE_MANDATES_EXTRA];
			}
		}
		foreach ($agg as $party_id => $mandates) {
			$prop_mandate = $this->proportional_mandates[$party_id];
			if ($prop_mandate != $mandates) {
				if ($prop_mandate < $mandates) {
					$this->parties_with_extra_mandates[] = $party_id;
				}
				$result = FALSE;
			}
		}
		return $result;
	}

	/**
	 * Rearranges mandates according to Section 3.3
	 */
	function rearrangeMandates() {
		$min_remainder = 1;
		$min_party_id = 0;
		$min_mir_id = 0;
		foreach ($this->parties as $mir_id => $parties) {
			foreach ($this->parties_with_extra_mandates as $party_id) {
				$values = $parties[$party_id];
				$remainder = $values[self::HARE_REMAINDER];
				if ($remainder > 0  // not used
						&& $remainder < $min_remainder // min
						&& $values[self::PRE_MANDATES_EXTRA] > 0 // has mandate
				) {
					$min_remainder = $remainder;
					$min_party_id = $party_id;
					$min_mir_id = $mir_id;
				}
			}
		}
		$this->parties[$min_mir_id][$min_party_id][self::PRE_MANDATES_EXTRA] -= 1; // decrease extra mandate
		$this->parties[$min_mir_id][$min_party_id][self::HARE_REMAINDER] = -1; // mark as passed
		$party_id = $this->getMaxRemainderForMir($min_mir_id);
		$this->parties[$min_mir_id][$party_id][self::PRE_MANDATES_EXTRA] += 1; // increase extra mandate
	}

	/**
	 * Returns the maximum remainder for MIR
	 * @param integer $mir_id Mir ID
	 * @return float The maximum remainder
	 */
	function getMaxRemainderForMir($mir_id) {
		$max_remainder = 0;
		$max_party_id = 0;
		foreach ($this->parties[$mir_id] as $party_id => $values) {
			if ($values[self::HARE_REMAINDER] > $max_remainder && $values[self::PRE_MANDATES_EXTRA] == 0) {
				$max_remainder = $values[self::HARE_REMAINDER];
				$max_party_id = $party_id;
			}
		}
		return $max_party_id;
	}

	/**
	 * Populates the result array with parties mandates
	 */
	function populateResults() {
		foreach ($this->parties as $mir_id => $parties) {
			foreach ($parties as $party_id => $values) {
				$total = $values[self::PRE_MANDATES_BASE] + $values[self::PRE_MANDATES_EXTRA];
				if ($total) {
					$this->results[] = array($mir_id, $party_id, $total);
				}
			}
		}
	}

	/**
	 * Saves output data
	 * @param string $result_filename Output filename or stdout if empty
	 */
	function saveData($result_filename) {
		$filename = $result_filename ? $result_filename : 'php://stdout';
		$fh = fopen($filename, 'w') or die($php_errormsg);
		foreach ($this->results as $result) {
			fputcsv($fh, $result, ';');
		}
		fclose($fh);
	}

}