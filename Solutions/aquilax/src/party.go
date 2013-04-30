package main

import (
	"strconv"
)

type Party struct {
	id   int
	name string
}

type Parties map[int]*Party

func NewParties() *Parties {
	parties := make(Parties)
	return &parties
}

func (p *Parties) Add(record []string) {
	id, _ := strconv.Atoi(record[0])
	(*p)[id] = &Party{id, record[1]}
}

func (p *Parties) Load(path string) {
	file_name := path + PARTIES_FILENAME
	loadFile(p, file_name)
}

func (p *Parties) Exists(party_id int) bool {
	_, ok := (*p)[party_id]
	return ok
}

func (p *Parties) Remove(party_id int) {
	delete(*p, party_id)
}

func (p *Parties) getCandidateType(candidate_id int) CandType {
	_, ok := (*p)[candidate_id]
	if ok {
		return CANDIDATE_PARTY
	}
	return CANDIDATE_INDEPENDENT
}
