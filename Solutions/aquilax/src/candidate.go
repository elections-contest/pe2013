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

type CandidatesList map[string]*Candidate

type Candidates struct {
	parties *Parties
	votes   *Votes
	list    CandidatesList
}

func NewCandidates(parties *Parties, votes *Votes) *Candidates {
	var candidates Candidates
	candidates.parties = parties
	candidates.votes = votes
	candidates.list = make(CandidatesList)
	return &candidates
}

func (c *Candidates) Add(record []string) {
	mir_id, _ := strconv.Atoi(record[0])
	candidate_id, _ := strconv.Atoi(record[1])
	candidate_num, _ := strconv.Atoi(record[2])
	cand_type := CandType(CANDIDATE_INDEPENDENT)
	if c.parties.Exists(candidate_id) {
		cand_type = CandType(CANDIDATE_PARTY)
		// Add to list of independent candidates
		global.icandidates.Add(mir_id, candidate_id)
	}
	(*c).list[key(mir_id, candidate_id)] = &Candidate{mir_id, candidate_id, candidate_num, record[3], cand_type}
}

func (c *Candidates) Load(path string) {
	file_name := path + CANDIDATE_FILENAME
	loadFile(c, file_name)
}

func (c *Candidates) RemoveParty(party_id int) {
	for key, candidate := range (*c).list {
		if candidate.candidate_id == party_id {
			delete((*c).list, key)
			(*c).votes.RemoveVotesForCandidate(candidate.mir_id, candidate.candidate_id)
		}
	}
	// Remove votes abroad for party
	(*c).votes.RemoveVotesForCandidate(global.abroad_mir_id, party_id)
}
