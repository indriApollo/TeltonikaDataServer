CREATE SCHEMA telematics AUTHORIZATION postgres;

CREATE TABLE telematics.device (
	dev_id serial4 NOT NULL,
	dev_imei bpchar(15) NOT NULL,
	CONSTRAINT device_pk PRIMARY KEY (dev_id),
	CONSTRAINT device_un UNIQUE (dev_imei)
);

CREATE TABLE telematics.gps_position (
	gps_id bigserial NOT NULL,
	gps_latitude int4 NOT NULL,
	gps_longitude int4 NOT NULL,
	gps_altitude int2 NOT NULL,
	gps_heading int2 NOT NULL,
	gps_speed int2 NOT NULL,
	gps_timestamp_utc time NOT NULL,
	gps_dev_id int4 NOT NULL,
	CONSTRAINT gps_position_pk PRIMARY KEY (gps_id),
	CONSTRAINT gps_dev_id_fk FOREIGN KEY (gps_dev_id) REFERENCES telematics.device(dev_id)
);
