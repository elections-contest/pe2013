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

func (pe Pe) loadData() bool {
	// Load Data
	pe.mirs.Load(pe.path)
	pe.parties.Load(pe.path)
	pe.candidates.Load(pe.path)
	pe.votes.Load(pe.path)
	return true
}

func (pe Pe) processData() bool {
	// minimum votes to qualify
	pe.global.min_votes = float64(pe.global.total_votes) * VOTE_BAREER
	// TODO:remove parties below min_votes
	// TODO:get new total_votes

	// calculate quota
	pe.global.hare_quota = float64(pe.global.total_votes) / float64(pe.global.total_mandates)
	// Process Data
	return true
}

func (pe Pe) saveData() {
	// Save Data
}
