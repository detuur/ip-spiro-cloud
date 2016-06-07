#!/bin/sh

# install the actual website from git
cd /var/www
git clone https://github.com/detuur/spiro-server --recursive
cd spiro-server
npm install

# start the node server
sudo forever start test.js

cowsay "Provisioning complete.
Follow part 2 of the instructions
to set up the server configuration."