package main

import (
	"bufio"
	"encoding/csv"
	"io"
	"log"
	"os"
)

type Mir struct {
	id       int
	name     string
	mandates int
}

type Mirs []int

func NewMirs() *Mirs {
	return &Mirs{}
}

func (m Mirs) Load(path string) {
	f, err := os.Open(path + MIR_FILENAME)
	if err != nil {
		log.Print(err)
	}
	defer f.Close()

	reader := bufio.NewReader(f)
	csv_reader := csv.NewReader(reader)
	csv_reader.Comma = ';'
	for {
		record, err := csv_reader.Read()
		if err == io.EOF {
			break
		}

		if err != nil {
			log.Print(err)
			os.Exit(FILE_READ_ERROR)
		}
		print(record)
	}
}
