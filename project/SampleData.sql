-- =============================================
-- SCRIPT TẠO DATA MẪU CHO DATABASE TRAVEL
-- =============================================

USE Travel;
GO

-- =============================================
-- 1. XÓA DỮ LIỆU CŨ (NẾU CÓ)
-- =============================================
PRINT 'Đang xóa dữ liệu cũ...'

DELETE FROM OrderDetail;
DELETE FROM [Order];
DELETE FROM History;
DELETE FROM OtpCode;
DELETE FROM Contact;
DELETE FROM Blog;
DELETE FROM Tour;
DELETE FROM Category;
DELETE FROM [User];

-- Reset Identity
DBCC CHECKIDENT ('OrderDetail', RESEED, 0);
DBCC CHECKIDENT ('[Order]', RESEED, 0);
DBCC CHECKIDENT ('History', RESEED, 0);
DBCC CHECKIDENT ('OtpCode', RESEED, 0);
DBCC CHECKIDENT ('Contact', RESEED, 0);
DBCC CHECKIDENT ('Blog', RESEED, 0);
DBCC CHECKIDENT ('Tour', RESEED, 0);
DBCC CHECKIDENT ('Category', RESEED, 0);
DBCC CHECKIDENT ('[User]', RESEED, 0);

PRINT 'Xóa dữ liệu cũ thành công!'
GO

-- =============================================
-- 2. THÊM DỮ LIỆU NGƯỜI DÙNG (USER)
-- =============================================
PRINT 'Đang thêm dữ liệu User...'

-- Password đã hash: "Admin123@"
INSERT INTO [User] (Name, Image, Gender, Address, Phone, Email, Username, Password, Role, DateOfBirth, OtpVerified, CreatedDate)
VALUES 
-- Admin
(N'Nguyễn Văn Admin', '/Uploads/admin.jpg', N'Nam', N'123 Đường Lê Lợi, Quận 1, TP.HCM', '0901234567', 'admin@travel.com', 
 'admin', 'AQAAAAIAAYagAAAAEKvW+RQxJXm8FqQXS5gZJONpF2BI3VGqKLHm0fHKl8Y6ZY0vLYBxTZ+8nQ0mN+qYuA==', 'ADMIN', '1990-01-15', 1, GETDATE()),

-- Users đã xác thực OTP
(N'Trần Thị Mai', '/Uploads/user1.jpg', N'Nữ', N'456 Nguyễn Huệ, Quận 1, TP.HCM', '0912345678', 'mai.tran@gmail.com', 
 'maitran', 'AQAAAAIAAYagAAAAEKvW+RQxJXm8FqQXS5gZJONpF2BI3VGqKLHm0fHKl8Y6ZY0vLYBxTZ+8nQ0mN+qYuA==', 'USER', '1995-05-20', 1, DATEADD(DAY, -10, GETDATE())),

(N'Lê Văn Hùng', '/Uploads/user2.jpg', N'Nam', N'789 Lê Thánh Tôn, Quận 3, TP.HCM', '0923456789', 'hung.le@gmail.com', 
 'hungle', 'AQAAAAIAAYagAAAAEKvW+RQxJXm8FqQXS5gZJONpF2BI3VGqKLHm0fHKl8Y6ZY0vLYBxTZ+8nQ0mN+qYuA==', 'USER', '1992-08-10', 1, DATEADD(DAY, -8, GETDATE())),

(N'Phạm Thị Lan', '/Uploads/user3.jpg', N'Nữ', N'321 Trần Hưng Đạo, Quận 5, TP.HCM', '0934567890', 'lan.pham@gmail.com', 
 'lanpham', 'AQAAAAIAAYagAAAAEKvW+RQxJXm8FqQXS5gZJONpF2BI3VGqKLHm0fHKl8Y6ZY0vLYBxTZ+8nQ0mN+qYuA==', 'USER', '1998-03-25', 1, DATEADD(DAY, -5, GETDATE())),

