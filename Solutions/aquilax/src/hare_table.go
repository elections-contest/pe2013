package main

type HareCell struct {
	whole int
	remainder int
}

type MirHareCells map[int]HareCell

func NewMirHareCells() MirHareCells {
	return make(MirHareCells)
}

type HareTable map[int]MirHareCells

func NewHareTable() HareTable {
	return make(HareTable)
}

func (ht HareTable) Generate(v Votes) {
	for mir_id, mir_votes := range v {
		print(1)
		total_mir_votes := mir_votes.TotalVotes()
		for party_id, votes := range mir_votes {
			ht.AddCell(mir_id, party_id, votes, total_mir_votes)
		}
	}
}

func (ht HareTable) AddCell(mir_id, party_id, votes, total_mir_votes int) {
	mir_hare_cells, ok := ht[mir_id]
	if !ok {
		mir_hare_cells = NewMirHareCells()
		ht[mir_id] = mir_hare_cells
	}
	hare := total_mir_votes / votes
	whole := int(hare)
	remainder := int((hare - whole) * REMAINDER_PRECISION)
	mir_hare_cells[party_id] = HareCell{whole, remainder}
}
