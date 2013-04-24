package main

import (
	"testing"
	"strconv"
)

func TestNewMirs(t *testing.T) {
	mirs := NewMirs()
	if mirs == nil {
		t.Error("Mirs is nil")
	}
}

func TestAdd(t *testing.T) {
	mirs := NewMirs()
	record := make([]string, 3, 3)
	record[0] = "1"
	record[1] = "МИР1"
	record[2] = "100"
	mirs.Add(record)
	rec, ok := mirs[1];
	if !ok {
		t.Error("New element is nil")
	}
	expected, _ := strconv.Atoi(record[0])
	actual := rec.id
	if actual != expected {
		t.Errorf("Wrong id actual:%d expected:%d", actual, expected)
	}
}
