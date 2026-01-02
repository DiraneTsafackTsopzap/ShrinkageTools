-- ============================
-- EXTENSIONS
-- ============================
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- ============================
-- TABLE: shrinkage_users
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_users (
    id          uuid PRIMARY KEY,
    user_email  varchar(50) NOT NULL,
    created_at  timestamptz NOT NULL,
    deleted_at  timestamptz,
    team_id     uuid,
    updated_by  uuid,
    updated_at  timestamptz,
    deleted_by  uuid,
    CONSTRAINT shrinkage_users_user_email_ukey UNIQUE (user_email)
);

-- ============================
-- TABLE: shrinkage_teams
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_teams (
    id               uuid PRIMARY KEY,
    created_at       timestamptz NOT NULL,
    deleted_at       timestamptz,
    team_name        varchar(50) NOT NULL,
    team_lead_ids    uuid[] NOT NULL,
    team_reference   varchar(50) NOT NULL
);

-- ============================
-- TABLE: shrinkage_user_paid_time
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_user_paid_time (
    id                     uuid PRIMARY KEY,
    created_at             timestamptz NOT NULL,
    created_by             uuid NOT NULL,
    user_id                uuid NOT NULL,
    valid_from             date NOT NULL,
    paid_time_monday       numeric NOT NULL DEFAULT 480,
    paid_time_tuesday      numeric NOT NULL DEFAULT 480,
    paid_time_wednesday    numeric NOT NULL DEFAULT 480,
    paid_time_thursday     numeric NOT NULL DEFAULT 480,
    paid_time_friday       numeric NOT NULL DEFAULT 480,
    paid_time_saturday     numeric NOT NULL DEFAULT 480,
    deleted_at             timestamptz,
    deleted_by             uuid
);

-- ============================
-- TABLE: shrinkage_user_activities
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_user_activities (
    id                   uuid PRIMARY KEY,
    created_at           timestamptz NOT NULL,
    created_by           uuid NOT NULL,
    updated_at           timestamptz,
    updated_by           uuid,
    deleted_at           timestamptz,
    deleted_by           uuid,
    user_id              uuid NOT NULL,
    team_id              uuid NOT NULL,
    started_at           timestamptz NOT NULL,
    stopped_at           timestamptz,
    activity_type        varchar(50) NOT NULL,
    activity_track_type  varchar(50) NOT NULL
);

-- ============================
-- TABLE: shrinkage_user_daily_values
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_user_daily_values (
    id              uuid PRIMARY KEY,
    created_at      timestamptz NOT NULL,
    created_by      uuid NOT NULL,
    updated_at      timestamptz,
    updated_by      uuid,
    deleted_at      timestamptz,
    deleted_by      uuid,
    paid_time       numeric NOT NULL DEFAULT 0,
    paid_time_off   numeric NOT NULL DEFAULT 0,
    overtime        numeric NOT NULL DEFAULT 0,
    vacation_time   numeric NOT NULL DEFAULT 0,
    user_id         uuid NOT NULL,
    team_id         uuid NOT NULL,
    status          varchar(50) NOT NULL,
    comment         varchar(1000),
    shrinkage_date  date NOT NULL
);

-- ============================
-- TABLE: shrinkage_user_absences
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_user_absences (
    id             uuid PRIMARY KEY,
    created_at     timestamptz NOT NULL,
    created_by     uuid NOT NULL,
    updated_at     timestamptz,
    updated_by     uuid,
    deleted_at     timestamptz,
    deleted_by     uuid,
    user_id        uuid NOT NULL,
    team_id        uuid NOT NULL,
    absence_type   varchar(50) NOT NULL,
    start_date     date NOT NULL,
    end_date       date NOT NULL
);

-- ============================
-- TABLE: shrinkage_team_public_holidays
-- ============================
CREATE TABLE IF NOT EXISTS shrinkage_team_public_holidays (
    id            uuid PRIMARY KEY,
    created_at    timestamptz NOT NULL,
    created_by    uuid NOT NULL,
    deleted_at    timestamptz,
    deleted_by    uuid,
    title         varchar(100) NOT NULL,
    affected_day  date NOT NULL,
    team_ids      uuid[] NOT NULL
);

-- ============================
-- INDEXES
-- ============================
CREATE INDEX IF NOT EXISTS idx_shrinkage_users_email_lower
    ON shrinkage_users (LOWER(user_email));

CREATE INDEX IF NOT EXISTS idx_shrinkage_paid_time_user_valid_from
    ON shrinkage_user_paid_time (user_id, valid_from DESC);

-- ============================
-- INSERTIONS INITIALES
-- ============================

-- Team: Backend Developer
INSERT INTO shrinkage_teams (
    id, created_at, deleted_at, team_name, team_lead_ids, team_reference
) VALUES (
    '7a9b2f3e-6d41-4e91-b7a1-2c1c1a8f9a01',
    NOW(), NULL,
    'Backend Developer',
    ARRAY['b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66']::uuid[],
    'BACKEND_DEV'
)
ON CONFLICT (id) DO NOTHING;

-- Team: Frontend Developer
INSERT INTO shrinkage_teams (
    id, created_at, deleted_at, team_name, team_lead_ids, team_reference
) VALUES (
    'c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2',
    NOW(), NULL,
    'Frontend Developer',
    ARRAY['b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66']::uuid[],
    'FRONTEND_DEV'
)
ON CONFLICT (id) DO NOTHING;

-- User: Dirane
INSERT INTO shrinkage_users (
    id, user_email, created_at, team_id
) VALUES (
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66',
    'diraneserges@gmail.com',
    NOW(),
    'c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2'
)
ON CONFLICT (id) DO NOTHING;

-- Paid Time: Dirane
INSERT INTO shrinkage_user_paid_time (
    id, user_id, valid_from, created_at, created_by
) VALUES (
    'e91f0c7a-3b6d-4e88-9c1f-2a5b7d6f8e99',
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66',
    CURRENT_DATE,
    NOW(),
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66'
)
ON CONFLICT (id) DO NOTHING;

--
-- ============================
-- INSERTIONS INITIALES: DAILY VALUE TEST : Tester la Methode SaveActivity il faudrai que ces valeurs soient presentes
-- ============================
INSERT INTO shrinkage_user_daily_values (
    id, created_at, created_by, user_id, team_id, shrinkage_date, status, paid_time, paid_time_off, overtime, vacation_time
) VALUES (
    'd4c2f750-71a9-44ea-a71d-9a1c9f2037dd', -- ← UUID réel ici
    NOW(),
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66', -- created_by dirane
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66', -- user_id
    'c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2', -- team_id
    '2025-12-26',
    'Pending',
    480, 0, 0, 0
) ON CONFLICT (id) DO NOTHING;


-- ============================
-- INSERTION TEST : Absence Dirane
-- ============================
INSERT INTO shrinkage_user_absences (
    id,
    created_at,
    created_by,
    user_id,
    team_id,
    absence_type,
    start_date,
    end_date,
    updated_at,
    updated_by
) VALUES (
    'cd50c6eb-1d5a-4f6b-9df4-b4e58e96d234',
    '2026-01-02T10:00:00Z',
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66',
    'b4e5c1a9-8f72-4d6b-9a1c-3e7f5d0b2a66',
    'c1f2b9d4-0c64-4c89-9d7b-8e91fcb6e7b2',
    'vacation',
    '2026-01-03',
    '2026-01-05',
    NULL,
    NULL
)
ON CONFLICT (id) DO NOTHING;
