package main

type Global struct {
	total_votes     int        // total votes
	total_mandates  int        // total mandates
	min_votes       float64    // minmum required votes to qualify
	hare_quota      float64    // Hare's quota
	party_votes     Aggregator // total votes per party
	mir_total_votes Aggregator // total votes per MIR used in II.9
}

func NewGlobal() Global {
	var global Global
	global.party_votes = NewAggregator()
	global.mir_total_votes = NewAggregator()
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
