CREATE TABLE PingRequest
(
	SessionIdentifier NVARCHAR(100) NOT NULL,
	PingIdentifier NVARCHAR(100) NOT NULL,
	StartTime DATETIME NOT NULL
)

CREATE TABLE PingResponse
(
	SessionIdentifier NVARCHAR(100) NOT NULL,
	PingIdentifier NVARCHAR(100) NOT NULL,
	ReceiverIdentifer NVARCHAR(100) NOT NULL,
	ReceiveTime DATETIME NOT NULL,
	Channel NVARCHAR(100) NOT NULL,
	CellularType NVARCHAR(100) NULL,
	CellularProvider NVARCHAR(100) NULL,
    CellularSignalStrength INT NULL,
    WifiProvider NVARCHAR(100) NULL,
    WifiSignalStrength  INT NULL,
    BatteryPercentage INT NULL,
    VolumePercentage  INT NULL,
)