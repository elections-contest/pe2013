package main

import (
	"bufio"
	"encoding/csv"
	"io"
	"log"
	"os"
)

func loadFile (file_name string, callback func([]string)) {
	f, err := os.Open(file_name)
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
		callback(record)
	}
}
