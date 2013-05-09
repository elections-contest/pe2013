#!/usr/bin/php
<?php

function usage($error_code) {
	echo "Elections 2013 Mandate Calculator.".PHP_EOL;
	echo "This code is released under the MIT License.".PHP_EOL;
	echo "For more information check the official contest page at:".PHP_EOL;
	echo "\thttps://electionscontest.wordpress.com/".PHP_EOL.PHP_EOL;
	printf("Usage: php %s DIR [OUTPUT_FILENAME]" . PHP_EOL, basename(__FILE__));
	exit($error_code);
}

if ($argc < 2) {
	usage(1);
}
if (!file_exists($argv[1])) {
	printf('ERROR: Input files path "%s" does not exist.'.PHP_EOL, $argv[1]);
	usage(2);
}

$path = $argv[1];
$result_filename = isset($argv[2]) ? $argv[2] : FALSE;

if ($result_filename && !is_writable($argv[2])) {
	printf('ERROR: Result filename "" is not writable.'.PHP_EOL, $argv[2]);
	usage(3);
}

require 'pe.php';

$pe = new Pe2013\Pe();
$pe->start($path, $result_filename);