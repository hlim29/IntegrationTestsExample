-- Drop the table if it already exists
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL
    DROP TABLE dbo.Users;

-- Create the Users table
CREATE TABLE dbo.Users
(
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NOT NULL
);

-- Insert 5 rows of dummy data
INSERT INTO dbo.Users (Name, Email, DateOfBirth)
VALUES 
    ('Sarah Johnson', 'sarah.johnson@email.com', '1985-03-15'),
    ('Michael Chen', 'michael.chen@email.com', '1990-07-22'),
    ('Emma Rodriguez', 'emma.rodriguez@email.com', '1988-11-08'),
    ('David Williams', 'david.williams@email.com', '1992-01-30'),
    ('Olivia Martinez', 'olivia.martinez@email.com', '1987-09-14');