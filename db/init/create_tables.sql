
create table passports(
    id serial primary key,
    type varchar(10) not null,
    number varchar(20) not null
);

create table companies(
    id serial primary key,
    name varchar(100) not null
);

create table departments(
    id serial primary key,
    name varchar(100) not null,
    phone varchar(15) not null,
    company_id integer not null,
    foreign key (company_id) references companies(id)
);

create table employees(
    id serial primary key,
    name varchar(100) not null,
    surname varchar(100) not null,
    phone varchar(15) not null,
    passport_id integer not null unique,
    foreign key (passport_id) references passport(id)
);

create table employee_departments(
    employee_id integer not null references employees(id) on delete cascade,
    department_id integer not null references departments(id),
    created_at timestamp default current_timestamp,
    primary key (employee_id, department_id)
);

