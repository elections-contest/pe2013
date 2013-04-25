package main

type Global struct {
	abroad_mir_id   int         // mir_id for votes abroad
	total_votes     int         // total votes
	total_mandates  int         // total mandates
	min_votes       float64     // minmum required votes to qualify
	hare_quota      float64     // Hare's quota
	candidate_votes Aggregator  // total votes per party
	mir_total_votes Aggregator  // total votes per MIR used in II.9
	icandidates     ICandidates // Independent Candidates
}

func NewGlobal() Global {
	var global Global
	global.candidate_votes = NewAggregator()
	global.mir_total_votes = NewAggregator()
	global.icandidates = NewICandidates()
	return global
}

type Aggregator map[int]int

func NewAggregator() Aggregator {
	return make(Aggregator)
}

func (a Aggregator) Add(key, value int) {
	old_value, _ := a[key]
	a[key] = old_value + value
}

// Generates map's key
func key(v1, v2 int) string {
	return string(v1) + "_" + string(v2)
}
