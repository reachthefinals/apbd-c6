create table Animal
(
    IdAnimal    int PRIMARY KEY IDENTITY (1,1),
    Name        nvarchar(200) NOT NULL,
    Description nvarchar(200),
    Category    nvarchar(200) NOT NULL,
    Area        nvarchar(200) NOT NULL
)
go