package main

var pe Pe

func main() {
	// TODO: Command line param
	path := "../../../Tests/1/"
	pe = NewPe(path)
	if pe.loadData() && pe.processData() {
		pe.saveData()
	}
}