(N'Hoàng Minh Tuấn', '/Uploads/user4.jpg', N'Nam', N'654 Võ Văn Tần, Quận 3, TP.HCM', '0945678901', 'tuan.hoang@gmail.com', 
 'tuanhoang', 'AQAAAAIAAYagAAAAEKvW+RQxJXm8FqQXS5gZJONpF2BI3VGqKLHm0fHKl8Y6ZY0vLYBxTZ+8nQ0mN+qYuA==', 'USER', '1993-11-30', 1, DATEADD(DAY, -3, GETDATE())),

-- User chưa xác thực OTP (để test)
(N'Nguyễn Văn Test', NULL, N'Nam', N'999 Test Street', '0999999999', 'test@example.com', 
 'testuser', 'AQAAAAIAAYagAAAAEKvW+RQxJXm8FqQXS5gZJONpF2BI3VGqKLHm0fHKl8Y6ZY0vLYBxTZ+8nQ0mN+qYuA==', 'USER', '1995-01-01', 0, GETDATE());

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' User thành công!'
GO

-- =============================================
-- 3. THÊM DỮ LIỆU DANH MỤC (CATEGORY)
-- =============================================
PRINT 'Đang thêm dữ liệu Category...'

INSERT INTO Category (Name, Description, Image)
VALUES 
(N'Du lịch biển', N'Các tour du lịch biển đảo tuyệt đẹp', '/Uploads/Categories/beach.jpg'),
(N'Du lịch núi', N'Khám phá các vùng núi non hùng vĩ', '/Uploads/Categories/mountain.jpg'),
(N'Du lịch văn hóa', N'Tìm hiểu văn hóa lịch sử các địa phương', '/Uploads/Categories/culture.jpg'),
(N'Du lịch mạo hiểm', N'Các hoạt động mạo hiểm thú vị', '/Uploads/Categories/adventure.jpg'),
(N'Du lịch sinh thái', N'Khám phá thiên nhiên và bảo vệ môi trường', '/Uploads/Categories/eco.jpg');

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' Category thành công!'
GO

-- =============================================
-- 4. THÊM DỮ LIỆU TOUR
-- =============================================
PRINT 'Đang thêm dữ liệu Tour...'

INSERT INTO Tour (Name, Description, Image, Price, Discount, StartDate, EndDate, Location, CategoryId, CreatedDate)
VALUES 
-- Tour biển
(N'Phú Quốc - Đảo Ngọc Thiên Đường', 
 N'Khám phá vẻ đẹp của đảo Phú Quốc với những bãi biển tuyệt đẹp, nước biển trong xanh và các hoạt động vui chơi thú vị. Tour bao gồm: Khách sạn 4*, vé máy bay, bữa ăn, hướng dẫn viên.', 
 '/Uploads/Tours/phuquoc.jpg', 
 5500000, 10, DATEADD(DAY, 7, GETDATE()), DATEADD(DAY, 10, GETDATE()), N'Phú Quốc, Kiên Giang', 1, GETDATE()),

(N'Nha Trang - Thành Phố Biển', 
 N'Trải nghiệm du lịch biển Nha Trang với các hoạt động lặn ngắm san hô, tắm bùn khoáng, tham quan các đảo. Bao gồm: Khách sạn 3*, xe ô tô, bữa ăn, vé tham quan.', 
 '/Uploads/Tours/nhatrang.jpg', 
 4200000, 15, DATEADD(DAY, 5, GETDATE()), DATEADD(DAY, 8, GETDATE()), N'Nha Trang, Khánh Hòa', 1, GETDATE()),

(N'Đà Nẵng - Hội An', 
 N'Du lịch Đà Nẵng - Hội An với bãi biển Mỹ Khê, Bà Nà Hills, phố cổ Hội An. Tour 4 ngày 3 đêm, khách sạn 4*, xe đưa đón, ăn uống, hướng dẫn viên.', 
 '/Uploads/Tours/danang.jpg', 
 6800000, 20, DATEADD(DAY, 10, GETDATE()), DATEADD(DAY, 14, GETDATE()), N'Đà Nẵng - Quảng Nam', 1, GETDATE()),

