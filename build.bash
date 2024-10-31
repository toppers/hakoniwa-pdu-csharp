#!/bin/bash

cd hakoniwa
dotnet build
cd ..

cd tests
for entry in pdu udp websocket
do
	cd hakoniwa.${entry}.test
	dotnet build
	cd ..
done
