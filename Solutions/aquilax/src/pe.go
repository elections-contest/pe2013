package main

import (
	"fmt"
	"math"
	"sort"
)

type Pe struct {
	path       string
	mirs       Mirs
	parties    Parties
	candidates Candidates
	votes      Votes
	results    Results
	global     Global
}

func NewPe(path string) Pe {
	var pe Pe
	pe.path = path
	pe.mirs = NewMirs()
	pe.parties = NewParties()
	pe.candidates = NewCandidates()
	pe.votes = NewVotes()
	pe.results = NewResults()
	pe.global = NewGlobal()
	return pe
}

func loadData() bool {
	// Load Data
	pe.mirs.Load(pe.path)
	pe.parties.Load(pe.path)
	pe.candidates.Load(pe.path)
	pe.votes.Load(pe.path)
	return true
}

func processData() bool {

	processIndependentCandidates()

	// minimum votes to qualify
	pe.global.min_votes = float64(pe.global.total_votes) * VOTE_BAREER

	// remove parties below min_votes
	removePartiesBelowMinVotesLimit(int(pe.global.min_votes))

	// calculate quota
	hare_quota := float64(pe.global.total_votes) / float64(pe.global.total_mandates)

	processPartyProportionalMandates(hare_quota)

	// Process Data
	return true
}

func processIndependentCandidates() {
	if len(pe.global.icandidates) == 0 {
		return
	}
	for _, icandidate := range pe.global.icandidates {
		mir_id := icandidate.mir_id
		mirIQuota := int(pe.global.mir_total_votes[mir_id] / pe.mirs[mir_id].mandates)

		if icandidate.votes >= mirIQuota {
			// Winner (single mandate for Indie)
			pe.results.Add(mir_id, icandidate.candidate_id, INDIVIDUAL_MANDATES)
			pe.global.total_mandates -= INDIVIDUAL_MANDATES
			mir, ok := pe.mirs[mir_id]
			if ok {
				mir.mandates -= INDIVIDUAL_MANDATES
			}
		}
	}
}

func removePartiesBelowMinVotesLimit(min_votes int) {
	for candidate_id, votes := range pe.global.candidate_votes {
		candidate_type := pe.parties.getCandidateType(candidate_id)
		if candidate_type == CANDIDATE_PARTY && votes < min_votes {
			pe.parties.Remove(candidate_id)
		}
	}
}

func processPartyProportionalMandates(quota float64) {
	mandates := make(map[int]int)
	remainders := NewRemainders()
	pre_total_mandates := 0
	for party_id, _ := range pe.parties {
		votes, ok := pe.global.candidate_votes[party_id]
		if ok {
			party_mandates := int(float64(votes) / quota)
			mandates[party_id] = party_mandates
			pre_total_mandates += party_mandates
			remainder := int(((float64(votes) / quota) - float64(party_mandates)) * math.Pow(10, float64(REMAINDER_PRECISION)))
			remainders.Add(party_id, remainder)
			fmt.Println(remainder)
		}
	}
	remaining_mandates := pe.global.total_mandates - pre_total_mandates
	if remaining_mandates > 0 {
		// Distribute remaining mandates
		sort.Sort(remainders)
		for i := 0; i < remaining_mandates; i++ {
			mandates[(*remainders)[i].party_id]++
		}
	}
	fmt.Println(mandates)
}

func saveData() {
	// Save Data
}
