#!/bin/bash

GREEN='\033[0;32m'
NC='\033[0m' # No Color

printf "${GREEN}Run client${NC}\n"
./client_build/MacOSBuild.app/Contents/MacOS/CubesSample 
