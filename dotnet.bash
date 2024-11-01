#!/bin/bash

if [ $# -ne 1 ]
then
	echo "Usage: $0 {build|test}"
	exit 1
fi

CMD=${1}

cd ProjectFiles
dotnet build
cd ..

cd tests
for entry in $(ls)
do
	echo -e "\033[0;32m#### ${entry}: ${CMD}\033[0m"
	cd ${entry}
	dotnet ${CMD}
	cd ..
done
