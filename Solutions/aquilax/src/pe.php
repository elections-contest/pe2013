#!/usr/bin/php
<?php

error_reporting(E_ALL);

class Pe {

	const MIR_FILENAME       = "MIRs.txt";
	const PARTIES_FILENAME   = "Parties.txt";
	const CANDIDATE_FILENAME = "Candidates.txt";
	const VOTE_FILENAME      = "Votes.txt";

	const VOTE_BAREER = 0.04;

	private $remainder_multiplier = 10000;
	private $abroad_mir_id = 0;
	private $total_mandates = 0;
	private $total_votes = 0;
	private $mirs = array();
	private $party_list = array();
	private $parties = array();
	private $ind_candidates = array(); // independent candidates
	
	private $hare_table = array();

	private $prop_mandates = array();
	private $party_votes = array();
	private $mir_total_votes = array();
	private $sec_3_2_mandates = array();
	private $parties_with_extra_mandates = array();

	function start($path) {
		$this->loadData($path);
		$this->processData();
		$this->saveData();
	}

	function loadData($path) {
		$this->loader($path.self::MIR_FILENAME, 'mirProcessor');
		$this->loader($path.self::PARTIES_FILENAME, 'partyProcessor');
		$this->loader($path.self::CANDIDATE_FILENAME, 'candidateProcessor');
		$this->loader($path.self::VOTE_FILENAME, 'voteProcessor');
	}

	function loader($filename, $processor) {
		if (!file_exists($filename)) {
			printf("Cannot open file: %s".PHP_EOL, $filename);
			exit(2);
		}
		if (($handle = fopen($filename, "r")) !== FALSE) {
	    	while (($data = fgetcsv($handle, 1000, ";")) !== FALSE) {
				call_user_func('self::'.$processor, $data);
			}
			fclose($handle);
		}
	}
	
	function mirProcessor($data) {
		$mir_id = (int)$data[0];
		$mandates = (int)$data[2];
		$this->mirs[$mir_id] = $mandates;
		if ($mandates === 0) {
			$this->abroad_mir_id = $mir_id;
		}
		$this->total_mandates += $mandates;
	}

	function partyProcessor($data) {
		$id = (int)$data[0];
		$this->party_list[] = $id;
	}

	function &getBucket($candidate_id) {
		if (in_array($candidate_id, $this->party_list)) {
			return $this->parties;
		}
		return $this->ind_candidates;
	}

	function candidateProcessor($data) {
		$mir_id = (int)$data[0];
		$candidate_id = (int)$data[1];
		$bucket = &$this->getBucket($candidate_id);
		if (!isset($bucket[$mir_id])) {
			$bucket[$mir_id] = array();
		}
		$bucket[$mir_id][$candidate_id] = 0;
	}

	function voteProcessor($data) {
		$mir_id = (int)$data[0];
		$candidate_id = (int)$data[1];
		$votes = (int)$data[2];
		$is_party = in_array($candidate_id, $this->party_list); 
		$bucket = &$this->getBucket($candidate_id);
		$bucket[$mir_id][$candidate_id] = $votes;
		$this->total_votes += $votes;
		if ($is_party) {
			$this->addNum($this->party_votes, $candidate_id, $votes);
		}
		$this->addNum($this->mir_total_votes, $mir_id, $votes);
	}

	function addNum(&$array, $id, $num) {
		if (!isset($array[$id])) {
			$array[$id] = 0;
		}
		$array[$id] += $num;
	}

	function processData() {
		$this->processIndependentCandidates();
		$min_votes = (int)($this->total_votes * self::VOTE_BAREER);
		
		$this->removePartiesBelowMinVotesLimit($min_votes);

		$hare_quota = $this->total_votes / $this->total_mandates;

		$this->processPartyProportionalMandates($hare_quota);

		//remove abroad
		unset($this->parties[$this->abroad_mir_id]);
		
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
			$mirIQuota = (int)($this->mir_total_votes[$mir_id] / $this->mirs[$mir_id]);
			foreach ($candidates as $candidate_id => $votes) {
				if ($votes >= $mirIQuota) {
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
				foreach($this->parties as $mir_id => $parties) {
					// remove party from all mirs
					if (isset($parties[$party_id])) {
						unset($this->parties[$mir_id][$party_id]);
					}
				}
			}
		}
	}

