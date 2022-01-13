USE [QuartzSample]
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Tabella che rappresenta le schedulazioni batch di processi per i vari server ******/
CREATE TABLE [dbo].[SCHEDULAZIONI_SERVIZI](
	[ID_REGOLA] [varchar](30) NOT NULL,
	[ESPRESSIONE_CRON] [varchar](50) NOT NULL,
	[NOME_BATCH] [varchar](100) NOT NULL,
	[NOME_SERVER] [varchar](100) NOT NULL,
	[PATH_PROCESSO] [varchar](100) NOT NULL,
	[PARAMETRI_PROCESSO] [varchar](500) NULL,
	[FG_ATTIVA] [bit] NOT NULL,
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
			,[PATH_PROCESSO]
			,[PARAMETRI_PROCESSO]
			,[FG_ATTIVA]
	FROM	[dbo].[SCHEDULAZIONI_SERVIZI]
	WHERE	NOME_BATCH = @nomeBatch AND NOME_SERVER = @nomeServer AND FG_ATTIVA = 1
END
GO

