package main

import (
	"strconv"
)

type MirVotes map[int]int

func NewMirVotes() *MirVotes {
	mir_votes := make(MirVotes)
	return &mir_votes
}

func (mv *MirVotes) TotalVotes() int {
	result := 0
	for _, votes := range *mv {
		result += votes
	}
	return result
}

type Votes map[int]*MirVotes

func NewVotes() *Votes {
	votes := make(Votes)
	return &votes
}

func (v *Votes) Add(record []string) {
	mir_id, _ := strconv.Atoi(record[0])
	candidate_id, _ := strconv.Atoi(record[1])
	votes, _ := strconv.Atoi(record[2])

	mir, ok := (*v)[mir_id]
	if !ok {
		mir = NewMirVotes()
		(*v)[mir_id] = mir
	}
	// set candidate's votes
	(*mir)[candidate_id] = votes

	// set independent votes (if aplicable)
	global.icandidates.SetVote(mir_id, candidate_id, votes)

	// aggregate
	global.total_votes += votes
	global.candidate_votes.Add(candidate_id, votes)
	global.mir_total_votes.Add(mir_id, votes)
}

func (v *Votes) Load(path string) {
	file_name := path + VOTE_FILENAME
	loadFile(v, file_name)
}

func (v *Votes) RemoveVotesForCandidate(mir_id, candidate_id int) {
	mir, ok := (*v)[mir_id]
	if ok {
		votes, ok := (*mir)[candidate_id]
		if ok {
			delete(*mir, candidate_id)
			// Decrease the total votes with the removed candidate's votes
			print(votes)
			print("\t")
			print(global.total_votes)
			print("\n")
			global.total_votes -= votes
		}
		delete(*v, mir_id)
	}
}

func (v *Votes) RemoveAbroad(abroad_mir_id int) {
	delete(*v, abroad_mir_id)
}
