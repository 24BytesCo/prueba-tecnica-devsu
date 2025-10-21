--
-- PostgreSQL database dump
--

-- Dumped from database version 14.3 (Debian 14.3-1.pgdg110+1)
-- Dumped by pg_dump version 14.3 (Debian 14.3-1.pgdg110+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

ALTER TABLE ONLY public.personas DROP CONSTRAINT "FK_personas_tipos_documento_identidad_TipoDocumentoIdentidadId";
ALTER TABLE ONLY public.personas DROP CONSTRAINT "FK_personas_generos_GeneroId";
ALTER TABLE ONLY public.movimientos DROP CONSTRAINT "FK_movimientos_tipos_movimiento_TipoId";
ALTER TABLE ONLY public.movimientos DROP CONSTRAINT "FK_movimientos_cuentas_CuentaId";
ALTER TABLE ONLY public.cuentas DROP CONSTRAINT "FK_cuentas_tipos_cuenta_TipoCuentaId";
ALTER TABLE ONLY public.cuentas DROP CONSTRAINT "FK_cuentas_clientes_ClienteId";
ALTER TABLE ONLY public.clientes DROP CONSTRAINT "FK_clientes_personas_PersonaId";
ALTER TABLE ONLY public."AspNetUserTokens" DROP CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId";
ALTER TABLE ONLY public."AspNetUserRoles" DROP CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId";
ALTER TABLE ONLY public."AspNetUserRoles" DROP CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId";
ALTER TABLE ONLY public."AspNetUserLogins" DROP CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId";
ALTER TABLE ONLY public."AspNetUserClaims" DROP CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId";
ALTER TABLE ONLY public."AspNetRoleClaims" DROP CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId";
DROP INDEX public."UserNameIndex";
DROP INDEX public."RoleNameIndex";
DROP INDEX public."IX_tipos_movimiento_Codigo";
DROP INDEX public."IX_tipos_documento_identidad_Codigo";
DROP INDEX public."IX_tipos_cuenta_Codigo";
DROP INDEX public."IX_personas_TipoDocumentoIdentidadId_NumeroDocumento";
DROP INDEX public."IX_personas_GeneroId";
DROP INDEX public."IX_personas_Email";
DROP INDEX public."IX_movimientos_TipoId";
DROP INDEX public."IX_movimientos_CuentaId_IdempotencyKey";
DROP INDEX public."IX_movimientos_CuentaId_Fecha";
DROP INDEX public."IX_generos_Codigo";
DROP INDEX public."IX_cuentas_TipoCuentaId";
DROP INDEX public."IX_cuentas_NumeroCuenta";
DROP INDEX public."IX_cuentas_ClienteId";
DROP INDEX public."IX_clientes_PersonaId";
DROP INDEX public."IX_clientes_AppUserId";
DROP INDEX public."IX_AspNetUserRoles_RoleId";
DROP INDEX public."IX_AspNetUserLogins_UserId";
DROP INDEX public."IX_AspNetUserClaims_UserId";
DROP INDEX public."IX_AspNetRoleClaims_RoleId";
DROP INDEX public."EmailIndex";
ALTER TABLE ONLY public.tipos_movimiento DROP CONSTRAINT "PK_tipos_movimiento";
ALTER TABLE ONLY public.tipos_documento_identidad DROP CONSTRAINT "PK_tipos_documento_identidad";
ALTER TABLE ONLY public.tipos_cuenta DROP CONSTRAINT "PK_tipos_cuenta";
ALTER TABLE ONLY public.personas DROP CONSTRAINT "PK_personas";
ALTER TABLE ONLY public.movimientos DROP CONSTRAINT "PK_movimientos";
ALTER TABLE ONLY public.generos DROP CONSTRAINT "PK_generos";
ALTER TABLE ONLY public.cuentas DROP CONSTRAINT "PK_cuentas";
ALTER TABLE ONLY public.clientes DROP CONSTRAINT "PK_clientes";
ALTER TABLE ONLY public."__EFMigrationsHistory" DROP CONSTRAINT "PK___EFMigrationsHistory";
ALTER TABLE ONLY public."AspNetUsers" DROP CONSTRAINT "PK_AspNetUsers";
ALTER TABLE ONLY public."AspNetUserTokens" DROP CONSTRAINT "PK_AspNetUserTokens";
ALTER TABLE ONLY public."AspNetUserRoles" DROP CONSTRAINT "PK_AspNetUserRoles";
ALTER TABLE ONLY public."AspNetUserLogins" DROP CONSTRAINT "PK_AspNetUserLogins";
ALTER TABLE ONLY public."AspNetUserClaims" DROP CONSTRAINT "PK_AspNetUserClaims";
ALTER TABLE ONLY public."AspNetRoles" DROP CONSTRAINT "PK_AspNetRoles";
ALTER TABLE ONLY public."AspNetRoleClaims" DROP CONSTRAINT "PK_AspNetRoleClaims";
DROP TABLE public.tipos_movimiento;
DROP TABLE public.tipos_documento_identidad;
DROP TABLE public.tipos_cuenta;
DROP TABLE public.personas;
DROP TABLE public.movimientos;
DROP TABLE public.generos;
DROP TABLE public.cuentas;
DROP TABLE public.clientes;
DROP TABLE public."__EFMigrationsHistory";
DROP TABLE public."AspNetUsers";
DROP TABLE public."AspNetUserTokens";
DROP TABLE public."AspNetUserRoles";
DROP TABLE public."AspNetUserLogins";
DROP TABLE public."AspNetUserClaims";
DROP TABLE public."AspNetRoles";
DROP TABLE public."AspNetRoleClaims";
SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: AspNetRoleClaims; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetRoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


ALTER TABLE public."AspNetRoleClaims" OWNER TO admin;

--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

ALTER TABLE public."AspNetRoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetRoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetRoles; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetRoles" (
    "Id" uuid NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);


ALTER TABLE public."AspNetRoles" OWNER TO admin;

--
-- Name: AspNetUserClaims; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetUserClaims" (
    "Id" integer NOT NULL,
    "UserId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);


ALTER TABLE public."AspNetUserClaims" OWNER TO admin;

--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE; Schema: public; Owner: admin
--

ALTER TABLE public."AspNetUserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."AspNetUserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);


