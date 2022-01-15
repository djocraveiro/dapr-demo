mongoimport --db=app_db --collection=products --file=/data/initial_products.json --jsonArray --mode=upsert --username=admin --password=admin --authenticationDatabase=admin

mongoimport --db=app_db --collection=products_dapr --file=/data/initial_products_dapr.json --jsonArray --mode=upsert --username=admin --password=admin --authenticationDatabase=admin
