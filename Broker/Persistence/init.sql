create table forsale(
    sellerid uuid,
    stock text,
    price decimal,
	amount integer
);

create table buyoffers(
	buyerid uuid,
	stock text,
	price decimal,
	amount integer
);

create table stocks(
	ownerid uuid,
	stock text,
	amount integer
);