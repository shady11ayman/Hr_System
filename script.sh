docker build -t hr .
docker stop running_hr_api
docker rm -f /running_hr_api
docker run -d -p 8012:8080 --name running_hr_api hr