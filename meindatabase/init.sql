-- ============================
-- Extensions
-- ============================
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================
-- Table: shrinkage_users
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_users
(
    id          uuid not null constraint shrinkage_users_pkey primary key,
    user_email  varchar(50) not null,
    created_at  timestamptz not null,
    deleted_at  timestamptz,
    team_id     uuid,
    updated_by  uuid,
    updated_at  timestamptz,
    deleted_by  uuid,

    CONSTRAINT shrinkage_users_user_email_ukey UNIQUE (user_email)
);

-- ⚠️ Sécurité : au cas où la colonne existerait déjà
ALTER TABLE shrinkage_users
DROP COLUMN IF EXISTS user_role;

-- ============================
-- Table: shrinkage_user_paid_time
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_user_paid_time
(
    id                     uuid not null constraint shrinkage_user_paid_time_pkey primary key,
    created_at             timestamptz not null,
    created_by             uuid not null,
    user_id                uuid not null,
    valid_from             timestamptz not null,

    paid_time_monday       numeric not null default 480,
    paid_time_tuesday      numeric not null default 480,
    paid_time_wednesday    numeric not null default 480,
    paid_time_thursday     numeric not null default 480,
    paid_time_friday       numeric not null default 480,
    paid_time_saturday     numeric not null default 480,

    deleted_at             timestamptz,
    deleted_by             uuid
);

CREATE TABLE IF NOT EXISTS shrinkage_teams
(
    id               uuid not null constraint shrinkage_teams_pkey primary key,
    created_at       timestamptz not null,
    deleted_at       timestamptz,
    team_name        varchar(50) not null,
    team_lead_ids    uuid[] not null,
    legal_entity_id  uuid not null,
    unit             varchar(2) not null,
    team_reference   varchar(50) not null
);



-- Insertion des Teams ici la :
-- Backend Developer
INSERT INTO shrinkage_teams (
    id,
    created_at,
    deleted_at,
    team_name,
    team_lead_ids,
    legal_entity_id,
    unit,
    team_reference
)
VALUES (
    '7a9b2f3e-6d41-4e91-b7a1-2c1c1a8f9a01',
    NOW(),
    NULL,
    'Backend Developer',
    ARRAY['b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66']::uuid[],
    '2d7a8c91-6f5b-4c88-a1d9-3e0b7c9f6a44',
    'BE',
    'BACKEND_DEV'
);

-- Frontend Developer
INSERT INTO shrinkage_teams (
    id,
    created_at,
    deleted_at,
    team_name,
    team_lead_ids,
    legal_entity_id,
    unit,
    team_reference
)
VALUES (
    'c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2',
    NOW(),
    NULL,
    'Frontend Developer',
    ARRAY['b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66']::uuid[],
    '2d7a8c91-6f5b-4c88-a1d9-3e0b7c9f6a44',
    'FE',
    'FRONTEND_DEV'
);


-- Insertion de L'user - Dirane est Froontend Developer
INSERT INTO shrinkage_users (
    id,
    user_email,
    created_at,
    team_id
)
VALUES (
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66',
    'diraneserges@gmail.com',
    NOW(),
    'c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2'
);

-- Insertion du Paid Time de Dirane ici
INSERT INTO shrinkage_user_paid_time (
    id,
    user_id,
    valid_from,
    created_at,
    created_by
)
VALUES (
    'e91f0c7a-3b6d-4e88-9c1f-2a5b7d6f8e99',
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66',
    NOW(),
    NOW(),
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66'
);

-- Insertion du Team Lead ici  : Dirane est TeamLead car son id figure deja ds Team_Lead_Array


-- ============================
-- Indexes
-- ============================
CREATE INDEX IF NOT EXISTS idx_shrinkage_users_email_lower
    ON shrinkage_users (LOWER(user_email));

CREATE INDEX IF NOT EXISTS idx_shrinkage_paid_time_user_valid_from
    ON shrinkage_user_paid_time (user_id, valid_from DESC);
