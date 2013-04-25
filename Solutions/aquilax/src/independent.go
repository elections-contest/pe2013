package main

type ICandidate struct {
	mir_id       int
	candidate_id int
	votes        int
}

type ICandidates map[string]ICandidate

func NewICandidates() ICandidates {
	return make(ICandidates)
}

func (ic ICandidates) Add(mir_id, candidate_id int) {
	ic[key(mir_id, candidate_id)] = ICandidate{mir_id, candidate_id, 0}
}

func (ic ICandidates) SetVote(mir_id, candidate_id, votes int) {
	candidate, ok := ic[key(mir_id, candidate_id)]
	if ok {
		candidate.votes = votes
	}

}
