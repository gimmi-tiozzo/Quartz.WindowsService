USE [QuartzSample]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Tabella che rappresenta le schedulazioni batch di processi per i vari server ******/
CREATE TABLE [dbo].[SCHEDULAZIONI_SERVIZI](
	[ID_REGOLA] [varchar](30) NOT NULL,
	[ESPRESSIONE_CRON] [varchar](1000) NOT NULL,
	[NOME_BATCH] [varchar](100) NOT NULL,
	[NOME_SERVER] [varchar](100) NOT NULL,
	[PATH_PROCESSO_ROOT] [varchar](100) NOT NULL,
	[PARAMETRI_PROCESSO] [varchar](500) NULL,
	[LISTA_PROCESSI] [varchar](1000) NOT NULL,
	[FG_ATTIVA] [bit] NOT NULL
 CONSTRAINT [PK_SCHEDULAZIONI_SERVIZI] PRIMARY KEY CLUSTERED 
(
	[ID_REGOLA] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

-- =============================================
-- Author:		Test
-- Create date: 12/01/2022
-- Description:	Store per la lettura delle schedulazioni associate ad un batch in un determinato server
-- =============================================
CREATE PROCEDURE GET_SCHEDULAZIONI
	@nomeBatch as varchar(100),
	@nomeServer as varchar(100)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT	[ID_REGOLA]
			,[ESPRESSIONE_CRON]
			,[NOME_BATCH]
			,[NOME_SERVER]
			,[PATH_PROCESSO_ROOT]
			,[PARAMETRI_PROCESSO]
			,[LISTA_PROCESSI]
			,[FG_ATTIVA]
	FROM	[dbo].[SCHEDULAZIONI_SERVIZI]
	WHERE	NOME_BATCH = @nomeBatch AND NOME_SERVER IN (@nomeServer, 'ALL_WORKER_NODES') AND FG_ATTIVA = 1
END
GO

INSERT [dbo].[SCHEDULAZIONI_SERVIZI] ([ID_REGOLA], [ESPRESSIONE_CRON], [NOME_BATCH], [NOME_SERVER], [PATH_PROCESSO_ROOT], [PARAMETRI_PROCESSO], [LISTA_PROCESSI], [FG_ATTIVA]) VALUES (N'BATCH_RULE_1', N'0/5 * 8-17 * * ?', N'QUARTZ_TEST', N'PC-TEST', N'Quartz.RootProcess.exe', N'root_param', 'Quartz.RootProcess|Quartz.ChildProcess', 1)
GO

