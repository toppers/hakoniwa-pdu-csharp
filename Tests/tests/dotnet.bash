#!/bin/bash

if [ $# -ne 1 ]
then
	echo "Usage: $0 {build|test}"
	exit 1
fi

CMD=${1}

CURR_DIR=`pwd`
cd ../ProjectFiles
dotnet build
cd ${CURR_DIR}
pwd
for entry in $(ls)
do
	if [ -f $entry ]
	then
		continue
	fi
	echo -e "\033[0;32m#### ${entry}: ${CMD}\033[0m"
	cd ${entry}
	dotnet ${CMD}
	cd ..
done