-- Tour núi
(N'Sapa - Vùng Đất Sương Mù', 
 N'Chinh phục đỉnh Fansipan, khám phá ruộng bậc thang, văn hóa dân tộc thiểu số. Bao gồm: Xe limousine, khách sạn 3*, ăn uống, hướng dẫn viên địa phương.', 
 '/Uploads/Tours/sapa.jpg', 
 3800000, 5, DATEADD(DAY, 3, GETDATE()), DATEADD(DAY, 6, GETDATE()), N'Sapa, Lào Cai', 2, GETDATE()),

(N'Đà Lạt - Thành Phố Ngàn Hoa', 
 N'Tour Đà Lạt 3 ngày 2 đêm với thác Datanla, núi Langbiang, thung lũng Tình Yêu, chợ đêm. Khách sạn 3*, xe ô tô, ăn sáng, hướng dẫn viên.', 
 '/Uploads/Tours/dalat.jpg', 
 3200000, 0, DATEADD(DAY, 8, GETDATE()), DATEADD(DAY, 11, GETDATE()), N'Đà Lạt, Lâm Đồng', 2, GETDATE()),

-- Tour văn hóa
(N'Huế - Cố Đô Ngàn Năm', 
 N'Tham quan Di sản văn hóa thế giới: Đại Nội, lăng tẩm các vua, chùa Thiên Mụ, sông Hương. Khách sạn 3*, xe ô tô, ăn uống, vé tham quan.', 
 '/Uploads/Tours/hue.jpg', 
 4500000, 10, DATEADD(DAY, 12, GETDATE()), DATEADD(DAY, 15, GETDATE()), N'Huế, Thừa Thiên Huế', 3, GETDATE()),

(N'Hà Nội - Thủ Đô Ngàn Năm Văn Hiến', 
 N'Khám phá Hà Nội với Văn Miếu, Lăng Bác, Hồ Gươm, phố cổ. Tour 3 ngày 2 đêm, khách sạn 4*, xe ô tô, ăn uống đặc sản.', 
 '/Uploads/Tours/hanoi.jpg', 
 5200000, 15, DATEADD(DAY, 6, GETDATE()), DATEADD(DAY, 9, GETDATE()), N'Hà Nội', 3, GETDATE()),

-- Tour mạo hiểm
(N'Mù Cang Chải - Chinh Phục Tây Bắc', 
 N'Trekking khám phá ruộng bậc thang Mù Cang Chải, cắm trại, trải nghiệm văn hóa H''Mông. Bao gồm: Xe limousine, homestay, đồ cắm trại, hướng dẫn viên.', 
 '/Uploads/Tours/mucangchai.jpg', 
 4800000, 0, DATEADD(DAY, 15, GETDATE()), DATEADD(DAY, 19, GETDATE()), N'Mù Cang Chải, Yên Bái', 4, GETDATE()),

(N'Tà Năng - Phan Dũng: Trekking Đỉnh Cao', 
 N'Chinh phục cung đường trekking đẹp nhất Việt Nam qua rừng, đồi, thác. Tour 3 ngày 2 đêm, cắm trại, đồ leo núi, hướng dẫn viên chuyên nghiệp.', 
 '/Uploads/Tours/tanang.jpg', 
 5600000, 5, DATEADD(DAY, 20, GETDATE()), DATEADD(DAY, 23, GETDATE()), N'Đà Lạt - Ninh Thuận', 4, GETDATE()),

-- Tour sinh thái
(N'Cát Tiên - Vườn Quốc Gia', 
 N'Khám phá rừng nhiệt đới Cát Tiên, quan sát động vật hoang dã, trekking, chèo thuyền. Bao gồm: Xe ô tô, homestay, ăn uống, vé vào cửa, hướng dẫn viên.', 
 '/Uploads/Tours/cattien.jpg', 
 3500000, 0, DATEADD(DAY, 4, GETDATE()), DATEADD(DAY, 6, GETDATE()), N'Cát Tiên, Đồng Nai', 5, GETDATE()),

