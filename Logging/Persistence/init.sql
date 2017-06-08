create table logs(
	correlationid uuid not null,
	service text not null,
	message text not null,
	level text not null,
	occurred timestamp default current_timestamp)