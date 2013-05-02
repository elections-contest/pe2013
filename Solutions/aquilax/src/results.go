package main

import (
	"fmt"
)

type MirResults map[int]int // mandates

func NewMirResults() *MirResults {
	mir_results := make(MirResults)
	return &mir_results
}

type Results map[int]*MirResults

func NewResults() *Results {
	results := make(Results)
	return &results
}

func (r *Results) Add(mir_id, candidate_id, mandates int) {
	mir_results, ok := (*r)[mir_id]
	if !ok {
		mir_results = NewMirResults()
		(*r)[mir_id] = mir_results
	}
	(*mir_results)[candidate_id] = mandates
}

func (r *Results) Print() {
	for mir_id, mir_results := range *r {
		for candidate_id, mandates := range *mir_results {
			fmt.Printf("%d;%d;%d\n", mir_id, candidate_id, mandates)
		}
	}
}
