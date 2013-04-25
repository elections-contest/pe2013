package main

import (
	"strconv"
)

/*
type Vote struct {
	mir_id       int
	candidate_id int
	votes        int
}
*/

type MirVotes map[int]int

func NewMirVotes() MirVotes {
	return make(MirVotes)
}

type Votes map[int]MirVotes

func NewVotes() Votes {
	return make(Votes)
}

func (v Votes) Add(record []string) {
	mir_id, _ := strconv.Atoi(record[0])
	candidate_id, _ := strconv.Atoi(record[1])
	votes, _ := strconv.Atoi(record[2])

	mir, ok := v[mir_id]
	if !ok {
		mir = NewMirVotes()
		v[mir_id] = mir
	}
	// set candidate's votes
	mir[candidate_id] = votes

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
	mir, ok := v[mir_id]
	if ok {
		votes, ok := mir[candidate_id]
		if ok {
			delete(mir, candidate_id)
			// Decrease the total votes with the removed candidate's votes
			print(votes)
			print("\t")
			print(pe.global.total_votes)
			print("\n")
			pe.global.total_votes -= votes
		}
		delete(v, mir_id)
	}
}
