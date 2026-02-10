#! /usr/bin/env bash
set -uvx
set -e
cd "$(dirname "$0")"
cwd=`pwd`
ts=`date "+%Y.%m%d.%H%M.%S"`
java -jar antlr-4.13.2-complete.jar JSON5.g4 -Dlanguage=CSharp -package JavaCommons.Parser.Json5
