#!/bin/sh

# Make a temporary directory for populating the database
mkdir /tmp/populate
cd /tmp/populate
explorer .

# Partially download sources. Remove the pipe to "head -n xxx" if you want the full database.
# This is not recommended as that means you'll have to manually enter indexing commands
# in couchbase's console to have any meaningful performance (but it can be done).
curl ftp://ftp.cdc.gov/pub/Health_Statistics/NCHS/nhanes/nhanes3/1A/exam.dat | head -n 500 > exam.dat
curl ftp://ftp.cdc.gov/pub/Health_Statistics/NCHS/nhanes/nhanes3/1A/youth.dat | head -n 200 > youth.dat
curl ftp://ftp.cdc.gov/pub/Health_Statistics/NCHS/nhanes/nhanes3/1A/adult.dat | head -n 200 > adult.dat

# Download and unpack the raw spirometry data. It's packed in an exe so that's
# the reason behind this fuckery. Again, remove lines 18 and 19 to get the full file.
curl -OkL ftp://ftp.cdc.gov/pub/Health_Statistics/NCHS/nhanes/nhanes3/9A/nh3spiro.exe
7z e nh3spiro.exe
mv NH3SPIRO.CSV NH3SPIRO.trimme
cat NH3SPIRO.trimme | head -n 1330 > NH3SPIRO.CSV

# We move the PopulateCouch executable into the directory, pass it the files it needs,
# let it do its thing.
cp /vagrant/PopulateCouch/PopulateCouch/PopulateCouch/bin/Debug/* .
mono PopulateCouch.exe NH3SPIRO.CSV adult.dat youth.dat exam.dat

# Clean up our mess. This is strictly speaking not necessary since we're on a tmpfs. Can't hurt though.
cd /tmp
rm -rf populate

# Congratulate user on managing to get this broken mess to work.
cowsay "Website is now online at http://localhost/ .
Follow the instructions in README.md to
figure out how to use the damn thing."