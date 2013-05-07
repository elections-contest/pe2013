#!/usr/bin/php
<?php

function usage($error_code) {
	print("Usage: php pe.php DIR [OUTPUT_FILENAME]" . PHP_EOL);
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

$pe = new Pe();
$pe->start($path, $result_filename);