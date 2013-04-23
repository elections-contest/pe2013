package main

type Pe struct {
	path string
	mirs *Mirs
}

func NewPe(path string) *Pe {
	var pe Pe
	pe.path = path
	pe.mirs = NewMirs()
	return &pe
}

func (pe Pe) loadData() bool {
	// Load Data
	pe.mirs.Load(pe.path)
	return true
}

func (pe Pe) processData() bool {
	// Process Data
	return true
}

func (pe Pe) saveData() {
	// Save Data
}
