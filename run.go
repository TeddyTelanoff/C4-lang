package main

import (
    "fmt"
	"log"
    "os"
	"os/exec"
    "io/ioutil"
    "strings"
)

func main() {
	path := strings.Split(os.Getenv("PATH"), ";")
	go_bin := ""
    for i := range path {
		dir := path[i]
		if (strings.HasSuffix(dir, "Go/bin") || strings.HasSuffix(dir, "Go\\bin") ||
		    strings.HasSuffix(dir, "Go//bin") || strings.HasSuffix(dir, "Go\\\\bin")) {
			if (exists(dir)) {
				go_bin = dir
			}
		}
    }
	
	os.Chdir("./C4")
    items, _ := ioutil.ReadDir(".")
    for _, item := range items {
        if !item.IsDir() {
			fn := item.Name()
			exec.Command("go", "build", fn).Run()			
        }
    }
	
    items, _ = ioutil.ReadDir(".")
    for _, item := range items {
        if !item.IsDir() {
			src := item.Name()
			dst := go_bin + "\\" + src
			copy(src, dst)
        }
    }
	
	fmt.Println("C4 should now be installed! Run C4 test to check. ")
}

func exists(path string) bool {
	src, err := os.Stat(path)
    if src == nil || os.IsNotExist(err) { return false }
    return true
}

func checkErr(err error) {
    if err != nil {
        log.Fatal(err)
    }
}

func copy(src string, dst string) {
    data, err := ioutil.ReadFile(src)
    checkErr(err)
    err = ioutil.WriteFile(dst, data, 0644)
    checkErr(err)
}