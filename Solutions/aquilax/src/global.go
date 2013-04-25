package main

type Global struct {
	total_votes    int        // total votes
	total_mandates int        // total mandates
	min_votes      float64    // minmum required votes to qualify
	hare_quota     float64    // Hare's quota
	party_votes    PartyVotes // total votes per party
}

func NewGlobal() Global {
	var global Global
	global.party_votes = NewPartyVotes()
	return global
}
