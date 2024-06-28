CREATE TABLE IF NOT EXISTS request_history (
    request_id VARCHAR(50) PRIMARY KEY,
    user_id VARCHAR(50) NOT NULL,
    request_type VARCHAR(50) NOT NULL,
    bucket_name VARCHAR(255) NOT NULL,
    object_name VARCHAR(255) NOT NULL,
    result_json JSONB NOT NULL,
    timestamp TIMESTAMP NOT NULL
);