package main

import (
	"strconv"
)

type Vote struct {
	mir_id       int
	candidate_id int
	votes        int
}

// Map of Maps should be better here
type Votes []Vote

func NewVotes() Votes {
	return make(Votes, 0, 0)
}

func (v Votes) Add(record []string) {
	mir_id, _ := strconv.Atoi(record[0])
	candidate_id, _ := strconv.Atoi(record[1])
	votes, _ := strconv.Atoi(record[2])
	v = append(v, Vote{mir_id, candidate_id, votes})
	// aggregate
	pe.global.total_votes += votes
	pe.global.candidate_votes.Add(candidate_id, votes)
	pe.global.mir_total_votes.Add(mir_id, votes)
}

func (v Votes) Load(path string) {
	file_name := path + VOTE_FILENAME
	loadFile(v, file_name)
}

func (v Votes) RemoveCandidate(mir_id, candidate_id int) {
	for index, vote := range v {
		if vote.mir_id == mir_id && vote.candidate_id == candidate_id {
			v[index] = v[len(v)-1]
			v = v[0 : len(v)-1]
		}
	}
}
