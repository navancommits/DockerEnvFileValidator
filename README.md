# DockerEnvFileValidator

Copy over the utility to a local folder and run .\DockerEnvFileValidator <Path to the directory with .env file and compose/override files>

To check if there are missing vars in .env file compared to what is used in the docker files:

.\DockerEnvFileValidator <Path to the directory with .env file and compose/override files> "0"

To check if there are extra vars in .env file compared to what is used in the docker files:

.\DockerEnvFileValidator <Path to the directory with .env file and compose/override files> "1"

To automatically add missing vars onto .env file:

.\DockerEnvFileValidator <Path to the directory with .env file and compose/override files> "2"
