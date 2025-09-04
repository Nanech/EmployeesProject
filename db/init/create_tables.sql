
-- Создание таблиц
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
    phone varchar(20) not null
);

create table company_departments(
    company_id integer not null references companies(id) on delete cascade,
    department_id integer not null references departments(id) on delete cascade,
    created_at timestamp default current_timestamp,
    primary key (company_id, department_id)
);

create table employees(
    id serial primary key,
    name varchar(100) not null,
    surname varchar(100) not null,
    phone varchar(15) not null,
    passport_id integer not null unique,
    company_id integer not null references companies(id),
    foreign key (passport_id) references passports(id) on delete cascade
);

create table employee_departments(
    employee_id integer not null references employees(id) on delete cascade,
    department_id integer not null references departments(id) on delete cascade,
    created_at timestamp default current_timestamp,
    primary key (employee_id, department_id)
);

-- Добавление начальных данных
insert into companies (name) values 
    ('ООО "Технологии будущего"'),
    ('АО "Инновационные решения"'),
    ('ООО "Цифровые системы"');

insert into departments (name, phone) values 
    ('Отдел разработки', '+7 (999) 123-45-67'),
    ('Отдел тестирования', '+7 (999) 234-56-78'),
    ('Отдел аналитики', '+7 (999) 345-67-89'),
    ('Отдел поддержки', '+7 (999) 456-78-90'),
    ('HR отдел', '+7 (999) 567-89-01');

insert into passports (type, number) values 
    ('Паспорт', '1234567890');

insert into employees (name, surname, phone, passport_id, company_id) values 
    ('Алексей', 'Петров', '+79991234567', 1, 1);

-- Связываем департаменты с компаниями
insert into company_departments (company_id, department_id) values 
    (1, 1), -- Технологии будущего - Отдел разработки
    (1, 2), -- Технологии будущего - Отдел тестирования
    (1, 3), -- Технологии будущего - Отдел аналитики
    (2, 1), -- Инновационные решения - Отдел разработки
    (2, 4), -- Инновационные решения - Отдел поддержки
    (3, 5), -- Цифровые системы - HR отдел
    (3, 1); -- Цифровые системы - Отдел разработки

insert into employee_departments (employee_id, department_id) values
    (1, 1);
