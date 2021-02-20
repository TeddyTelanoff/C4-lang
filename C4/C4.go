package main

import (
    "fmt"
    "os"
)

func main() {
    argsWithProg := os.Args
    argsWithoutProg := argsWithProg[1:]
	if (len(argsWithoutProg) == 1) {
		if (argsWithoutProg[0] == "test") {
			fmt.Println("C4 was successfully installed! Run C4 help to learn how to compile or run C4 code.");
		}
	}
	if (len(argsWithoutProg) == 0 || argsWithoutProg[0] == "help") {
		fmt.Println("Build and run file quickly: C4 run <filename>");
		fmt.Println("Just build a file to be ran later: C4 build <filename>");
	}
	if (len(argsWithoutProg) == 2) {
		if (argsWithoutProg[0] == "build") {
			fmt.Println("Sorry! WIP... #TODO")
		}
		if (argsWithoutProg[0] == "run") {
			fmt.Println("Sorry! WIP... #TODO")
		}
	}
}
