package main

import (
	"strconv"
)

type Vote struct {
	mir_id       int
	candidate_id int
	votes        int
}

type Votes map[int]Vote

func NewVotes() Votes {
	return make(Votes)
}

func (v Votes) Add(record []string) {
	mir_id, _ := strconv.Atoi(record[0])
	candidate_id, _ := strconv.Atoi(record[1])
	votes, _ := strconv.Atoi(record[2])
	v[mir_id] = Vote{mir_id, candidate_id, votes}
	global.total_votes += votes
	global.party_votes.Add(candidate_id, votes)
}

func (v Votes) Load(path string) {
	file_name := path + VOTE_FILENAME
	loadFile(v, file_name)
}
