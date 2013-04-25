package main

import (
	"strconv"
)

type Party struct {
	id   int
	name string
}

type Parties map[int]Party

func NewParties() Parties {
	return make(Parties)
}

func (p Parties) Add(record []string) {
	id, _ := strconv.Atoi(record[0])
	p[id] = Party{id, record[1]}
}

func (p Parties) Load(path string) {
	file_name := path + PARTIES_FILENAME
	loadFile(p, file_name)
}
