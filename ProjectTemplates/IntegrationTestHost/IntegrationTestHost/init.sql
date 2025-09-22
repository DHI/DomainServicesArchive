DO
$$
BEGIN
   IF NOT EXISTS (
      SELECT FROM pg_database WHERE datname = 'JsonDocumentsTest'
   ) THEN
      CREATE DATABASE "JsonDocumentsTest";
   END IF;
END
$$;