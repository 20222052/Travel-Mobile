CREATE DATABASE Travel;

USE Travel;
-- Bảng User
CREATE TABLE [User] (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100),
    Image NVARCHAR(255), 
    Gender NVARCHAR(10),
    Address NVARCHAR(255),
    Phone NVARCHAR(20) UNIQUE,
    Email NVARCHAR(100) UNIQUE,
    DateOfBirth DATE,
    Username NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    Role NVARCHAR(50),
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Bảng Category
CREATE TABLE Category (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Type NVARCHAR(100)
);

--Bảng Tour
CREATE TABLE Tour (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255), 
    CategoryId INT NOT NULL,        
    Image NVARCHAR(255),         
    Location NVARCHAR(100),       
    Duration NVARCHAR(50),        
    Price FLOAT,         
    People INT,    
    View INT,                
    Description NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (CategoryId) REFERENCES Category(Id)
   
);

-- Bảng History
CREATE TABLE History (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    TourId INT NOT NULL,
    Content NVARCHAR(MAX),
    CreatedDate DATETIME DEFAULT GETDATE(),
    Status TINYINT DEFAULT 0,
    FOREIGN KEY (UserId) REFERENCES [User](Id),
    FOREIGN KEY (TourId) REFERENCES Tour(Id)
);

-- Bảng Contact
CREATE TABLE Contact(
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(255),
    Title NVARCHAR(255),
    Content NVARCHAR(MAX),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    PostedDate DATE DEFAULT GETDATE()
);
-- Bảng Blog
CREATE TABLE Blog (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(255),
    Image NVARCHAR(255), 
    Content NVARCHAR(MAX),
    PostedDate DATE DEFAULT GETDATE(),
    CategoryId INT NOT NULL,
    View INT,
    FOREIGN KEY (CategoryId) REFERENCES Category(Id)
);

-- Bảng Odder
CREATE TABLE [Order](
    Id INT PRIMARY KEY IDENTITY(1,1),
    TourId INT,
    UserId INT NOT NULL,
    Name NVARCHAR(100),
    Gender NVARCHAR(10),
    Address NVARCHAR(255),
    Phone NVARCHAR(20),
    Email NVARCHAR(100),
    Status TINYINT DEFAULT 0,
    Date DATETIME DEFAULT GETDATE(),	
    FOREIGN KEY (TourId) REFERENCES Tour(Id),
    FOREIGN KEY (UserId) REFERENCES [User](Id)
);

INSERT INTO Category (Type) VALUES
(N'Thám hiểm'),
(N'Nghỉ dưỡng'),
(N'Văn hóa');

INSERT INTO [User] (Name, Image, Gender, Address, Phone, Email, DateOfBirth, Username, Password, Role)
VALUES
(N'Nguyễn Văn A', N'fansi.jpg', N'Nam', N'123 Lê Lợi, TP.HCM', '0909123456', 'a.nguyen@example.com', '1990-05-10', 'nguyenvana', '123456', 'ADMIN'),
(N'Trần Thị B', N'fansi.jpg' , N'Nữ', N'456 Trần Hưng Đạo, Hà Nội', '0912345678', 'b.tran@example.com', '1995-08-22', 'tranthib', 'abcdef', 'USER');

INSERT INTO Tour (Name, CategoryId, Image, Location, Duration, Price, People, Description)
VALUES
(N'Chinh phục Fansipan', 1, N'fansi.jpg', N'Lào Cai', N'3 ngày 2 đêm', 3500000, 20, N'Tour leo núi khám phá đỉnh Fansipan.'),
(N'Nghỉ dưỡng Phú Quốc', 2, N'phuquoc.jpg', N'Phú Quốc', N'4 ngày 3 đêm', 5500000, 15, N'Tour nghỉ dưỡng biển đảo Phú Quốc.'),
(N'Khám phá Hội An', 3, N'hoian.jpg', N'Quảng Nam', N'2 ngày 1 đêm', 2500000, 25, N'Tour du lịch văn hóa tại phố cổ Hội An.');

INSERT INTO Blog (Title, Image, Content, PostedDate, CategoryId)
VALUES
(N'Mẹo chuẩn bị cho chuyến đi leo núi', N'fansi.jpg', N'Chuẩn bị thể lực và dụng cụ là quan trọng...', '2025-05-01', 1),
(N'Top 5 khu nghỉ dưỡng lý tưởng', N'fansi.jpg', N'Cùng điểm qua các địa điểm sang trọng...', '2025-05-05', 2),
(N'Hội An – Di sản văn hóa thế giới', N'fansi.jpg', N'Lịch sử và kiến trúc độc đáo...', '2025-05-10', 3);

INSERT INTO History (UserId, TourId, Content)
VALUES
(1, 1, N'Đã tham gia tour Fansipan, rất hài lòng.'),
(2, 2, N'Phú Quốc thật đẹp, dịch vụ tốt.');

INSERT INTO [Order] (TourId, UserId, Name, Gender, Address, Phone, Email)
VALUES
(1, 1, N'Nguyễn Văn A', N'Nam', N'123 Lê Lợi, TP.HCM', '0909123456', 'a.nguyen@example.com'),
(3, 2, N'Trần Thị B', N'Nữ', N'456 Trần Hưng Đạo, Hà Nội', '0912345678', 'b.tran@example.com');
