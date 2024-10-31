#!/bin/bash

if [ $# -ne 1 ]
then
	echo "Usage: $0 {build|test}"
	exit 1
fi

CMD=${1}

cd hakoniwa
dotnet build
cd ..

cd tests
for entry in pdu udp websocket
do
	cd hakoniwa.${entry}.test
	dotnet ${CMD}
	cd ..
done
