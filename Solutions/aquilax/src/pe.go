package main

type Pe struct {
	path       string
	mirs       Mirs
	parties    Parties
	candidates Candidates
	votes      Votes
}

func NewPe(path string) Pe {
	var pe Pe
	pe.path = path
	pe.mirs = NewMirs()
	pe.parties = NewParties()
	pe.candidates = NewCandidates()
	pe.votes = NewVotes()
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
	// Process Data
	return true
}

func (pe Pe) saveData() {
	// Save Data
}
