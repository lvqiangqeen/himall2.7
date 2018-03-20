DROP TABLE IF EXISTS `himall_indexcategory`;
CREATE TABLE `himall_indexcategory` (
  `Id` bigint(20) NOT NULL,
  `CategoryId` bigint(20) NOT NULL,
  `Level` int(2) NOT NULL,
  PRIMARY KEY (`Id`,`CategoryId`),
  KEY `Index` (`CategoryId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `himall_indexname`;
CREATE TABLE `himall_indexname` (
  `Id` bigint(20) NOT NULL,
  `Word` char(50) NOT NULL,
  `Count` int(2) DEFAULT '1',
  PRIMARY KEY (`Id`,`Word`),
  KEY `Index` (`Word`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `himall_indexattr`;
CREATE TABLE `himall_indexattr` (
  `Id` bigint(20) NOT NULL,
  `AttrValueId` varchar(30) NOT NULL,
  `AttrId` bigint(20) NOT NULL,
  `ValueId` bigint(20) NOT NULL,
  PRIMARY KEY (`Id`,`AttrValueId`),
  KEY `Index` (`AttrValueId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `himall_indexproduct`;
CREATE TABLE `himall_indexproduct` (
  `Id` bigint(20) NOT NULL,
  `ShopId` bigint(20) DEFAULT NULL,
  `CategoryId` bigint(20) DEFAULT NULL,
  `BrandId` bigint(20) DEFAULT NULL,
  `ProductName` varchar(100) DEFAULT NULL,
  `SaleCount` int(8) DEFAULT NULL,
  `Price` decimal(18,2) DEFAULT NULL,
  `Comments` int(5) DEFAULT NULL,
  `AddedDate` datetime DEFAULT NULL,
  `QueryWeight` char(10) DEFAULT NULL,
  `ImagePath` varchar(100) DEFAULT NULL,
  `ShopName` varchar(100) DEFAULT NULL,
  `Address` varchar(100) DEFAULT NULL,
  `EndDate` datetime DEFAULT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;