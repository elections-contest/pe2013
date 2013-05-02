package main

import (
	"fmt"
	"sort"
)

type Pe struct {
	path       string
	mirs       *Mirs
	parties    *Parties
	candidates *Candidates
	votes      *Votes
	results    *Results
}

func NewPe(path string) *Pe {
	var pe Pe
	pe.path = path
	pe.mirs = NewMirs()
	pe.parties = NewParties()
	pe.votes = NewVotes()
	pe.candidates = NewCandidates(pe.parties, pe.votes)
	pe.results = NewResults()
	return &pe
}

func (pe *Pe) loadData() bool {
	// Load Data
	pe.mirs.Load(pe.path)
	pe.parties.Load(pe.path)
	pe.candidates.Load(pe.path)
	pe.votes.Load(pe.path)
	return true
}

func (pe *Pe) processData() bool {

	pe.processIndependentCandidates()

	// minimum votes to qualify
	global.min_votes = float64(global.total_votes) * VOTE_BAREER

	// remove parties below min_votes
	pe.removePartiesBelowMinVotesLimit(int(global.min_votes))

	// calculate quota
	hare_quota := float64(global.total_votes) / float64(global.total_mandates)

	pe.processPartyProportionalMandates(hare_quota)

	// remove votes abroad
	pe.votes.RemoveAbroad(global.abroad_mir_id)

	pe.votes.Print()

	hare_table := NewHareTable()
	hare_table.Generate(pe.votes, pe.mirs)
	hare_table.Print()
	// Process Data
	return true
}

func (pe Pe) processIndependentCandidates() {
	if len(global.icandidates) == 0 {
		return
	}
	for _, icandidate := range global.icandidates {
		mir_id := icandidate.mir_id
		mirIQuota := int(global.mir_total_votes[mir_id] / (*pe.mirs)[mir_id].mandates)

		if icandidate.votes >= mirIQuota {
			// Winner (single mandate for Indie)
			pe.results.Add(mir_id, icandidate.candidate_id, INDIVIDUAL_MANDATES)
			global.total_mandates -= INDIVIDUAL_MANDATES
			mir, ok := (*pe.mirs)[mir_id]
			if ok {
				mir.mandates -= INDIVIDUAL_MANDATES
			}
		}
	}
}

func (pe *Pe) removePartiesBelowMinVotesLimit(min_votes int) {
	for candidate_id, votes := range global.candidate_votes {
		candidate_type := pe.parties.getCandidateType(candidate_id)
		if candidate_type == CANDIDATE_PARTY && votes < min_votes {
			pe.parties.Remove(candidate_id)
			pe.candidates.RemoveParty(candidate_id)
		}
	}
}

func (pe *Pe) processPartyProportionalMandates(quota float64) {
	mandates := make(map[int]int)
	remainders := NewRemainders()
	pre_total_mandates := 0
	for party_id, _ := range *(*pe).parties {
		votes, ok := global.candidate_votes[party_id]
		if ok {
			party_mandates := int(float64(votes) / quota)
			mandates[party_id] = party_mandates
			pre_total_mandates += party_mandates
			remainder := int(((float64(votes) / quota) - float64(party_mandates)) * global.remainder_multiplier)
			remainders.Add(party_id, remainder)
			fmt.Println(remainder)
		}
	}
	remaining_mandates := global.total_mandates - pre_total_mandates
	if remaining_mandates > 0 {
		// Distribute remaining mandates
		sort.Sort(remainders)
		for i := 0; i < remaining_mandates; i++ {
			mandates[(*remainders)[i].party_id]++
		}
	}
	fmt.Println(mandates)
}

func (pe Pe) saveData() {
	// Save Data
}