	function processPartyProportionalMandates($quota) {
		$this->prop_mandates = array();
		$remainders = array();
		$pre_total_mandates = 0;
		foreach ($this->party_votes as $party_id => $votes) {
			$party_mandates = (int)($votes / $quota);
			$this->prop_mandates[$party_id] = $party_mandates;
			$pre_total_mandates += $party_mandates;
			$remainder = (int)((($votes / $quota) - $party_mandates) * $this->remainder_multiplier);
			$remainders[$party_id] =  $remainder;
		}
		$remaining_mandates = $this->total_mandates - $pre_total_mandates;
		if ($remaining_mandates > 0) {
			// Distribute remaining mandates
			// TODO: Consider 16(6)
			arsort($remainders, SORT_NUMERIC);
			$remainders = array_slice($remainders, 0, $remaining_mandates, TRUE);
			foreach ($remainders as $party_id => $remainder) {
				$this->prop_mandates[$party_id]++;
			}
		}
	}

	
	function getMirTotalVotes($mir_id) {
		$result = 0;
		foreach ($this->parties[$mir_id] as $party_id => $votes) {
			$result += $votes;
		}
		return $result;
	}

	function generateHareTable() {
		foreach($this->parties as $mir_id => $parties) {
			$total_mir_votes = $this->getMirTotalVotes($mir_id);
			$mandates = $this->mirs[$mir_id];
			$hare_quota = $total_mir_votes / $mandates;

			$this->sec_3_2_mandates[$mir_id] = array();
			$this->sec_3_2_totals[$mir_id] = 0;
			foreach ($parties as $party_id => $votes) {
				if (!isset($this->hare_table)) {
					$this->hare_table[$mir_id] = array();
				}
				$mandates = $votes / $hare_quota;
				$this->sec_3_2_mandates[$mir_id][$party_id] = array();
				$this->sec_3_2_mandates[$mir_id][$party_id][0] = (int)$mandates; // BASE mandates
				$this->sec_3_2_mandates[$mir_id][$party_id][1] = 0; // Extra mandates
				$this->sec_3_2_totals[$mir_id] += (int)$mandates;

				$this->hare_table[$mir_id][$party_id] = $mandates - (int)$mandates; 
			}
		}
		foreach ($this->hare_table as $mir_id => $parties) {
			$remaining = $this->mirs[$mir_id] - $this->sec_3_2_totals[$mir_id];
			// TODO: Consider 21(6)
			arsort($parties, SORT_NUMERIC);
			$parties = array_slice($parties, 0, $remaining, TRUE);
			foreach ($parties as $party_id => $remainder) {
				$this->sec_3_2_mandates[$mir_id][$party_id][1] = 1;
			}
		}
	}

	function checkSolution() {
		$agg = array();
		$result = TRUE;
		$this->parties_with_extra_mandates = array();
		foreach ($this->sec_3_2_mandates as $mir_id => $parties) {
			foreach ($parties as $party_id => $mandates) {
				if (!isset($agg[$party_id])) {
					$agg[$party_id] = 0;
				}
				$agg[$party_id] += $mandates[0] + $mandates[1];
			}
		}
		foreach($agg as $party_id => $mandates) {
			if ($this->prop_mandates[$party_id] != $mandates) {
				if ($this->prop_mandates[$party_id] < $mandates) {
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
		foreach ($this->hare_table as $mir_id => $parties) {
			foreach ($this->parties_with_extra_mandates as $party_id) {
				$remainder = $parties[$party_id]; 
				if ($remainder > 0  // not used
					&& $remainder < $min_remainder // min
					&& $this->sec_3_2_mandates[$mir_id][$party_id][1] > 0 // has mandate
				) {
					$min_remainder = $parties[$party_id]; 
					$min_party_id = $party_id;
					$min_mir_id = $mir_id;
				}
			}
		}
		$this->sec_3_2_mandates[$min_mir_id][$min_party_id][1] -= 1; // decrease extra mandate
		$this->hare_table[$min_mir_id][$min_party_id] = -1; // mark as passed
		$party_id = $this->getMaxRemainderForMir($min_mir_id);
		$this->sec_3_2_mandates[$min_mir_id][$party_id][1] += 1; // increase extra mandate
	}

	function getMaxRemainderForMir($mir_id) {
		$max_remainder = 0;
		$max_party_id = 0;
		foreach($this->hare_table[$mir_id] as $party_id => $remainder) {
			if ($remainder > $max_remainder && $this->sec_3_2_mandates[$mir_id][$party_id][1] == 0) {
				$max_remainder = $remainder;
				$max_party_id = $party_id;
			}
		}
		return $max_party_id;
	}

	function populateResults() {
		foreach ($this->sec_3_2_mandates as $mir_id => $parties) {
			foreach ($parties as $party_id => $mandates) {
				$total = $mandates[0] + $mandates[1];
				if ($total) {
					$this->results[] = array($mir_id, $party_id, $mandates[0] + $mandates[1]);
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
	print("Usage: php pe.php files_path".PHP_EOL);
	exit(1);
}
?>
