CREATE TABLE Day_table (
    day_id INTEGER,
    day_name CHAR(10),
    PRIMARY KEY(day_id)
);

INSERT INTO Day_table
VALUES (0, 'Monday');
INSERT INTO Day_table
VALUES (1, 'Tuesday');
INSERT INTO Day_table
VALUES (2, 'Wednesday');
INSERT INTO Day_table
VALUES (3, 'Thursday');
INSERT INTO Day_table
VALUES (4, 'Friday');
INSERT INTO Day_table
VALUES (5, 'Saturday');
INSERT INTO Day_table
VALUES (6, 'Sunday');

CREATE TABLE Users_table (
    user_id  CHAR(22),
    name VARCHAR(60),
    average_stars REAL,
    fans INTEGER,
    cool INTEGER,
    funny INTEGER,
    tipCount INTEGER,
    totalLikes INTEGER,
    useful INTEGER,
    user_latitude REAL,
    user_longitude REAL,
    yelping_since DATE,
    PRIMARY KEY (user_id)
);

CREATE TABLE Friends_table (
    friend_for CHAR(22),
    friend_of CHAR(22),
    FOREIGN KEY (friend_for)
       REFERENCES Users_table(user_id)
       ON DELETE CASCADE,
    FOREIGN KEY (friend_of)
        REFERENCES Users_table(user_id)
        ON DELETE CASCADE
);

CREATE TABLE Business_table (
    business_id  CHAR(22),
    name VARCHAR(100),
    city VARCHAR(30),
    state VARCHAR(30),
    zipcode INTEGER,
    address VARCHAR(100),
    latitude REAL,
    longitude REAL,
    numTips INTEGER,
    numCheckins INTEGER,
    is_open BOOLEAN,
    stars REAL,
    PRIMARY KEY (business_id)
);

CREATE TABLE Tip_table (
    tipDate TIMESTAMP,
    business_id CHAR(22),
    user_id CHAR(22),
    tipText VARCHAR(1000),
    likes INTEGER,
    PRIMARY KEY (tipDate, business_id, user_id),
    FOREIGN KEY (business_id)
        REFERENCES Business_table(business_id)
        ON DELETE CASCADE,
    FOREIGN KEY (user_id)
        REFERENCES Users_table(user_id)
        ON DELETE CASCADE
);

CREATE TABLE Categories_table (
    business_id CHAR(22),
    category_name VARCHAR(100),
    PRIMARY KEY (business_id, category_name),
    FOREIGN KEY (business_id)
        REFERENCES Business_table(business_id)
        ON DELETE CASCADE
);

CREATE TABLE Attributes_table (
    business_id CHAR(22),
    attr_name VARCHAR(30),
    attr_value VARCHAR(30),
    PRIMARY KEY (business_id, attr_name),
    FOREIGN KEY (business_id)
        REFERENCES Business_table(business_id)
        on DELETE CASCADE
);

CREATE TABLE Hours_table (
    business_id CHAR(22),
    dayofweek INTEGER,
    close TIME,
    open TIME,
    PRIMARY KEY (business_id, dayofweek),
    FOREIGN KEY (dayofweek) REFERENCES Day_table(day_id),
    FOREIGN KEY (business_id)
        REFERENCES Business_table(business_id)
        ON DELETE CASCADE
);

CREATE TABLE Checkins_table (
    business_id CHAR(22),
    year INTERVAL YEAR,
    month INTERVAL MONTH,
    day INTERVAL DAY,
    time TIME,
    PRIMARY KEY (business_id, year, month, day, time),
    FOREIGN KEY (business_id)
        REFERENCES Business_table(business_id)
        ON DELETE CASCADE
);
