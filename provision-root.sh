#!/bin/sh

# install couchbase
cd /tmp
curl -OkL http://packages.couchbase.com/releases/4.5.0-beta/couchbase-server-enterprise_4.5.0-beta-ubuntu14.04_amd64.deb
dpkg -i couchbase-server-enterprise_4.5.0-beta-ubuntu14.04_amd64.deb

# optimise Linux memory behaviour
sysctl vm.swappiness=0
cp /vagrant/disable-thp /etc/init.d/disable-thp
chmod 755 /etc/init.d/disable-thp
service disable-thp start
update-rc.d disable-thp defaults

# install git, 7z, mono & node
apt-get install -y git p7zip-rar cowsay
sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF
echo "deb http://download.mono-project.com/repo/debian wheezy main" | sudo tee /etc/apt/sources.list.d/mono-xamarin.list
apt-get update
apt-get install -y mono-devel
curl -sL https://deb.nodesource.com/setup_4.x | sudo -E bash -
apt-get install -y nodejs

# prep the installation directory
mkdir /var/www
chmod 777 /var/www

# set up node autorun
npm install -g forever
{ crontab -l; echo '@reboot /usr/bin/forever start /var/www/spiro-server/test.js'; } | crontab -