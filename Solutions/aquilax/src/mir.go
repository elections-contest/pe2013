package main

import (
	"strconv"
)

type Mir struct {
	id       int
	name     string
	mandates int
}

type Mirs map[int]Mir

func NewMirs() Mirs {
	return make(Mirs)
}

func (m Mirs) Add(record []string) {
	id, _ := strconv.Atoi(record[0])
	mandates, _ := strconv.Atoi(record[2])
	m[id] = Mir{id, record[1], mandates}
}

func (m Mirs) Load(path string) {
	file_name := path + MIR_FILENAME
	loadFile(m, file_name)
}
