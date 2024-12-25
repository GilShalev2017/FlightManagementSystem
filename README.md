"# FlightManagementSystem" 

docker pull rabbitmq:management //to install

docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management //to run

http://localhost:15672 use name: guest, password: guest //run the dashboard

docker logs rabbitmq //Verify the RabbitMQ Server

docker start rabbitmq //start

docker start rabbitmq //stop

docker restart rabbitmq //restart

docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -v rabbitmq_data:/var/lib/rabbitmq rabbitmq:management //run with persistence