(N'Cần Thơ - Miền Tây Sông Nước', 
 N'Trải nghiệm chợ nổi Cái Răng, vườn trái cây, làng nghề truyền thống. Tour 3 ngày 2 đêm, khách sạn 3*, xe ô tô, bữa ăn đặc sản.', 
 '/Uploads/Tours/cantho.jpg', 
 3000000, 10, DATEADD(DAY, 9, GETDATE()), DATEADD(DAY, 12, GETDATE()), N'Cần Thơ', 5, GETDATE());

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' Tour thành công!'
GO

-- =============================================
-- 5. THÊM DỮ LIỆU BLOG
-- =============================================
PRINT 'Đang thêm dữ liệu Blog...'

INSERT INTO Blog (Title, Content, Image, Author, CreatedDate)
VALUES 
(N'Top 10 địa điểm du lịch không thể bỏ qua tại Việt Nam', 
 N'<h2>Danh sách những địa điểm du lịch tuyệt đẹp</h2>
 <p>Việt Nam là một đất nước với vẻ đẹp đa dạng từ biển đảo đến núi non, từ đô thị hiện đại đến làng quê yên bình...</p>
 <h3>1. Vịnh Hạ Long</h3>
 <p>Di sản thiên nhiên thế giới với hàng nghìn hòn đảo đá vôi kỳ vĩ...</p>
 <h3>2. Phố Cổ Hội An</h3>
 <p>Phố cổ được UNESCO công nhận với kiến trúc độc đáo...</p>', 
 '/Uploads/Blogs/top10.jpg', 
 N'Nguyễn Văn Admin', 
 DATEADD(DAY, -15, GETDATE())),

(N'Kinh nghiệm du lịch Phú Quốc tự túc', 
 N'<h2>Hướng dẫn chi tiết để có chuyến đi Phú Quốc tiết kiệm</h2>
 <p>Phú Quốc là điểm đến lý tưởng cho những ai yêu thích biển đảo. Dưới đây là những kinh nghiệm hữu ích...</p>
 <h3>1. Thời điểm đi</h3>
 <p>Tháng 11 đến tháng 3 là thời điểm lý tưởng nhất...</p>
 <h3>2. Phương tiện di chuyển</h3>
 <p>Có thể đi máy bay hoặc tàu cao tốc...</p>', 
 '/Uploads/Blogs/phuquoc-tips.jpg', 
 N'Trần Thị Mai', 
 DATEADD(DAY, -10, GETDATE())),

(N'Khám phá ẩm thực Đà Nẵng', 
 N'<h2>Những món ăn đặc sản không thể bỏ qua</h2>
 <p>Đà Nẵng không chỉ nổi tiếng với cảnh đẹp mà còn có ẩm thực phong phú...</p>
 <h3>1. Mì Quảng</h3>
 <p>Món ăn đặc trưng của vùng Quảng Nam...</p>
 <h3>2. Bánh Tráng Cuốn Thịt Heo</h3>
 <p>Món ăn vặt nổi tiếng...</p>', 
 '/Uploads/Blogs/danang-food.jpg', 
 N'Lê Văn Hùng', 
 DATEADD(DAY, -7, GETDATE())),

(N'Sapa - Thiên đường mùa đông', 
 N'<h2>Vẻ đẹp của Sapa trong mùa đông</h2>
 <p>Sapa trong mùa đông có một vẻ đẹp riêng với băng tuyết phủ trắng...</p>
 <h3>Thời điểm ngắm tuyết</h3>
 <p>Tháng 12 và tháng 1 là thời điểm có tuyết...</p>', 
 '/Uploads/Blogs/sapa-winter.jpg', 
 N'Phạm Thị Lan', 
 DATEADD(DAY, -5, GETDATE())),

(N'Bí quyết săn vé máy bay giá rẻ', 
 N'<h2>Những tips để mua được vé máy bay giá tốt</h2>
 <p>Vé máy bay là khoản chi phí lớn trong chuyến du lịch...</p>
 <h3>1. Đặt vé sớm</h3>
 <p>Đặt vé trước 2-3 tháng sẽ được giá tốt hơn...</p>
 <h3>2. Theo dõi khuyến mãi</h3>
 <p>Các hãng hàng không thường có chương trình khuyến mãi...</p>', 
 '/Uploads/Blogs/flight-tips.jpg', 
 N'Hoàng Minh Tuấn', 
 DATEADD(DAY, -3, GETDATE()));

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' Blog thành công!'
GO

