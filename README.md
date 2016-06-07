##International Project Cloud Infrastructure

###Phase 1: Clone repo and provision Vagrant

In an empty directory of your choosing, do the following:
```
git clone https://github.com/detuur/ip-spiro-cloud
cd ip-spiro-cloud
vagrant up
```
The machine will proceed to completely set up itself. There's one moment where manual intervention is necessary. Keep an eye on the terminal. A cow will tell you when it's time to click a few buttons to advance the process.

###Phase 2: Light manual setup

Once the cow tells you it's time, go to http://localhost:8091 on the dev machine and start the couchbase setup by clicking the button.

#####Step 1:
Change the "Data RAM Quota" field from 1200 to 1000 MB.
Leave the rest default.

#####Step2:
Press next.

#####Step 3:
Uncheck "Enable" under "Replicas"

#####Step 4:
Disable software notifications.
Agree to terms.

#####Step 5:
Username: Administrator
Password: spirometer

###Phase 3: Populating the database

You are now ready to populate the database. This is also automated. On the dev machine, inside the git repo, do the following:
```
./start-phase-3.sh
```
As soon as this is done, the box is completely set up.

###Phase 4: Using it

Go to http://localhost/Statistics.html in order to see the result.
There are two functions that work:
####Group stats
Under the tab Filter

* Select the filters you want to activate
 * e.g.: males between 12 and 24 years old
* Select single stat (whether you pick FEV or FVC doesn't matter, you're getting both)
* Click search
* You'll get the average results for that group on the bottom (scroll down more)

####FEV/FVC chart
Under the top Detail tab, enter any of these data IDs to chart the breathing cycle for that measurement.

* data-NHANES3-00047-19900604T084910
* data-NHANES3-00014-19901102T175600
* data-NHANES3-00019-19900606T143800
* data-NHANES3-00019-19900606T144100
* data-NHANES3-00039-19900505T180600

The full list of data can be found in the [database control panel][1]. Data entries start with the "data-" prefix.

###Full code contents

Data is spread over multiple repos.
[This][2] repo contains the VM configuration and data import utility.
[Spiro-server][3] contains the actual website code (test.js is the live code).
[Spirometer-website][4] contains the statical front-end files.


[1]: http://localhost:8091/ui/index.html#/documents?documentsBucket=default&pageLimit=100&pageNumber=0
[2]: https://github.com/detuur/ip-spiro-cloud
[3]: https://github.com/detuur/spiro-server
[4]: https://github.com/remberluyckx/Spirometer-website

