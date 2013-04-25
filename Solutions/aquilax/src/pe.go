package main

type Pe struct {
	path       string
	mirs       Mirs
	parties    Parties
	candidates Candidates
	votes      Votes
	global     Global
}

func NewPe(path string) Pe {
	var pe Pe
	pe.path = path
	pe.mirs = NewMirs()
	pe.parties = NewParties()
	pe.candidates = NewCandidates()
	pe.votes = NewVotes()
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

	processIndipendentCandidates()

	// minimum votes to qualify
	pe.global.min_votes = float64(pe.global.total_votes) * VOTE_BAREER

	// remove parties below min_votes
	removePartiesBelowMinVotesLimit(int(pe.global.min_votes))

	// calculate quota
	pe.global.hare_quota = float64(pe.global.total_votes) / float64(pe.global.total_mandates)

	// Process Data
	return true
}

func processIndipendentCandidates() {
}

func removePartiesBelowMinVotesLimit(min_votes int) {
	for candidate_id, votes := range pe.global.candidate_votes {
		candidate_type := pe.parties.getCandidateType(candidate_id)
		if candidate_type == CANDIDATE_PARTY && votes < min_votes {
			pe.parties.Remove(candidate_id)
		}
	}
}

func saveData() {
	// Save Data
}