-- =============================================
-- 6. THÊM DỮ LIỆU CONTACT
-- =============================================
PRINT 'Đang thêm dữ liệu Contact...'

INSERT INTO Contact (Name, Email, Phone, Subject, Message, CreatedDate, Status)
VALUES 
(N'Nguyễn Văn A', 'nguyenvana@gmail.com', '0901111111', 
 N'Hỏi về tour Phú Quốc', 
 N'Cho tôi hỏi tour Phú Quốc 4 ngày 3 đêm có giá bao nhiêu? Có khuyến mãi cho nhóm 10 người không?', 
 DATEADD(DAY, -5, GETDATE()), N'Pending'),

(N'Trần Thị B', 'tranthib@gmail.com', '0902222222', 
 N'Đặt tour Đà Lạt', 
 N'Tôi muốn đặt tour Đà Lạt cho ngày 20/11. Vui lòng liên hệ lại.', 
 DATEADD(DAY, -3, GETDATE()), N'Pending'),

(N'Lê Văn C', 'levanc@gmail.com', '0903333333', 
 N'Thắc mắc về chính sách hoàn hủy', 
 N'Nếu tôi đặt tour nhưng không thể đi được thì có được hoàn tiền không?', 
 DATEADD(DAY, -2, GETDATE()), N'Resolved'),

(N'Phạm Thị D', 'phamthid@gmail.com', '0904444444', 
 N'Góp ý về dịch vụ', 
 N'Tour Nha Trang rất tuyệt vời, hướng dẫn viên nhiệt tình. Cảm ơn công ty!', 
 DATEADD(DAY, -1, GETDATE()), N'Resolved');

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' Contact thành công!'
GO

-- =============================================
-- 7. THÊM DỮ LIỆU ORDER & ORDER DETAIL
-- =============================================
PRINT 'Đang thêm dữ liệu Order và OrderDetail...'

DECLARE @UserId1 INT = 2; -- Trần Thị Mai
DECLARE @UserId2 INT = 3; -- Lê Văn Hùng
DECLARE @UserId3 INT = 4; -- Phạm Thị Lan

-- Order 1: Đã thanh toán và hoàn thành
INSERT INTO [Order] (UserId, TotalAmount, Status, PaymentMethod, PaymentStatus, OrderDate, CompletedDate)
VALUES (@UserId1, 11000000, N'Completed', N'Banking', N'Paid', DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -10, GETDATE()));

DECLARE @OrderId1 INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, TourId, Quantity, Price, TotalPrice)
VALUES 
(@OrderId1, 1, 2, 5500000, 11000000); -- 2 người đi Phú Quốc

-- Order 2: Đã thanh toán, đang xử lý
INSERT INTO [Order] (UserId, TotalAmount, Status, PaymentMethod, PaymentStatus, OrderDate)
VALUES (@UserId2, 13600000, N'Processing', N'Banking', N'Paid', DATEADD(DAY, -5, GETDATE()), NULL);

DECLARE @OrderId2 INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, TourId, Quantity, Price, TotalPrice)
VALUES 
(@OrderId2, 3, 2, 6800000, 13600000); -- 2 người đi Đà Nẵng

-- Order 3: Chưa thanh toán
INSERT INTO [Order] (UserId, TotalAmount, Status, PaymentMethod, PaymentStatus, OrderDate)
VALUES (@UserId3, 7600000, N'Pending', N'COD', N'Unpaid', DATEADD(DAY, -2, GETDATE()), NULL);

DECLARE @OrderId3 INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, TourId, Quantity, Price, TotalPrice)
VALUES 
(@OrderId3, 4, 2, 3800000, 7600000); -- 2 người đi Sapa

-- Order 4: Nhiều tour
INSERT INTO [Order] (UserId, TotalAmount, Status, PaymentMethod, PaymentStatus, OrderDate)
VALUES (@UserId1, 9000000, N'Processing', N'Banking', N'Paid', DATEADD(DAY, -1, GETDATE()), NULL);