--
-- Name: AspNetUserLogins; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetUserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" uuid NOT NULL
);


ALTER TABLE public."AspNetUserLogins" OWNER TO admin;

--
-- Name: AspNetUserRoles; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetUserRoles" (
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL
);


ALTER TABLE public."AspNetUserRoles" OWNER TO admin;

--
-- Name: AspNetUserTokens; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetUserTokens" (
    "UserId" uuid NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text
);


ALTER TABLE public."AspNetUserTokens" OWNER TO admin;

--
-- Name: AspNetUsers; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."AspNetUsers" (
    "Id" uuid NOT NULL,
    "DisplayName" text,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL
);


ALTER TABLE public."AspNetUsers" OWNER TO admin;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO admin;

--
-- Name: clientes; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.clientes (
    "Id" uuid NOT NULL,
    "PasswordHash" character varying(256),
    "Estado" boolean DEFAULT true NOT NULL,
    "AppUserId" uuid,
    "PersonaId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.clientes OWNER TO admin;

--
-- Name: cuentas; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.cuentas (
    "Id" uuid NOT NULL,
    "NumeroCuenta" character varying(30) NOT NULL,
    "TipoCuentaId" uuid NOT NULL,
    "ClienteId" uuid NOT NULL,
    "SaldoInicial" numeric(18,2) NOT NULL,
    "SaldoActual" numeric(18,2) NOT NULL,
    "Estado" character varying(12) NOT NULL,
    "FechaApertura" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.cuentas OWNER TO admin;

--
-- Name: generos; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.generos (
    "Id" uuid NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.generos OWNER TO admin;

--
-- Name: movimientos; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.movimientos (
    "Id" uuid NOT NULL,
    "CuentaId" uuid NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "TipoId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "SaldoPrevio" numeric(18,2) NOT NULL,
    "SaldoPosterior" numeric(18,2) NOT NULL,
    "Descripcion" character varying(250),
    "CreatedBy" character varying(100),
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IdempotencyKey" character varying(100)
);


ALTER TABLE public.movimientos OWNER TO admin;

--
-- Name: personas; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.personas (
    "Id" uuid NOT NULL,
    "Nombres" character varying(120) NOT NULL,
    "Apellidos" character varying(120) NOT NULL,
    "Edad" integer NOT NULL,
    "GeneroId" uuid NOT NULL,
    "TipoDocumentoIdentidadId" uuid NOT NULL,
    "NumeroDocumento" character varying(50) NOT NULL,
    "Direccion" character varying(200),
    "Telefono" character varying(50),
    "Email" character varying(200),
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.personas OWNER TO admin;

--
-- Name: tipos_cuenta; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.tipos_cuenta (
    "Id" uuid NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.tipos_cuenta OWNER TO admin;

--
-- Name: tipos_documento_identidad; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.tipos_documento_identidad (
    "Id" uuid NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.tipos_documento_identidad OWNER TO admin;

--
-- Name: tipos_movimiento; Type: TABLE; Schema: public; Owner: admin
--

CREATE TABLE public.tipos_movimiento (
    "Id" uuid NOT NULL,
    "Codigo" character varying(20) NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Activo" boolean DEFAULT true NOT NULL,
    "CreatedBy" character varying(100),
    "CreatedAt" timestamp with time zone DEFAULT CURRENT_TIMESTAMP NOT NULL,
    "UpdatedAt" timestamp with time zone
);


ALTER TABLE public.tipos_movimiento OWNER TO admin;

--
-- Data for Name: AspNetRoleClaims; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetRoleClaims" ("Id", "RoleId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetRoles; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetRoles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp") FROM stdin;
019a0442-a50b-78d5-85eb-b14bee628c00	Admin	ADMIN	\N
019a0442-a51c-7ada-89f5-2322e7c002e0	User	USER	\N
\.


--
-- Data for Name: AspNetUserClaims; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetUserClaims" ("Id", "UserId", "ClaimType", "ClaimValue") FROM stdin;
\.


--
-- Data for Name: AspNetUserLogins; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetUserLogins" ("LoginProvider", "ProviderKey", "ProviderDisplayName", "UserId") FROM stdin;
\.


--
-- Data for Name: AspNetUserRoles; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetUserRoles" ("UserId", "RoleId") FROM stdin;
16c22d4c-4c2c-4740-b4ec-e785c0a2008d	019a0442-a50b-78d5-85eb-b14bee628c00
08124f99-7f47-4a1a-8c4f-e2a72c50a065	019a0442-a51c-7ada-89f5-2322e7c002e0
90e187fb-276a-4b34-a103-ee6ec1e3157d	019a0442-a51c-7ada-89f5-2322e7c002e0
6f3ea1db-7606-4a46-84c3-71c05e47081f	019a0442-a51c-7ada-89f5-2322e7c002e0
\.


--
-- Data for Name: AspNetUserTokens; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value") FROM stdin;
16c22d4c-4c2c-4740-b4ec-e785c0a2008d	Bancalite	RefreshToken	79c6eb1f110948a8865af0f5a362c2dc.638972097247187548
\.


--
-- Data for Name: AspNetUsers; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."AspNetUsers" ("Id", "DisplayName", "UserName", "NormalizedUserName", "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount") FROM stdin;
16c22d4c-4c2c-4740-b4ec-e785c0a2008d	\N	admin	ADMIN	admin@gmail.com	ADMIN@GMAIL.COM	t	AQAAAAIAAYagAAAAEOMQ0jSWe6z4ojMUPRKccOwmZ3PPeTspMDYn4v8zW8fIdIHXqpckRXm3fQWAqnl4Lg==	ZQVCFOO25RAPXZNJZOFLVLCGXWG2V7BZ	adabcb7b-fc40-46f2-9980-83585eafc2ce	\N	f	f	\N	t	0
08124f99-7f47-4a1a-8c4f-e2a72c50a065	Maria Isabela Dorado Salazar	1061087439	1061087439	maria@gmail.com	MARIA@GMAIL.COM	f	AQAAAAIAAYagAAAAEN/vcOUqUcYvUij3c1gECkHYEpjRCcPo4WYAHbNQzzUtQhVnMtbVeg/coEYvppxL/A==	DBOGIX3XG52LDJOVWSB32UNA7A7BD2W4	9c761c37-6e53-41c3-befb-fd49ee931dc0	\N	f	f	\N	t	0
90e187fb-276a-4b34-a103-ee6ec1e3157d	Mireida Muñoz Cerón	3455778	3455778	mireida@gmail.com	MIREIDA@GMAIL.COM	f	AQAAAAIAAYagAAAAEKUBpvTrHTyAiH7chZR2pzJZpFi397TI/kYtPZn/43IRQCN1h71KnWKrdUO1hZS/+A==	LST4JKPJ73SDY43QF7HPQQY2YWGOHBI7	1a1e35ec-8f63-4a1a-b07a-24f3a8d39f99	\N	f	f	\N	t	0
6f3ea1db-7606-4a46-84c3-71c05e47081f	Orlando Fernandez Dorado	234234234	234234234	Orlando@gmail.com	ORLANDO@GMAIL.COM	f	AQAAAAIAAYagAAAAEE6Je0Q33uDeX9SrA/sN+Ac1KLBteBM6I4zQL+9Ctpxb3sBC1SR/oTHhsZzT6Li1iQ==	TQC42FCINMJTKCVRU5VIV6ZM3GD3FTHP	3b4707cb-6c42-405a-bb4e-e51c6f0227f3	\N	f	f	\N	t	0
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20251017025055_SyncModel_9_0_0	9.0.0
20251017142347_AddMovimientoIdempotencyKey	9.0.0
20251020071844_SyncModel_Current	9.0.0
\.


--
-- Data for Name: clientes; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.clientes ("Id", "PasswordHash", "Estado", "AppUserId", "PersonaId", "CreatedAt", "UpdatedAt") FROM stdin;
3421eb0e-276b-4e26-9bd8-0a6d6d139495	AQAAAAIAAYagAAAAEOMQ0jSWe6z4ojMUPRKccOwmZ3PPeTspMDYn4v8zW8fIdIHXqpckRXm3fQWAqnl4Lg==	t	16c22d4c-4c2c-4740-b4ec-e785c0a2008d	dbb53c7a-bc64-4b0c-80e0-8d598b0c686b	2025-10-21 00:54:28.060723+00	\N
a7f52ee2-6d01-420a-979a-a1d3d0fcdff8	AQAAAAIAAYagAAAAEN/vcOUqUcYvUij3c1gECkHYEpjRCcPo4WYAHbNQzzUtQhVnMtbVeg/coEYvppxL/A==	f	08124f99-7f47-4a1a-8c4f-e2a72c50a065	e08d72c3-22b2-4f5e-b5db-272257f96f9d	2025-10-21 00:56:22.298351+00	2025-10-21 00:58:27.719328+00
27cb3a0d-08f8-4210-97d9-f8d7944c74c1	AQAAAAIAAYagAAAAEKUBpvTrHTyAiH7chZR2pzJZpFi397TI/kYtPZn/43IRQCN1h71KnWKrdUO1hZS/+A==	t	90e187fb-276a-4b34-a103-ee6ec1e3157d	b783c289-7e5f-455e-86a5-a412be7b96c9	2025-10-21 01:04:26.191517+00	\N
707e2102-1386-458f-bc65-edd61b1b69de	AQAAAAIAAYagAAAAEE6Je0Q33uDeX9SrA/sN+Ac1KLBteBM6I4zQL+9Ctpxb3sBC1SR/oTHhsZzT6Li1iQ==	t	6f3ea1db-7606-4a46-84c3-71c05e47081f	265044ba-dd96-4417-a6b5-63e169bbec2e	2025-10-21 01:05:17.496682+00	\N
\.


--
-- Data for Name: cuentas; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.cuentas ("Id", "NumeroCuenta", "TipoCuentaId", "ClienteId", "SaldoInicial", "SaldoActual", "Estado", "FechaApertura", "CreatedAt", "UpdatedAt") FROM stdin;
a5f9a4c6-f3a6-477b-9248-875b0ceda132	3967-4087-4907	eced8667-d2ef-4503-9d46-8094e219d567	a7f52ee2-6d01-420a-979a-a1d3d0fcdff8	200.00	160.00	Inactiva	2025-10-21 00:56:43.233971+00	2025-10-21 00:56:43.233901+00	\N
d2cb5298-5dde-4f55-99a3-da62e1a33cdc	3957-7007-1224	eced8667-d2ef-4503-9d46-8094e219d567	3421eb0e-276b-4e26-9bd8-0a6d6d139495	741.00	741.00	Activa	2025-10-21 01:06:13.079812+00	2025-10-21 01:06:13.079806+00	\N
858684ea-b310-4527-bf07-c16c08f55d3a	2078-2125-7205	eced8667-d2ef-4503-9d46-8094e219d567	27cb3a0d-08f8-4210-97d9-f8d7944c74c1	346.00	331.00	Activa	2025-10-21 01:05:54.375758+00	2025-10-21 01:05:54.375741+00	\N
82f937bc-d810-4244-a6f7-614c1da579f1	1331-7684-0794	a4f58b55-175a-48dd-b290-48b5df7b54ef	707e2102-1386-458f-bc65-edd61b1b69de	450.00	437.00	Activa	2025-10-21 01:05:39.597502+00	2025-10-21 01:05:39.597496+00	\N
\.


--
-- Data for Name: generos; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.generos ("Id", "Codigo", "Nombre", "Activo", "CreatedAt", "UpdatedAt") FROM stdin;
4e1efb18-5e3d-46e8-9a13-72338c39ac49	M	Masculino	t	2025-10-21 00:54:27.644776+00	\N
7a4a4366-05db-466f-a866-030109914649	X	No binario	t	2025-10-21 00:54:27.646583+00	\N
944c078f-bc43-4312-8436-5f47940e73da	F	Femenino	t	2025-10-21 00:54:27.646578+00	\N
\.


--
-- Data for Name: movimientos; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.movimientos ("Id", "CuentaId", "Fecha", "TipoId", "Monto", "SaldoPrevio", "SaldoPosterior", "Descripcion", "CreatedBy", "CreatedAt", "UpdatedAt", "IdempotencyKey") FROM stdin;
73a2398d-bac1-4bf2-8986-4bd4e5a363ba	a5f9a4c6-f3a6-477b-9248-875b0ceda132	2025-10-21 00:57:12.497737+00	541d84e9-8704-4723-be08-83985e913530	10.00	200.00	190.00	\N	admin@gmail.com	2025-10-21 00:57:12.497643+00	\N	\N
bd526a4a-c1bc-4b87-a0c9-c246176d55d7	a5f9a4c6-f3a6-477b-9248-875b0ceda132	2025-10-21 00:57:20.52565+00	df67f0ab-788e-4dd1-a7e7-d18d74f128ef	20.00	190.00	210.00	\N	admin@gmail.com	2025-10-21 00:57:20.525639+00	\N	\N
69775627-d3ef-47fb-8526-185da5ee9180	a5f9a4c6-f3a6-477b-9248-875b0ceda132	2025-10-21 00:57:29.466155+00	541d84e9-8704-4723-be08-83985e913530	50.00	210.00	160.00	\N	admin@gmail.com	2025-10-21 00:57:29.466145+00	\N	\N
9b3b56ae-1c65-433b-9a18-89668bdd7743	82f937bc-d810-4244-a6f7-614c1da579f1	2025-10-21 01:06:30.753022+00	df67f0ab-788e-4dd1-a7e7-d18d74f128ef	41.00	450.00	491.00	\N	admin@gmail.com	2025-10-21 01:06:30.753016+00	\N	\N
223e2557-2a38-4ea2-95a2-6f64f59ee7ce	82f937bc-d810-4244-a6f7-614c1da579f1	2025-10-21 01:06:40.763968+00	541d84e9-8704-4723-be08-83985e913530	10.00	491.00	481.00	\N	admin@gmail.com	2025-10-21 01:06:40.763951+00	\N	\N
d5605637-e7f5-4d61-b244-701e1f7fe068	858684ea-b310-4527-bf07-c16c08f55d3a	2025-10-21 01:07:02.271401+00	541d84e9-8704-4723-be08-83985e913530	15.00	346.00	331.00	\N	admin@gmail.com	2025-10-21 01:07:02.271396+00	\N	\N
c03d01aa-cccb-4532-87ca-91eeee90def8	858684ea-b310-4527-bf07-c16c08f55d3a	2025-10-21 01:07:09.408465+00	df67f0ab-788e-4dd1-a7e7-d18d74f128ef	10.00	331.00	341.00	\N	admin@gmail.com	2025-10-21 01:07:09.408459+00	\N	\N
2a207c27-19af-4713-ba36-7f6228e2a4b2	858684ea-b310-4527-bf07-c16c08f55d3a	2025-10-21 01:07:16.701116+00	df67f0ab-788e-4dd1-a7e7-d18d74f128ef	14.00	341.00	355.00	\N	admin@gmail.com	2025-10-21 01:07:16.701111+00	\N	\N
83726476-abec-4592-8531-b2a3ee1c93b5	858684ea-b310-4527-bf07-c16c08f55d3a	2025-10-21 01:07:26.602778+00	541d84e9-8704-4723-be08-83985e913530	24.00	355.00	331.00	\N	admin@gmail.com	2025-10-21 01:07:26.602771+00	\N	\N
7d830500-61db-47e3-ac38-b84c11cf3a23	82f937bc-d810-4244-a6f7-614c1da579f1	2025-10-21 01:07:38.460581+00	df67f0ab-788e-4dd1-a7e7-d18d74f128ef	10.00	481.00	491.00	\N	admin@gmail.com	2025-10-21 01:07:38.460576+00	\N	\N
1c0ef695-b8ac-4ebc-8929-92dcb112c026	82f937bc-d810-4244-a6f7-614c1da579f1	2025-10-21 01:07:46.175236+00	541d84e9-8704-4723-be08-83985e913530	54.00	491.00	437.00	\N	admin@gmail.com	2025-10-21 01:07:46.175231+00	\N	\N
\.


--
-- Data for Name: personas; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.personas ("Id", "Nombres", "Apellidos", "Edad", "GeneroId", "TipoDocumentoIdentidadId", "NumeroDocumento", "Direccion", "Telefono", "Email", "CreatedAt", "UpdatedAt") FROM stdin;
dbb53c7a-bc64-4b0c-80e0-8d598b0c686b	Admin	System	30	4e1efb18-5e3d-46e8-9a13-72338c39ac49	517856a4-8e46-4a7f-861e-90b87551db18	ADM-0001	\N	\N	admin@gmail.com	2025-10-21 00:54:28.041355+00	\N
e08d72c3-22b2-4f5e-b5db-272257f96f9d	Maria Isabela	Dorado Salazar	18	944c078f-bc43-4312-8436-5f47940e73da	ec79bc34-5825-4ecf-8cfd-3b927dab63b6	1061087439	\N	\N	maria@gmail.com	2025-10-21 00:56:22.18968+00	\N
b783c289-7e5f-455e-86a5-a412be7b96c9	Mireida	Muñoz Cerón	18	944c078f-bc43-4312-8436-5f47940e73da	ec79bc34-5825-4ecf-8cfd-3b927dab63b6	3455778	\N	\N	mireida@gmail.com	2025-10-21 01:04:26.086304+00	\N
265044ba-dd96-4417-a6b5-63e169bbec2e	Orlando	Fernandez Dorado	44	4e1efb18-5e3d-46e8-9a13-72338c39ac49	ec79bc34-5825-4ecf-8cfd-3b927dab63b6	234234234	\N	\N	Orlando@gmail.com	2025-10-21 01:05:17.3398+00	\N
\.


--
-- Data for Name: tipos_cuenta; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.tipos_cuenta ("Id", "Codigo", "Nombre", "Activo", "CreatedAt", "UpdatedAt") FROM stdin;
67846746-2aeb-44d6-a79b-de9313f65297	PLZ	Plazo Fijo	t	2025-10-21 00:54:27.707725+00	\N
a4f58b55-175a-48dd-b290-48b5df7b54ef	COR	Corriente	t	2025-10-21 00:54:27.707723+00	\N
eced8667-d2ef-4503-9d46-8094e219d567	AHO	Ahorros	t	2025-10-21 00:54:27.707593+00	\N
\.


--
-- Data for Name: tipos_documento_identidad; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.tipos_documento_identidad ("Id", "Codigo", "Nombre", "Activo", "CreatedAt", "UpdatedAt") FROM stdin;
517856a4-8e46-4a7f-861e-90b87551db18	PAS	Pasaporte	t	2025-10-21 00:54:27.6974+00	\N
b7b202eb-9bd0-4471-9bcf-7630d462add7	CE	Cédula de Extranjería	t	2025-10-21 00:54:27.697403+00	\N
ec79bc34-5825-4ecf-8cfd-3b927dab63b6	CC	Cédula de Ciudadanía	t	2025-10-21 00:54:27.697269+00	\N
\.


--
-- Data for Name: tipos_movimiento; Type: TABLE DATA; Schema: public; Owner: admin
--

COPY public.tipos_movimiento ("Id", "Codigo", "Nombre", "Activo", "CreatedBy", "CreatedAt", "UpdatedAt") FROM stdin;
541d84e9-8704-4723-be08-83985e913530	DEB	Débito	t	\N	2025-10-21 00:54:27.716903+00	\N
df67f0ab-788e-4dd1-a7e7-d18d74f128ef	CRE	Crédito	t	\N	2025-10-21 00:54:27.71704+00	\N
\.


--
-- Name: AspNetRoleClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public."AspNetRoleClaims_Id_seq"', 1, false);


--
-- Name: AspNetUserClaims_Id_seq; Type: SEQUENCE SET; Schema: public; Owner: admin
--

SELECT pg_catalog.setval('public."AspNetUserClaims_Id_seq"', 1, false);


--
-- Name: AspNetRoleClaims PK_AspNetRoleClaims; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetRoles PK_AspNetRoles; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetRoles"
    ADD CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id");


--
-- Name: AspNetUserClaims PK_AspNetUserClaims; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id");


--
-- Name: AspNetUserLogins PK_AspNetUserLogins; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");


--
-- Name: AspNetUserRoles PK_AspNetUserRoles; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId");


--
-- Name: AspNetUserTokens PK_AspNetUserTokens; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");


--
-- Name: AspNetUsers PK_AspNetUsers; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUsers"
    ADD CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: clientes PK_clientes; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.clientes
    ADD CONSTRAINT "PK_clientes" PRIMARY KEY ("Id");


--
-- Name: cuentas PK_cuentas; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.cuentas
    ADD CONSTRAINT "PK_cuentas" PRIMARY KEY ("Id");


--
-- Name: generos PK_generos; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.generos
    ADD CONSTRAINT "PK_generos" PRIMARY KEY ("Id");


--
-- Name: movimientos PK_movimientos; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.movimientos
    ADD CONSTRAINT "PK_movimientos" PRIMARY KEY ("Id");


--
-- Name: personas PK_personas; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.personas
    ADD CONSTRAINT "PK_personas" PRIMARY KEY ("Id");


--
-- Name: tipos_cuenta PK_tipos_cuenta; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.tipos_cuenta
    ADD CONSTRAINT "PK_tipos_cuenta" PRIMARY KEY ("Id");


--
-- Name: tipos_documento_identidad PK_tipos_documento_identidad; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.tipos_documento_identidad
    ADD CONSTRAINT "PK_tipos_documento_identidad" PRIMARY KEY ("Id");


--
-- Name: tipos_movimiento PK_tipos_movimiento; Type: CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.tipos_movimiento
    ADD CONSTRAINT "PK_tipos_movimiento" PRIMARY KEY ("Id");


--
-- Name: EmailIndex; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "EmailIndex" ON public."AspNetUsers" USING btree ("NormalizedEmail");


--
-- Name: IX_AspNetRoleClaims_RoleId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON public."AspNetRoleClaims" USING btree ("RoleId");


--
-- Name: IX_AspNetUserClaims_UserId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_AspNetUserClaims_UserId" ON public."AspNetUserClaims" USING btree ("UserId");


--
-- Name: IX_AspNetUserLogins_UserId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_AspNetUserLogins_UserId" ON public."AspNetUserLogins" USING btree ("UserId");


--
-- Name: IX_AspNetUserRoles_RoleId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_AspNetUserRoles_RoleId" ON public."AspNetUserRoles" USING btree ("RoleId");


--
-- Name: IX_clientes_AppUserId; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_clientes_AppUserId" ON public.clientes USING btree ("AppUserId") WHERE ("AppUserId" IS NOT NULL);


--
-- Name: IX_clientes_PersonaId; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_clientes_PersonaId" ON public.clientes USING btree ("PersonaId");


--
-- Name: IX_cuentas_ClienteId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_cuentas_ClienteId" ON public.cuentas USING btree ("ClienteId");


--
-- Name: IX_cuentas_NumeroCuenta; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_cuentas_NumeroCuenta" ON public.cuentas USING btree ("NumeroCuenta");


--
-- Name: IX_cuentas_TipoCuentaId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_cuentas_TipoCuentaId" ON public.cuentas USING btree ("TipoCuentaId");


--
-- Name: IX_generos_Codigo; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_generos_Codigo" ON public.generos USING btree ("Codigo");


--
-- Name: IX_movimientos_CuentaId_Fecha; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_movimientos_CuentaId_Fecha" ON public.movimientos USING btree ("CuentaId", "Fecha");


--
-- Name: IX_movimientos_CuentaId_IdempotencyKey; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_movimientos_CuentaId_IdempotencyKey" ON public.movimientos USING btree ("CuentaId", "IdempotencyKey") WHERE ("IdempotencyKey" IS NOT NULL);


--
-- Name: IX_movimientos_TipoId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_movimientos_TipoId" ON public.movimientos USING btree ("TipoId");


--
-- Name: IX_personas_Email; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_personas_Email" ON public.personas USING btree ("Email") WHERE ("Email" IS NOT NULL);


--
-- Name: IX_personas_GeneroId; Type: INDEX; Schema: public; Owner: admin
--

CREATE INDEX "IX_personas_GeneroId" ON public.personas USING btree ("GeneroId");


--
-- Name: IX_personas_TipoDocumentoIdentidadId_NumeroDocumento; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_personas_TipoDocumentoIdentidadId_NumeroDocumento" ON public.personas USING btree ("TipoDocumentoIdentidadId", "NumeroDocumento");


--
-- Name: IX_tipos_cuenta_Codigo; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_tipos_cuenta_Codigo" ON public.tipos_cuenta USING btree ("Codigo");


--
-- Name: IX_tipos_documento_identidad_Codigo; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_tipos_documento_identidad_Codigo" ON public.tipos_documento_identidad USING btree ("Codigo");


--
-- Name: IX_tipos_movimiento_Codigo; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "IX_tipos_movimiento_Codigo" ON public.tipos_movimiento USING btree ("Codigo");


--
-- Name: RoleNameIndex; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "RoleNameIndex" ON public."AspNetRoles" USING btree ("NormalizedName");


--
-- Name: UserNameIndex; Type: INDEX; Schema: public; Owner: admin
--

CREATE UNIQUE INDEX "UserNameIndex" ON public."AspNetUsers" USING btree ("NormalizedUserName");


--
-- Name: AspNetRoleClaims FK_AspNetRoleClaims_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetRoleClaims"
    ADD CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserClaims FK_AspNetUserClaims_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserClaims"
    ADD CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserLogins FK_AspNetUserLogins_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserLogins"
    ADD CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetRoles_RoleId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."AspNetRoles"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserRoles FK_AspNetUserRoles_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserRoles"
    ADD CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: AspNetUserTokens FK_AspNetUserTokens_AspNetUsers_UserId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public."AspNetUserTokens"
    ADD CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES public."AspNetUsers"("Id") ON DELETE CASCADE;


--
-- Name: clientes FK_clientes_personas_PersonaId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.clientes
    ADD CONSTRAINT "FK_clientes_personas_PersonaId" FOREIGN KEY ("PersonaId") REFERENCES public.personas("Id") ON DELETE CASCADE;


--
-- Name: cuentas FK_cuentas_clientes_ClienteId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.cuentas
    ADD CONSTRAINT "FK_cuentas_clientes_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES public.clientes("Id") ON DELETE CASCADE;


--
-- Name: cuentas FK_cuentas_tipos_cuenta_TipoCuentaId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.cuentas
    ADD CONSTRAINT "FK_cuentas_tipos_cuenta_TipoCuentaId" FOREIGN KEY ("TipoCuentaId") REFERENCES public.tipos_cuenta("Id") ON DELETE RESTRICT;


--
-- Name: movimientos FK_movimientos_cuentas_CuentaId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.movimientos
    ADD CONSTRAINT "FK_movimientos_cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES public.cuentas("Id") ON DELETE CASCADE;


--
-- Name: movimientos FK_movimientos_tipos_movimiento_TipoId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.movimientos
    ADD CONSTRAINT "FK_movimientos_tipos_movimiento_TipoId" FOREIGN KEY ("TipoId") REFERENCES public.tipos_movimiento("Id") ON DELETE RESTRICT;


--
-- Name: personas FK_personas_generos_GeneroId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.personas
    ADD CONSTRAINT "FK_personas_generos_GeneroId" FOREIGN KEY ("GeneroId") REFERENCES public.generos("Id") ON DELETE RESTRICT;


--
-- Name: personas FK_personas_tipos_documento_identidad_TipoDocumentoIdentidadId; Type: FK CONSTRAINT; Schema: public; Owner: admin
--

ALTER TABLE ONLY public.personas
    ADD CONSTRAINT "FK_personas_tipos_documento_identidad_TipoDocumentoIdentidadId" FOREIGN KEY ("TipoDocumentoIdentidadId") REFERENCES public.tipos_documento_identidad("Id") ON DELETE RESTRICT;


--
-- PostgreSQL database dump complete
--

