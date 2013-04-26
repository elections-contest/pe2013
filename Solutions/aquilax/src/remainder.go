package main

type Remainder struct {
	party_id  int
	remainder int
}

type Remainders []Remainder

func NewRemainders() *Remainders {
	return &Remainders{}
}

func (r *Remainders) Len() int {
	return len(*r)
}

func (r *Remainders) Less(i, j int) bool {
	return (*r)[i].remainder > (*r)[j].remainder
}

func (r *Remainders) Swap(i, j int) {
	(*r)[i], (*r)[j] = (*r)[j], (*r)[i]
}

func (r *Remainders) Add(party_id, remainder int) {
	*r = append(*r, Remainder{party_id, remainder})
}
