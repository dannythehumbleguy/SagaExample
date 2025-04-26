rerun-all:
	docker-compose up --build -d orders-service sellers-service payment-service

rerun-one:
	docker-compose up --build -d $(s)