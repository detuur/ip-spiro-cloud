# Defines our Vagrant environment
#
# -*- mode: ruby -*-
# vi: set ft=ruby :

Vagrant.configure("2") do |config|

	config.vm.box = "ubuntu/trusty64"
	config.vm.hostname = "spiro"
	
	# download more RAMs
	config.vm.provider "virtualbox" do |vb|
		vb.memory = "2048"
	end
	
	# provisioning
	config.vm.provision :shell, path: "provision-root.sh"
	config.vm.provision :shell, path: "provision-user.sh", privileged: false

	# general ports
	config.vm.network "forwarded_port", guest: 22, host:2223
	config.vm.network "forwarded_port", guest: 80, host:80
	
	# couchbase ports
	config.vm.network "forwarded_port", guest: 8091, host:8091
	config.vm.network "forwarded_port", guest: 4984, host:4984
	config.vm.network "forwarded_port", guest: 4985, host:4985
	config.vm.network "forwarded_port", guest: 8092, host:8092
	config.vm.network "forwarded_port", guest: 11210, host:11210
	config.vm.network "forwarded_port", guest: 11211, host:11211
end
