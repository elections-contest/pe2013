SHELL := /bin/bash

doc: clean
	phpdoc run --title pe2013 -f ./src/pe.php -t doc

clean:
	rm -rf doc/*

test:
	diff -s <(src/izbori.php ../../Tests/1/) ../../Tests/1/Result.txt
