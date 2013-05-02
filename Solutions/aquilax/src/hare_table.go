package main

import (
	"fmt"
)

type HareCell struct {
	whole     int
	remainder int
}

type MirHareCells map[int]*HareCell

func NewMirHareCells() *MirHareCells {
	mir_hare_cells := make(MirHareCells)
	return &mir_hare_cells
}

type HareTable map[int]*MirHareCells

func NewHareTable() *HareTable {
	hare_table := make(HareTable)
	return &hare_table
}

func (ht *HareTable) Generate(v *Votes, m *Mirs) {
	for mir_id, mir_votes := range *v {
		total_mir_votes := mir_votes.TotalVotes()
		mandates := m.GetMandates(mir_id)
		hare_quota := float64(total_mir_votes) / float64(mandates)
		for party_id, votes := range *mir_votes {
			ht.AddCell(mir_id, party_id, votes, hare_quota)
		}
	}
}

func (ht *HareTable) AddCell(mir_id, party_id, votes int, hare_quota float64) {
	mir_hare_cells, ok := (*ht)[mir_id]
	if !ok {
		mir_hare_cells = NewMirHareCells()
		(*ht)[mir_id] = mir_hare_cells
	}

	hare := float64(votes) / hare_quota
	whole := int(hare)
	remainder := int((hare - float64(whole)) * global.remainder_multiplier)
	(*mir_hare_cells)[party_id] = &HareCell{whole, remainder}
}

func (ht *HareTable) Print() {
	fmt.Println("Hare table")
	for mir_id, hare_cells := range *ht {
		fmt.Printf("%d\t", mir_id)
		for party_id, hare_cell := range *hare_cells {
			fmt.Printf("%d=(%d,%d) ", party_id, hare_cell.whole, hare_cell.remainder)
		}
		fmt.Print("\n")
	}
}
