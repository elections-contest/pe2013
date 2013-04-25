package main

import (
	"strconv"
)

type CandType int

type Candidate struct {
	mir_id        int
	candidate_id  int
	candidate_num int
	name          string
	cand_type     CandType
}

type Candidates map[string]Candidate

func NewCandidates() Candidates {
	return make(Candidates)
}

func (c Candidates) Add(record []string) {
	mir_id, _ := strconv.Atoi(record[0])
	candidate_id, _ := strconv.Atoi(record[1])
	candidate_num, _ := strconv.Atoi(record[2])
	cand_type := CandType(CANDIDATE_INDEPENDENT)
	if pe.parties.Exists(candidate_id) {
		cand_type = CandType(CANDIDATE_PARTY)
		// Add to list of independent candidates
		pe.global.icandidates.Add(mir_id, candidate_id)
	}
	c[key(mir_id, candidate_id)] = Candidate{mir_id, candidate_id, candidate_num, record[3], cand_type}
}

func (c Candidates) Load(path string) {
	file_name := path + CANDIDATE_FILENAME
	loadFile(c, file_name)
}

func (c Candidates) RemoveParty(party_id int) {
	for key, candidate := range c {
		if candidate.candidate_id == party_id {
			delete(c, key)
			pe.votes.RemoveVotesForCandidate(candidate.mir_id, candidate.candidate_id)
		}
	}
	// Remove votes abroad for party
	pe.votes.RemoveVotesForCandidate(pe.global.abroad_mir_id, party_id)
}
