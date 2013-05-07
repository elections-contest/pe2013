#!/usr/bin/php
<?php
error_reporting(E_ALL);

class Pe {

	/**
	 * Filenames
	 */
	const MIR_FILENAME = "MIRs.txt";
	const PARTIES_FILENAME = "Parties.txt";
	const CANDIDATE_FILENAME = "Candidates.txt";
	const VOTE_FILENAME = "Votes.txt";

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

	private $mirs = array();
	private $party_list = array();
	private $parties = array();
	private $ind_candidates = array(); // independent candidates

	private $proportional_mandates = array();
	private $party_votes = array();
	private $mir_total_votes = array();
	private $parties_with_extra_mandates = array();

	function start($path) {
		$this->loadData($path);
		$this->processData();
		$this->saveData();
	}

	function loadData($path) {
		$this->loader($path . self::MIR_FILENAME, 'mirProcessor');
		$this->loader($path . self::PARTIES_FILENAME, 'partyProcessor');
		$this->loader($path . self::CANDIDATE_FILENAME, 'candidateProcessor');
		$this->loader($path . self::VOTE_FILENAME, 'voteProcessor');
	}

	function loader($filename, $processor) {
		if (!file_exists($filename)) {
			printf("Cannot open file: %s" . PHP_EOL, $filename);
			exit(2);
		}
		if (($handle = fopen($filename, "r")) !== FALSE) {
			while (($data = fgetcsv($handle, 1000, ";")) !== FALSE) {
				call_user_func('self::' . $processor, $data);
			}
			fclose($handle);
		}
	}

	function mirProcessor($data) {
		$mir_id = (int) $data[0];
		$mandates = (int) $data[2];
		$this->mirs[$mir_id] = $mandates;
		if ($mandates === 0) {
			$this->abroad_mir_id = $mir_id;
		}
		$this->total_mandates += $mandates;
	}

	function partyProcessor($data) {
		$id = (int) $data[0];
		$this->party_list[] = $id;
	}

	function &getBucket($candidate_id) {
		if (in_array($candidate_id, $this->party_list)) {
			return $this->parties;
		}
		return $this->ind_candidates;
	}

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

	function addNum(&$array, $id, $num) {
		if (!isset($array[$id])) {
			$array[$id] = 0;
		}
		$array[$id] += $num;
	}

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

	function processIndependentCandidates() {
		if (count($this->ind_candidates) == 0) {
			return;
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
	}

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

	function getElement(&$arr, $index, $total) {
		$keys = array_keys($arr);
		$vals = array_values($arr);
		$result = $keys[$index];
		if (!isset($vals[$index + 1])) {
			// No next element so no duplicates
			return $result;
		}
		// No "next number" if total equals array length
		if ($total >= count($vals)) {
			$total = count($vals) - 1;
		}
		// Check remaining numbers for duplicates
		for ($i = 1; $i < $total + 1; $i++) {
			if ($vals[$index] != $vals[$index + $i]) {
				// difference, return the resulting key
				return $result;
			}
		}
		return FALSE;
	}

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
				$party_id = $this->getElement($remainders, $i, $remaining_mandates);
				if ($party_id === FALSE) {
					echo '0' . PHP_EOL;
					echo 'Достигнат жребий по Чл. 16.(6)' . PHP_EOL;
					exit(3);
				}
				$this->proportional_mandates[$party_id]++;
			}
		}
	}

	function getMirTotalVotes($mir_id) {
		$result = 0;
		foreach ($this->parties[$mir_id] as $values) {
			$result += $values[self::VOTES];
		}
		return $result;
	}

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
				$party_id = $this->getElement($parties, $i, $remaining);
				if ($party_id === FALSE) {
					echo '0' . PHP_EOL;
					echo 'Достигнат жребий по Чл. 21.(6)' . PHP_EOL;
					exit(4);
				}
				$this->parties[$mir_id][$party_id][self::PRE_MANDATES_EXTRA] = 1;
			}
		}
	}

	function checkSolution() {
		//print_r($this->parties);die();
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

	function saveData() {
		foreach ($this->results as $result) {
			printf("%d;%d;%d\n", $result[0], $result[1], $result[2]);
		}
	}

}

if (isset($argv[1]) && file_exists($argv[1])) {
	$path = $argv[1]; //'../../../Tests/1/';
	$pe = new Pe();
	$pe->start($path);
} else {
	print("Usage: php pe.php files_path" . PHP_EOL);
	exit(1);
}
?>