DECLARE @OrderId4 INT = SCOPE_IDENTITY();

INSERT INTO OrderDetail (OrderId, TourId, Quantity, Price, TotalPrice)
VALUES 
(@OrderId4, 5, 1, 3200000, 3200000), -- 1 người Đà Lạt
(@OrderId4, 11, 2, 3000000, 6000000); -- 2 người Cần Thơ

PRINT 'Thêm Order và OrderDetail thành công!'
GO

-- =============================================
-- 8. THÊM DỮ LIỆU HISTORY
-- =============================================
PRINT 'Đang thêm dữ liệu History...'

INSERT INTO History (UserId, TourId, OrderId, BookingDate, Status)
VALUES 
(2, 1, 1, DATEADD(DAY, -20, GETDATE()), N'Completed'),
(3, 3, 2, DATEADD(DAY, -5, GETDATE()), N'Processing'),
(4, 4, 3, DATEADD(DAY, -2, GETDATE()), N'Pending'),
(2, 5, 4, DATEADD(DAY, -1, GETDATE()), N'Processing'),
(2, 11, 4, DATEADD(DAY, -1, GETDATE()), N'Processing');

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' History thành công!'
GO

-- =============================================
-- 9. THÊM DỮ LIỆU OTP CODE (Để test)
-- =============================================
PRINT 'Đang thêm dữ liệu OTP Code...'

-- OTP còn hiệu lực (5 phút)
INSERT INTO OtpCode (Email, Code, ExpireAt, Used)
VALUES 
('test@example.com', '123456', DATEADD(MINUTE, 5, GETDATE()), 0),
('newuser@example.com', '789012', DATEADD(MINUTE, 3, GETDATE()), 0);

-- OTP đã hết hạn
INSERT INTO OtpCode (Email, Code, ExpireAt, Used)
VALUES 
('expired@example.com', '111111', DATEADD(MINUTE, -10, GETDATE()), 0);

-- OTP đã sử dụng
INSERT INTO OtpCode (Email, Code, ExpireAt, Used)
VALUES 
('used@example.com', '222222', DATEADD(MINUTE, 5, GETDATE()), 1);

PRINT 'Thêm ' + CAST(@@ROWCOUNT AS VARCHAR) + ' OTP Code thành công!'
GO

-- =============================================
-- 10. KIỂM TRA DỮ LIỆU
-- =============================================
PRINT ''
PRINT '============================================='
PRINT 'KIỂM TRA DỮ LIỆU ĐÃ THÊM'
PRINT '============================================='

SELECT 'Users' AS [Table], COUNT(*) AS [Count] FROM [User]
UNION ALL
SELECT 'Categories', COUNT(*) FROM Category
UNION ALL
SELECT 'Tours', COUNT(*) FROM Tour
UNION ALL
SELECT 'Blogs', COUNT(*) FROM Blog
UNION ALL
SELECT 'Contacts', COUNT(*) FROM Contact
UNION ALL
SELECT 'Orders', COUNT(*) FROM [Order]
UNION ALL
SELECT 'OrderDetails', COUNT(*) FROM OrderDetail
UNION ALL
SELECT 'Histories', COUNT(*) FROM History
UNION ALL
SELECT 'OtpCodes', COUNT(*) FROM OtpCode;

PRINT ''
PRINT '============================================='
PRINT 'HOÀN THÀNH! DỮ LIỆU MẪU ĐÃ ĐƯỢC THÊM'
PRINT '============================================='
PRINT ''
PRINT 'THÔNG TIN TÀI KHOẢN TEST:'
PRINT '- Admin: username = admin, password = Admin123@'
PRINT '- User 1: username = maitran, password = Admin123@'
PRINT '- User 2: username = hungle, password = Admin123@'
PRINT '- User 3: username = lanpham, password = Admin123@'
PRINT '- User 4: username = tuanhoang, password = Admin123@'
PRINT '- User (chưa xác thực OTP): username = testuser, password = Admin123@'
PRINT ''
PRINT 'LƯU Ý: Mật khẩu đã được hash bằng ASP.NET Core Identity PasswordHasher'
GO
