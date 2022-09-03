
CREATE TABLE user(
	user_id INT NOT NULL AUTO_INCREMENT,
	username VARCHAR(32) UNIQUE NOT NULL,
    email VARCHAR(64) UNIQUE NOT NULL,
    password VARBINARY(16) NOT NULL,
    salt VARBINARY(16) NOT NULL,
    PRIMARY KEY (user_id)
);

CREATE TABLE settings(
	settings_id INT NOT NULL AUTO_INCREMENT,
    name VARCHAR(64) NOT NULL,
    default_value BOOL NOT NULL,
    description VARCHAR(64),
    PRIMARY KEY(settings_id)
);

CREATE TABLE user_settings(
    user_id INT NOT NULL,
	settings_id INT NOT NULL,
    value BOOL NOT NULL,
    PRIMARY KEY(settings_id, user_id),
    FOREIGN KEY (settings_id) REFERENCES settings(settings_id) 
		ON DELETE CASCADE ON UPDATE CASCADE,
	FOREIGN KEY (user_id) REFERENCES user(user_id) 
		ON DELETE CASCADE ON UPDATE CASCADE
);

create table MFA(
	user_id INT NOT NULL,
	code VARBINARY(16) NOT NULL,
    recovery_code VARBINARY(16),
    FOREIGN KEY (user_id) REFERENCES user(user_id) 
		ON DELETE CASCADE ON UPDATE CASCADE,
    PRIMARY KEY (user_id)
);

CREATE TRIGGER user_settings_generator1
AFTER INSERT 
ON user FOR EACH ROW
	INSERT INTO user_settings (user_id, settings_id, value)
    SELECT user_id, settings_id, default_value as value
	FROM user, settings
	WHERE user_id = NEW.user_id;
;

CREATE TRIGGER user_settings_generator2
AFTER INSERT 
ON settings FOR EACH ROW
	INSERT INTO user_settings (user_id, settings_id, value)
    SELECT user_id, settings_id, default_value as value
	FROM user, settings
	WHERE settings_id = NEW.settings_id;
;

INSERT INTO settings (name, default_value) VALUES ("two_factor_login", false);