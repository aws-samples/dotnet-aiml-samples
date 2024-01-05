CREATE EXTENSION IF NOT EXISTS vector;
CREATE TABLE IF NOT EXISTS kb_article(id serial PRIMARY KEY,title text ,paragraph_id int ,paragraph text ,source text, embedding vector(1536))
