#!/bin/bash

getArch()
{
#  STR=getconf LONG_BIT
#  STR=64
#  STR=uname -r | cut -f2 -d_
#  if [ $STR -eq 64 ]; then
  if [ $(getconf LONG_BIT) -eq 64 ]; then
    ARCH=x86_64
  else
# default is 32bit
    ARCH=i386
  fi
  echo "----------------------------------------"
  echo "in function getArch"
# echo STR = $STR
  echo ARCH = $ARCH
# echo "----------------------------------------"
}

buildCopyLibrary()
{
rmmod -f ftdi_sio
rmmod -f usbserial
cd ../../LibMPSSE/Build/Linux
make clean
make
#if [ $ARCH == "x86_64" ]; then
if [ $(getconf LONG_BIT) -eq 64 ]; then
  cp -f libMPSSE.* /usr/lib64
fi
cp -f libMPSSE.* /usr/lib
cp -f libMPSSE.* ../../../Release/lib/linux/i386
}

I2C-Static()
{
buildCopyLibrary
cd ../../../Release/samples/I2C
rm -rf libMPSSE.so
rm -rf libMPSSE.a
rm -rf sample-dynamic.o
rm -rf sample-static.o
cp -f ../../include/libMPSSE_i2c.h .
cp -f ../../include/linux/*.h .
cp -f ../../lib/linux/i386/* .
echo ------------------------------------------------
echo Building sample by linking to static library and running it
gcc -g -ldl -I. -o sample-static.o sample-static.c libMPSSE.a
./sample-static.o
}

I2C-Dynamic()
{
buildCopyLibrary
cd ../../../Release/samples/I2C
rm -rf libMPSSE.so
rm -rf libMPSSE.a
rm -rf sample-dynamic.o
rm -rf sample-static.o
cp -f ../../include/libMPSSE_i2c.h .
cp -f ../../include/linux/*.h .
cp -f ../../lib/linux/i386/* .
echo ------------------------------------------------
echo Building sample by linking to dynamic library and running it
gcc -ldl sample-dynamic.c -o sample-dynamic.o
./sample-dynamic.o
}

SPI-Static()
{
buildCopyLibrary
cd ../../../Release/samples/SPI
rm -rf libMPSSE.so
rm -rf libMPSSE.a
rm -rf sample-dynamic.o
rm -rf sample-static.o
cp -f ../../include/libMPSSE_spi.h .
cp -f ../../include/linux/*.h .
cp -f ../../lib/linux/i386/* .
echo ------------------------------------------------
echo Building sample by linking to static library and running it
gcc -g -ldl -I. -o sample-static.o sample-static.c libMPSSE.a
./sample-static.o
}

SPI-Dynamic()
{
buildCopyLibrary
cd ../../../Release/samples/SPI
rm -rf libMPSSE.so
rm -rf libMPSSE.a
rm -rf sample-dynamic.o
rm -rf sample-static.o
cp -f ../../include/libMPSSE_spi.h .
cp -f ../../include/linux/*.h .
cp -f ../../lib/linux/i386/* .
echo ------------------------------------------------
echo Building sample by linking to dynamic library and running it
gcc -ldl sample-dynamic.c -o sample-dynamic.o
./sample-dynamic.o
}

testAll()
{
	I2C-Static
	echo Press enter to continue
	read
	I2C-Dynamic
	echo Press enter to continue
	read
	SPI-Static
	echo Press enter to continue
	read
	SPI-Dynamic
}

#Execution starts from here
getArch ;
echo "----------------------------------------"
echo Please select the sample application to build and run
echo 1. I2C-Static
echo 2. I2C-Dynamic
echo 3. SPI-Static
echo 4. SPI-Dynamic
echo 5. Test all
read input
#Select input
    case $input in
        1 ) I2C-Static ;;
        2 ) I2C-Dynamic ;;
	3 ) SPI-Static ;;
	4 ) SPI-Dynamic ;;
	5 ) testAll ;;
	* ) echo "invalid input" ;;
    esac
#Done

